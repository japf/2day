using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Exchange.ActiveSync.Exceptions;
using Chartreuse.Today.Exchange.Model;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Chartreuse.Today.Exchange.Shared;

namespace Chartreuse.Today.Exchange.ActiveSync.Commands
{
    // status error code: http://msdn.microsoft.com/en-us/library/ee218647(v=exchg.80).aspx
    public class SyncCommandResult : ASResponseParserBase
    {
        private const string ReportedNoteCrumbles = "\n------------\n";

        private static readonly Regex dateTimeRegex = new Regex(@"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})T(?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2})");

        public string SyncKey { get; private set; }
        public string Status { get; private set; }

        public int ServerAddedTasks { get; private set; }
        public int ServerModifiedTasks { get; private set; }
        public int ServerDeletedTasks { get; private set; }

        public bool MoreAvailable { get; private set; }

        public IDictionary<int, string> ClientServerMapIds { get; private set; }

        public List<ServerDeletedAsset> DeletedTasks { get; private set; }

        public List<ExchangeTask> ModifiedTasks { get; private set; }

        public List<ExchangeTask> AddedTasks { get; private set; }

        public override bool RequireValidXml
        {
            get { return false; }
        }

        internal static EventHandler<EventArgs<string>> InvalidStatusFound;

        public SyncCommandResult()
        {
            this.AddedTasks = new List<ExchangeTask>();
            this.ModifiedTasks = new List<ExchangeTask>();
            this.DeletedTasks = new List<ServerDeletedAsset>();

            this.ClientServerMapIds = new Dictionary<int, string>();
        }

        protected override void ParseResponseCore(string commandName, XDocument document, WebRequestResponse response)
        {
            if (document == null)
            {
                // no changes, leave the object as is
                this.Status = "1";
                return;
            }

            if (document.Element("Sync").Elements("Status").Any())
            {
                this.Status = document.Element("Sync").Element("Status").Value;
                LogService.LogFormat("SyncCommandResult", "General status not OK : {0}", this.Status);

                // we detect and are able to react to provisioning error status code
                if (!ActiveSyncErrorHelper.IsStatusProvisioningError(this.Status, null))
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();

                    if (InvalidStatusFound != null)
                        InvalidStatusFound(this, new EventArgs<string>($"Status: {this.Status}"));
                }

                return;
            }

            var collection = document.Element("Sync").Element("Collections").Element("Collection");

            this.SyncKey = collection.Element("SyncKey").Value;
            this.Status = collection.Element("Status").Value;
            this.MoreAvailable = collection.Elements("MoreAvailable").Any();

            this.ManageServerChanges(collection);
            this.ManageClientAcknowledgement(collection, response);

            LogService.LogFormat("SyncCommandResult", "status: {0} sync key: {1} more: {2}", this.Status, this.SyncKey.TakeLast(5), this.MoreAvailable.ToString());
        }

        private void ManageServerChanges(XElement collection)
        {
            var commands = collection.Elements("Commands");
            if (commands.Any())
            {
                var commandsNode = commands.First();
                FillTasksList(commandsNode, "Add", this.AddedTasks);
                FillTasksList(commandsNode, "Change", this.ModifiedTasks);
                var deletes = commandsNode.Elements("Delete");
                if (deletes.Any())
                    this.ManageDeletedTasks(deletes);
            }
        }

        private void ManageDeletedTasks(IEnumerable<XElement> deletes)
        {
            foreach (var delete in deletes)
            {
                this.DeletedTasks.Add(new ServerDeletedAsset(delete.Element("ServerId").Value));
            }
        }

        private void ManageClientAcknowledgement(XElement collection, WebRequestResponse response)
        {
            var responses = collection.Element("Responses");
            if (responses == null)
                return;

            // tasks added
            var adds = responses.Elements("Add");
            if (adds != null)
            {
                foreach (var add in adds)
                {
                    var status = add.Element("Status").Value;

                    if (status == "1")
                    {
                        var clientId = int.Parse(add.Element("ClientId").Value);
                        var serverId = add.Element("ServerId").Value;
                        this.ServerAddedTasks++;
                        this.ClientServerMapIds.Add(clientId, serverId);
                    }
                    else
                    {
                        TryTrackEvent("Add failed status " + status, response.RequestBody);
                        if (InvalidStatusFound != null)
                            InvalidStatusFound(this, new EventArgs<string>($"Update task status failure: {status}"));
                    }
                }
            }

            // tasks changed
            var changes = responses.Elements("Change");
            if (changes != null)
            {
                foreach (var change in changes)
                {
                    var status = change.Element("Status").Value;

                    // Manage status != 1
                    if (status == "1")
                    {
                        this.ServerModifiedTasks++;
                    }
                    else
                    {
                        TryTrackEvent("Update failed status " + status, response.RequestBody);
                    }
                }
            }

            // task deleted, nothing because delete acknowledgement is not sent back
        }

        private static void TryTrackEvent(string category, string description)
        {
            if (Ioc.HasType<ITrackingManager>())
                Ioc.Resolve<ITrackingManager>().Event(TrackingSource.SyncMessage, "ActiveSync " + category, description);
        }

        private static void FillTasksList(XElement commands, string tag, List<ExchangeTask> listToFill)
        {
            var tasks =
                (from folder in commands.Elements(tag)
                 select GetExchangeTask(folder)).ToList();
            listToFill.AddRange(tasks);
        }

        public static ExchangeTask GetExchangeTask(XElement task)
        {
            try
            {
                ExchangeTask result = new ExchangeTask();
                XElement data = task.Element("ApplicationData");

                // Manage common fields
                result.Id = task.Element("ServerId").Value;

                if (data.Element("Subject") != null)
                    result.Subject = data.Element("Subject").Value;
                else
                    result.Subject = "-";

                result.Note = data.Elements("Body").Any() && data.Element("Body").Elements("Data").Any()
                    ? data.Element("Body").Element("Data").Value : null;
                if (!string.IsNullOrEmpty(result.Note) && result.Note.StartsWith(ReportedNoteCrumbles))
                {
                    result.Note = result.Note.Substring(ReportedNoteCrumbles.Length);
                    result.Note = result.Note.TrimEnd('\n');
                }
                if (string.IsNullOrWhiteSpace(result.Note))
                    result.Note = null;

                result.Due = data.Element("DueDate") != null ? GetDate(data.Element("DueDate").Value) : null;
                result.Start = data.Element("StartDate") != null ? GetDate(data.Element("StartDate").Value) : null;
                
                var categories = data.Element("Categories");
                if (categories != null)
                {
                    var category = categories.Elements("Category");
                    if (category != null && category.Any())
                        result.Category = category.First().Value;
                }
                
                if (data.Element("Complete") != null && data.Element("Complete").Value == "1")
                {
                    var applicationData = task.Element("ApplicationData");
                    if (applicationData != null)
                    {
                        var dateCompleted = applicationData.Element("DateCompleted");
                        if (dateCompleted != null && dateCompleted.Value != null)
                        {
                            result.Completed = GetDate(dateCompleted.Value);
                        }
                    }

                    // ActiveSync specification specify that is "Complete" is set to 1, a DateCompleted must be included:
                    // "The DateCompleted element MUST be included in the response if the Complete element (section 2.2.2.7) value is 1."
                    // but for some reason, that's not the case when syncing with AkrutoSync and Memotoo, and in that case we fallback 
                    // to "now" for the completion date
                    if (result.Completed == null)
                        result.Completed = DateTime.Now;
                }
                result.Importance = data.Element("Importance") != null ? (ExchangeTaskImportance)int.Parse(data.Element("Importance").Value) : ExchangeTaskImportance.Normal;
                
                result.IsRecurring = data.Element("Recurrence") != null;
                XElement recurrence = data.Element("Recurrence");
                if (result.IsRecurring)
                {
                    // Get the DeadOccur property to know if it is a "real" recurring task or only an instance
                    if (recurrence.Element("DeadOccur") != null)
                    {
                        int deadOccur = int.Parse(recurrence.Element("DeadOccur").Value);
                        if (deadOccur > 0)
                            result.IsRecurring = false;
                    }
                    else
                    {
                        // No "DeadOccur" element, we assume (from the documentation) that value is equal to 0
                        result.IsRecurring = false;
                    }
                }

                XElement reminderSet = data.Element("ReminderSet");
                if (reminderSet != null && reminderSet.Value == "1")
                {
                    XElement reminderTime = data.Element("ReminderTime");
                    if (reminderTime != null && !string.IsNullOrWhiteSpace(reminderTime.Value))
                    {
                        result.Alarm = GetDate(reminderTime.Value);
                        if (result.Alarm.HasValue)
                            result.Alarm = result.Alarm.Value.ToLocalTime();
                    }
                }

                if (result.IsRecurring)
                {
                    int regenerate = 0;
                    if (recurrence.Element("Regenerate") != null && !string.IsNullOrEmpty(recurrence.Element("Regenerate").Value) && int.TryParse(recurrence.Element("Regenerate").Value, out regenerate))
                        result.UseFixedDate = regenerate != 1;

                    result.RecurrenceType = result.IsRecurring ? GetRecurrencePattern(int.Parse(recurrence.Element("Type").Value), result.UseFixedDate) : ExchangeRecurrencePattern.None;

                    // Manage recurring fields
                    result.Interval = int.Parse(recurrence.Element("Interval").Value);
                    
                    // DayOfWeek element
                    result.DaysOfWeek = ExchangeDayOfWeek.None;
                    if (result.RecurrenceType == ExchangeRecurrencePattern.Weekly || result.RecurrenceType == ExchangeRecurrencePattern.MonthlyRelative)
                    {
                        int days = 0;
                        if (recurrence.Element("DayOfWeek") != null)
                            days = int.Parse(recurrence.Element("DayOfWeek").Value);
                        result.DaysOfWeek = (ExchangeDayOfWeek)days;
                        if (HasMultipleDays(result.DaysOfWeek) && result.RecurrenceType == ExchangeRecurrencePattern.Weekly)
                            result.Interval = 0; // Set interval to 0 to avoid the Weekly standard pattern
                    }
                    
                    // DayOfMonth element
                    result.DayOfMonth = 0;
                    if (result.RecurrenceType == ExchangeRecurrencePattern.Monthly || result.RecurrenceType == ExchangeRecurrencePattern.Yearly)
                    {
                        result.DayOfMonth = int.Parse(recurrence.Element("DayOfMonth").Value);
                    }
                    
                    // WeekOfMonth element
                    if (result.RecurrenceType == ExchangeRecurrencePattern.MonthlyRelative)
                    {
                        // TODO : manage 'Yearly in the nth month' pattern
                        result.DayOfWeekIndex = GetDayOfTheWeekIndex(int.Parse(recurrence.Element("WeekOfMonth").Value));
                    }
                    
                    // MonthOfYear element
                    if (result.RecurrenceType == ExchangeRecurrencePattern.Yearly)
                    {
                        // TODO : manage 'Yearly in the nth month' pattern
                        result.Month = int.Parse(recurrence.Element("MonthOfYear").Value);
                    }
                    
                    LogService.Log("SyncCommandResult", $"Task {result.Subject} read completed");
                }

                return result;
            }
            catch (Exception ex)
            {
                using (var sw = new StringWriter())
                {
                    using (var xw = XmlWriter.Create(sw))
                    {
                        task.WriteTo(xw);
                    }

                    string content = sw.ToString().Replace("\r\n", string.Empty);
                    string message = $"Error when parsing task XML {ex}: {content}";

                    LogService.Log("SyncCommandResult", message);
                    
                    TryTrackEvent("Parsing", message);

                    throw new CommandException("unable to read task content");
                }
            }
        }

        private static ExchangeDayOfWeekIndex GetDayOfTheWeekIndex(int weekNumber)
        {
            switch (weekNumber)
            {
                case 1:
                    return ExchangeDayOfWeekIndex.First;
                case 2:
                    return ExchangeDayOfWeekIndex.Second;
                case 3:
                    return ExchangeDayOfWeekIndex.Third;
                case 4:
                    return ExchangeDayOfWeekIndex.Fourth;
                case 5:
                    return ExchangeDayOfWeekIndex.Last;
                default:
                    return ExchangeDayOfWeekIndex.First;
            }
        }

        private static ExchangeRecurrencePattern GetRecurrencePattern(int recurrencyType, bool useFixedDate)
        {
            switch (recurrencyType)
            {
                case 0:
                    if (useFixedDate)
                        return ExchangeRecurrencePattern.Daily;
                    else
                        return ExchangeRecurrencePattern.DailyRegeneration;
                case 1:
                    if (useFixedDate)
                        return ExchangeRecurrencePattern.Weekly;
                    else
                        return ExchangeRecurrencePattern.WeeklyRegeneration;
                case 2:
                    if (useFixedDate)
                        return ExchangeRecurrencePattern.Monthly;
                    else
                        return ExchangeRecurrencePattern.MonthlyRegeneration;
                case 3:
                    return ExchangeRecurrencePattern.MonthlyRelative;
                case 5:
                    if (useFixedDate)
                        return ExchangeRecurrencePattern.Yearly;
                    else
                        return ExchangeRecurrencePattern.YearlyRegeneration;
                default:
                    return ExchangeRecurrencePattern.None;
            }
        }

        private static DateTime? GetDate(string date)
        {
            if (string.IsNullOrEmpty(date) || date.Equals("0"))
                return null;

            if (dateTimeRegex.IsMatch(date))
            {
                Match m = dateTimeRegex.Match(date);
                return new DateTime(
                    int.Parse(m.Groups["year"].Value),
                    int.Parse(m.Groups["month"].Value),
                    int.Parse(m.Groups["day"].Value),
                    int.Parse(m.Groups["hour"].Value),
                    int.Parse(m.Groups["minute"].Value),
                    int.Parse(m.Groups["second"].Value));
            }
            return null;
        }

        private static bool HasMultipleDays(ExchangeDayOfWeek days)
        {
            int daysValue = (int)days;
            // Test if value is a power of 2
            double sqrt = Math.Sqrt(Convert.ToDouble(daysValue));
            return sqrt != Convert.ToDouble(Convert.ToInt32(sqrt));
        }
    }
}

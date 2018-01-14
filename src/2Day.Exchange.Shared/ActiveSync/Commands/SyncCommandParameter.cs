using System;
using System.Linq;
using System.Text;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Exchange.Shared.Commands;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange.ActiveSync.Commands
{
    public class SyncCommandParameter : ExchangeChangeSet, IRequestParameterBuilder
    {
        public string SyncKey { get; private set; }
        public string FolderId { get; private set; }

        public SyncCommandParameter(string syncKey, string folderId, ExchangeChangeSet changeset)
        {
            if (string.IsNullOrEmpty(syncKey))
                throw new ArgumentNullException("syncKey");
            if (string.IsNullOrEmpty(folderId))
                throw new ArgumentNullException("folderId");
            if (changeset == null)
                throw new ArgumentNullException("changeset");

            this.SyncKey = syncKey;
            this.FolderId = folderId;

            this.AddedTasks = changeset.AddedTasks;
            this.ModifiedTasks = changeset.ModifiedTasks;
            this.DeletedTasks = changeset.DeletedTasks;
        }

        public string BuildXml(string command)
        {
            var builder = new StringBuilder();

            builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            builder.Append("<Sync xmlns=\"AirSync\" xmlns:tasks=\"Tasks\" xmlns:airsyncbase=\"AirSyncBase\">");
            builder.Append(" <Collections>");
            builder.Append("     <Collection>");
            builder.AppendFormat("         <SyncKey>{0}</SyncKey>", this.SyncKey);
            builder.AppendFormat("         <CollectionId>{0}</CollectionId>", this.FolderId);
            // Request for full body ! (Type 1 = Plain text, AllOrNone 1 = No truncate)
            //builder.Append("<DeletesAsMoves>0</DeletesAsMoves><WindowSize>512</WindowSize>");
            builder.Append("<Options><airsyncbase:BodyPreference><airsyncbase:Type>1</airsyncbase:Type><airsyncbase:AllOrNone>1</airsyncbase:AllOrNone></airsyncbase:BodyPreference></Options>");
            
            if (this.AddedTasks.Any() || this.ModifiedTasks.Any() || this.DeletedTasks.Any())
            {
                builder.Append("     <Commands>");

                this.AppendAddedTasks(builder);
                this.AppendModifiedTasks(builder);
                this.AppendDeletedTasks(builder);

                builder.Append("     </Commands>");
            }
            builder.Append("     </Collection>");
            builder.Append("</Collections>");
            builder.Append("</Sync>");

            return builder.ToString();
        }

        private void AppendAddedTasks(StringBuilder xmlBuilder)
        {
            if (this.AddedTasks.Any())
            {
                foreach (var task in this.AddedTasks)
                {
                    xmlBuilder.Append("        <Add>");
                    CreateTaskXml(task, xmlBuilder, true);
                    xmlBuilder.Append("        </Add>");
                }
            }
        }

        private void AppendModifiedTasks(StringBuilder xmlBuilder)
        {
            if (this.ModifiedTasks.Any())
            {
                foreach (var task in this.ModifiedTasks)
                {
                    xmlBuilder.Append("        <Change>");
                    CreateTaskXml(task, xmlBuilder, false);
                    xmlBuilder.Append("        </Change>");
                }
            }
        }

        private void AppendDeletedTasks(StringBuilder xmlBuilder)
        {
            if (this.DeletedTasks.Any())
            {
                foreach (var task in this.DeletedTasks)
                {
                    xmlBuilder.Append("        <Delete>");
                    xmlBuilder.AppendFormat("<ServerId>{0}</ServerId>", task.Id);
                    xmlBuilder.Append("        </Delete>");
                }
            }
        }

        private static void CreateTaskXml(ExchangeTask task, StringBuilder builder, bool isAdd)
        {
            // Server Id (in case of update)
            if (isAdd)
                builder.AppendFormat("<ClientId>{0}</ClientId>", task.LocalId);
            else
                builder.AppendFormat("<ServerId>{0}</ServerId>", task.Id);

            builder.Append("<ApplicationData>");
            
            // Subject
            builder.AppendFormat("<tasks:Subject>{0}</tasks:Subject>", task.Subject.ToEscapedXml());
            
            // Notes
            builder.Append("<airsyncbase:Body>");
            builder.Append("<airsyncbase:Type>1</airsyncbase:Type>");
            builder.AppendFormat("<airsyncbase:Data>{0}</airsyncbase:Data>", !string.IsNullOrEmpty(task.Note) ? task.Note.ToEscapedXml() : " ");
            builder.Append("</airsyncbase:Body>");

            // Category
            if (!string.IsNullOrEmpty(task.Category))
            {
                builder.Append("<tasks:Categories>");
                builder.AppendFormat("<tasks:Category>{0}</tasks:Category>", task.Category.ToEscapedXml());
                builder.Append("</tasks:Categories>");
            }
            // Complete
            builder.AppendFormat("<tasks:Complete>{0}</tasks:Complete>", task.Completed.HasValue ? "1" : "0");

            // Completed
            if (task.Completed.HasValue)
                builder.AppendFormat("<tasks:DateCompleted>{0}</tasks:DateCompleted>", GetFullDateString(task.Completed.Value.ToUniversalTime()));

            // Due Date
            if (task.Due.HasValue)
            {
                builder.AppendFormat("<tasks:DueDate>{0}</tasks:DueDate>", GetFullDateString(task.Due.Value));
                builder.AppendFormat("<tasks:UtcDueDate>{0}</tasks:UtcDueDate>", GetFullDateString(task.Due.Value.ToUniversalTime()));
            }

            // Priority
            builder.AppendFormat("<tasks:Importance>{0}</tasks:Importance>", (int)task.Importance);

            if (task.Start.HasValue)
            {
                // Start Date
                builder.AppendFormat("<tasks:StartDate>{0}</tasks:StartDate>", GetFullDateString(task.Start.Value));
                builder.AppendFormat("<tasks:UtcStartDate>{0}</tasks:UtcStartDate>", GetFullDateString(task.Start.Value.ToUniversalTime()));
            }

            // Recurrence
            AppendRecurrenceTag(task, builder);
            
            // Sensitivity
            builder.Append("<tasks:Sensitivity>0</tasks:Sensitivity>");

            // Reminder
            if (task.Alarm.HasValue)
            {
                builder.Append("<tasks:ReminderSet>1</tasks:ReminderSet>");
                builder.AppendFormat("<tasks:ReminderTime>{0}</tasks:ReminderTime>", GetFullDateString(task.Alarm.Value.ToUniversalTime()));
            }

            builder.Append("</ApplicationData>");
        }

        private static void AppendRecurrenceTag(ExchangeTask task, StringBuilder xmlBuilder)
        {
            if (!task.IsRecurring)
                return;

            xmlBuilder.Append("<tasks:Recurrence>");

            // Type
            xmlBuilder.AppendFormat("<tasks:Type>{0}</tasks:Type>", GetRecurrencePatternInteger(task.RecurrenceType));
            
            // Start
            xmlBuilder.AppendFormat("<tasks:Start>{0}</tasks:Start>", task.Start.HasValue ? GetFullDateString(task.Start.Value) : GetFullDateString(task.Created));
            
            //// Dead occur
            //builder.AppendFormat("<tasks:DeadOccur>{0}</tasks:DeadOccur>", task.Completed.HasValue ? 1 : 0);
            xmlBuilder.AppendFormat("<tasks:DeadOccur>{0}</tasks:DeadOccur>", 0);

            // Regenerate
            int regenerate = 0;
            if (task.RecurrenceType == ExchangeRecurrencePattern.DailyRegeneration ||
                task.RecurrenceType == ExchangeRecurrencePattern.WeeklyRegeneration ||
                task.RecurrenceType == ExchangeRecurrencePattern.MonthlyRegeneration ||
                task.RecurrenceType == ExchangeRecurrencePattern.YearlyRegeneration)
            {
                regenerate = 1;
            }
            xmlBuilder.AppendFormat("<tasks:Regenerate>{0}</tasks:Regenerate>", regenerate);

            // Interval
            xmlBuilder.AppendFormat("<tasks:Interval>{0}</tasks:Interval>", task.Interval == 0 ? 1 : task.Interval);

            // DayOfWeek
            if (task.RecurrenceType == ExchangeRecurrencePattern.Weekly || task.RecurrenceType == ExchangeRecurrencePattern.MonthlyRelative)
            {
                xmlBuilder.AppendFormat("<tasks:DayOfWeek>{0}</tasks:DayOfWeek>", (int)task.DaysOfWeek);
            }

            // DayOfMonth
            if (task.RecurrenceType == ExchangeRecurrencePattern.Monthly || task.RecurrenceType == ExchangeRecurrencePattern.Yearly)
            {
                xmlBuilder.AppendFormat("<tasks:DayOfMonth>{0}</tasks:DayOfMonth>", task.DayOfMonth == 0 ? 1 : task.DayOfMonth);
            }

            // WeekOfMonth
            if (task.RecurrenceType == ExchangeRecurrencePattern.MonthlyRelative)
            {
                xmlBuilder.AppendFormat("<tasks:WeekOfMonth>{0}</tasks:WeekOfMonth>", (int)task.DayOfWeekIndex +1);
            }

            // MonthOfYear
            if (task.RecurrenceType == ExchangeRecurrencePattern.Yearly)
            {
                xmlBuilder.AppendFormat("<tasks:MonthOfYear>{0}</tasks:MonthOfYear>", task.Month == 0 ? 1 : task.Month);

            }

            xmlBuilder.Append("</tasks:Recurrence>");
        }

        private static string GetFullDateString(DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss.000Z");
        }

        private static int GetRecurrencePatternInteger(ExchangeRecurrencePattern recurrencyType)
        {
            /*
            Value Meaning 
            0 Recurs daily. 
            1 Recurs weekly. 
            2 Recurs monthly. 
            3 Recurs monthly on the nth day. 
            5 Recurs yearly. 
            6 Recurs yearly on the nth day. 
            */
            switch (recurrencyType)
            {
                case ExchangeRecurrencePattern.None:
                    return -1;
                case ExchangeRecurrencePattern.DailyRegeneration:
                case ExchangeRecurrencePattern.Daily:
                    return 0;
                case ExchangeRecurrencePattern.Weekly:
                case ExchangeRecurrencePattern.WeeklyRegeneration:
                    return 1;
                case ExchangeRecurrencePattern.Monthly:
                case ExchangeRecurrencePattern.MonthlyRegeneration:
                    return 2;
                case ExchangeRecurrencePattern.MonthlyRelative:
                    return 3;
                case ExchangeRecurrencePattern.Yearly:
                case ExchangeRecurrencePattern.YearlyRegeneration:
                    return 5;
                case ExchangeRecurrencePattern.YearlyRelative:
                    return 6;
                default:
                    throw new ArgumentOutOfRangeException("recurrencyType");
            }
        }
    }
}

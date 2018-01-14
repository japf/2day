using System;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange.Ews
{
    /// <summary>
    /// Helper class to make the glue between EwsXXX types and ExchangeXXX types
    /// </summary>
    public static class EwsModelHelper
    {
        private const char ChangeKeySeparator = '#';
        private const string ItemPrefix = "item.";

        public static ExchangeTask BuildExchangeTask(this EwsTask ewsTask)
        {
            var id = string.Format("{0}{1}{2}", ewsTask.Id, ChangeKeySeparator, ewsTask.ChangeKey);
            if (ewsTask.Type == EwsItemType.Item)
                id = ItemPrefix + id;

            var exchangeTask = new ExchangeTask
            {
                Id = id,
                Subject = ewsTask.Subject,
                Start = ewsTask.StartDate,
                Due = ewsTask.DueDate,
                Completed = ewsTask.CompleteDate,
                Importance = (ExchangeTaskImportance)(int)ewsTask.Importance,
            };

            if (ewsTask.ReminderIsSet && ewsTask.ReminderDate.HasValue && ewsTask.ReminderDate.Value != DateTime.MinValue)
                exchangeTask.Alarm = ewsTask.ReminderDate;

            if (!string.IsNullOrWhiteSpace(ewsTask.Body))
                exchangeTask.Note = ewsTask.Body;

            if (ewsTask.Categories != null && ewsTask.Categories.Length > 0)
                exchangeTask.Category = ewsTask.Categories[0];

            if (ewsTask.Recurrence != null && ewsTask.Recurrence.RecurrenceType != ExchangeRecurrencePattern.None)
            {
                exchangeTask.IsRecurring = true;
                exchangeTask.DayOfMonth = ewsTask.Recurrence.DayOfMonth;
                exchangeTask.DayOfWeekIndex = ewsTask.Recurrence.DayOfWeekIndex;
                exchangeTask.DaysOfWeek = ewsTask.Recurrence.DaysOfWeek;
                exchangeTask.Interval = ewsTask.Recurrence.Interval;
                exchangeTask.Month = ewsTask.Recurrence.Month;
                exchangeTask.RecurrenceType = ewsTask.Recurrence.RecurrenceType;
            }

            return exchangeTask;
        }

        public static EwsTask BuildEwsTask(this ExchangeTask exchangeTask)
        {
            var ewsTask = new EwsTask
            {
                Subject = exchangeTask.Subject,
                Categories = new[] { exchangeTask.Category },
                StartDate = exchangeTask.Start,
                CompleteDate = exchangeTask.Completed,
                Importance = (EwsImportance)(int)exchangeTask.Importance
            };

            if (exchangeTask.Due.HasValue)
                ewsTask.DueDate = exchangeTask.Due.Value.Date;

            if (exchangeTask.Alarm.HasValue)
            {
                ewsTask.ReminderDate = exchangeTask.Alarm;
                ewsTask.ReminderIsSet = true;
            }

            if (!string.IsNullOrWhiteSpace(exchangeTask.Note))
            {
                ewsTask.Body = exchangeTask.Note;
                ewsTask.BodyType = exchangeTask.Note.HasHtml() ? EwsBodyType.HTML : EwsBodyType.Text;
            }

            if (!exchangeTask.Properties.HasValue)
                throw new NotSupportedException("Task has no properties set");

            if (exchangeTask.IsRecurring && exchangeTask.Due.HasValue)
            {
                var recurrence = new EwsRecurrence
                {
                    RecurrenceType = exchangeTask.RecurrenceType,
                    Interval = exchangeTask.Interval,
                    DayOfMonth = exchangeTask.DayOfMonth,
                    DayOfWeekIndex = exchangeTask.DayOfWeekIndex,
                    DaysOfWeek = exchangeTask.DaysOfWeek,
                    Month = exchangeTask.Month,
                };

                if (!exchangeTask.Start.HasValue)
                    recurrence.StartDate = exchangeTask.Due.Value;
                else
                    recurrence.StartDate = exchangeTask.Start.Value;

                ewsTask.Recurrence = recurrence;
            }

            if (exchangeTask.Properties.Value.HasFlag(ExchangeTaskProperties.Title))
                ewsTask.Changes |= EwsFields.Subject;
            if (exchangeTask.Properties.Value.HasFlag(ExchangeTaskProperties.Folder))
                ewsTask.Changes |= EwsFields.Categories;
            if (exchangeTask.Properties.Value.HasFlag(ExchangeTaskProperties.Due))
                ewsTask.Changes |= EwsFields.DueDate;
            if (exchangeTask.Properties.Value.HasFlag(ExchangeTaskProperties.Start))
                ewsTask.Changes |= EwsFields.StartDate;
            if (exchangeTask.Properties.Value.HasFlag(ExchangeTaskProperties.Completed))
                ewsTask.Changes |= EwsFields.CompleteDate;
            if (exchangeTask.Properties.Value.HasFlag(ExchangeTaskProperties.Priority))
                ewsTask.Changes |= EwsFields.Importance;
            if (exchangeTask.Properties.Value.HasFlag(ExchangeTaskProperties.RepeatFrom))
                ewsTask.Changes |= EwsFields.Recurrence;
            if (exchangeTask.Properties.Value.HasFlag(ExchangeTaskProperties.Alarm))
                ewsTask.Changes |= EwsFields.Reminder;
            if (exchangeTask.Properties.Value.HasFlag(ExchangeTaskProperties.Note))
                ewsTask.Changes |= EwsFields.Body;

            if (!string.IsNullOrWhiteSpace(exchangeTask.Id))
            {
                string id = exchangeTask.Id;
                if (exchangeTask.Id.StartsWith(ItemPrefix))
                {
                    id = id.Replace(ItemPrefix, string.Empty);
                    ewsTask.Type = EwsItemType.Item;
                }

                var identifier = GetEwsItemIdentifier(id);
                ewsTask.Id = identifier.Id;
                ewsTask.ChangeKey = identifier.ChangeKey;
            }
            
            return ewsTask;
        }

        public static string GetFullId(this EwsItemIdentifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            return string.Format("{0}{1}{2}", identifier.Id, ChangeKeySeparator, identifier.ChangeKey);
        }

        public static string GetId(this string fullId)
        {
            if (string.IsNullOrWhiteSpace(fullId))
                throw new ArgumentNullException(nameof(fullId));
            if (fullId.IndexOf(ChangeKeySeparator) < 0)
                throw new ArgumentException("Id should contains the '#' separator");

            string[] array = fullId.Split(ChangeKeySeparator);
            if (array.Length != 2)
                throw new ArgumentException("Invalid full id");

            return array[0].Replace(ItemPrefix, string.Empty);
        }

        public static EwsItemIdentifier GetEwsItemIdentifier(this string fullId)
        {
            if (string.IsNullOrWhiteSpace(fullId))
                throw new ArgumentNullException(nameof(fullId));
            if (fullId.IndexOf(ChangeKeySeparator) < 0)
                throw new ArgumentException("Id should contains the '#' separator");
            if (fullId.Contains(ItemPrefix))
                fullId = fullId.Replace(ItemPrefix, string.Empty);

            string[] array = fullId.Split(ChangeKeySeparator);
            if (array.Length != 2)
                throw new ArgumentException("Invalid full id");

            return new EwsItemIdentifier(array[0], array[1]);
        }

        public static EwsRequestSettings CreateEwsSettings(this ExchangeConnectionInfo connectionInfo)
        {
            return new EwsRequestSettings(connectionInfo.Email, connectionInfo.Username, connectionInfo.Password, connectionInfo.ServerUri.ToString());
        }
    }
}

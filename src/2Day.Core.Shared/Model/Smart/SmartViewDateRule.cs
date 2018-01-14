using System;
using System.Collections.Generic;
using System.Globalization;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public class SmartViewDateRule : SmartViewRule<SmartViewDateParameter>
    {
        private static readonly List<SmartViewFilter> supportedFilters = new List<SmartViewFilter>
        {
            SmartViewFilter.Is,
            SmartViewFilter.IsYesterday,
            SmartViewFilter.IsToday,
            SmartViewFilter.IsTomorrow,
            SmartViewFilter.IsNot,
            SmartViewFilter.IsAfter,
            SmartViewFilter.IsBefore,
            SmartViewFilter.WasInTheLast,
            SmartViewFilter.WasNotInTheLast,
            SmartViewFilter.IsInTheNext,
            SmartViewFilter.IsNotInTheNext,
            SmartViewFilter.IsIn,
            SmartViewFilter.IsNotIn,
            SmartViewFilter.Was,
            SmartViewFilter.WasNot,
            SmartViewFilter.Yes,
            SmartViewFilter.No,
            SmartViewFilter.Exists,
            SmartViewFilter.DoesNotExist
        };

        private static readonly List<SmartViewField> supportedFields = new List<SmartViewField>
        {
            SmartViewField.Added,
            SmartViewField.Completed,
            SmartViewField.Due,
            SmartViewField.Start,
            SmartViewField.Modified
        };

        public override List<SmartViewField> SupportedFields
        {
            get { return supportedFields; }
        }

        public override List<SmartViewFilter> SupportedFilters
        {
            get { return supportedFilters; }
        }

        public SmartViewDateRule()
        {
        }

        public SmartViewDateRule(SmartViewFilter filter, SmartViewField field, SmartViewDateParameter value)
            : base(filter, field, value)
        {
        }

        public override bool IsMatch(ITask task)
        {
            DateTime value;
            switch (this.Field)
            {
                case SmartViewField.HasAlarm:
                    value = task.Alarm ?? DateTime.MinValue;
                    break;
                case SmartViewField.Added:
                    value = task.Added.Date;
                    break;
                case SmartViewField.Completed:
                    value = task.Completed ?? DateTime.MinValue;
                    value = value.Date;
                    break;
                case SmartViewField.Due:
                    value = task.Due ?? DateTime.MinValue;
                    value = value.Date;
                    break;
                case SmartViewField.Start:
                    value = task.Start ?? DateTime.MinValue;
                    value = value.Date;
                    break;
                case SmartViewField.Modified:
                    value = task.Modified;
                    value = value.Date;
                    break;
                default:
                    throw new NotSupportedException("Incorrect field");
            }

            DateTime now = DateTime.Now.Date;
            DateTime date = DateTime.MinValue;
            int days = -1;
            if (this.Value.Date.HasValue)
                date = this.Value.Date.Value;
            else
                days = this.Value.Days;

            bool result;
            switch (this.Filter)
            {
                case SmartViewFilter.Is:
                    result = value.Date == date.Date;
                    break;
                case SmartViewFilter.IsYesterday:
                    result = value.Date == now.AddDays(-1).Date;
                    break;
                case SmartViewFilter.IsToday:
                    result = value.Date == now;
                    break;
                case SmartViewFilter.IsTomorrow:
                    result = value.Date == now.AddDays(1).Date;
                    break;
                case SmartViewFilter.IsNot:
                    result = value.Date != date.Date;
                    break;
                case SmartViewFilter.IsAfter:
                    result = value.Date > date.Date;
                    break;
                case SmartViewFilter.IsBefore:
                    result = value.Date < date.Date;
                    break;
                case SmartViewFilter.WasInTheLast:      // days
                    result = now.AddDays(-days).Date <= value && value <= now && value != DateTime.MinValue;
                    break;
                case SmartViewFilter.WasNotInTheLast:   // days
                    result = !(now.AddDays(-days).Date <= value && value <= now && value != DateTime.MinValue);
                    break;
                case SmartViewFilter.IsInTheNext:       // days
                    result = now.AddDays(days).Date > value && value >= now && value != DateTime.MinValue;
                    break;
                case SmartViewFilter.IsNotInTheNext:    // days
                    result = !(now.AddDays(days).Date > value && value >= now && value != DateTime.MinValue);
                    break;
                case SmartViewFilter.IsIn:              // days
                    result = now.AddDays(days).Date == value;
                    break;
                case SmartViewFilter.IsNotIn:           // days
                    result = now.AddDays(days).Date != value;
                    break;
                case SmartViewFilter.Was:               // days ago
                    result = now.AddDays(-days).Date == value;
                    break;
                case SmartViewFilter.WasNot:            // days ago
                    result = now.AddDays(-days).Date != value;
                    break;
                case SmartViewFilter.Yes:
                case SmartViewFilter.Exists:
                    result = value != DateTime.MinValue;
                    break;
                case SmartViewFilter.DoesNotExist:
                case SmartViewFilter.No:
                    result = value == DateTime.MinValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        public override SmartViewRule Read(SmartViewFilter filter, SmartViewField field, string value)
        {
            DateTime datetime;
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out datetime))
                return new SmartViewDateRule(filter, field, new SmartViewDateParameter(datetime));
            else
                return new SmartViewDateRule(filter, field, new SmartViewDateParameter(int.Parse(value)));
        }
    }
}
using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public class SmartViewBoolRule : SmartViewRule<bool>
    {
        private static readonly List<SmartViewFilter> supportedFilters = new List<SmartViewFilter>
        {
            SmartViewFilter.Yes,
            SmartViewFilter.No
        };
        private static readonly List<SmartViewField> supportedFields = new List<SmartViewField>
        {
            SmartViewField.HasAlarm,
            SmartViewField.HasRecurrence,
            SmartViewField.HasSubtasks,
            SmartViewField.IsLate
        };

        public override List<SmartViewField> SupportedFields
        {
            get { return supportedFields; }
        }

        public override List<SmartViewFilter> SupportedFilters
        {
            get { return supportedFilters; }
        }

        public SmartViewBoolRule()
        {
        }

        public SmartViewBoolRule(SmartViewFilter filter, SmartViewField field, bool value)
            : base(filter, field, value)
        {
        }

        public override bool IsMatch(ITask task)
        {
            bool value;
            if (this.Field == SmartViewField.HasAlarm)
                value = task.Alarm != null;
            else if (this.Field == SmartViewField.HasRecurrence)
                value = task.IsPeriodic;
            else if (this.Field == SmartViewField.HasSubtasks)
                value = task.Children.Count > 0;
            else if (this.Field == SmartViewField.IsLate)
                value = task.IsLate;
            else
                throw new NotSupportedException("Incorrect field");

            return (value && this.Filter == SmartViewFilter.Yes) || (!value && this.Filter == SmartViewFilter.No);
        }

        public override SmartViewRule Read(SmartViewFilter filter, SmartViewField field, string value)
        {
            return new SmartViewBoolRule(filter, field, bool.Parse(value));
        }
    }
}
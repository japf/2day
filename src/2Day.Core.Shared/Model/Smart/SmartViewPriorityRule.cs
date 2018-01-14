using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public class SmartViewPriorityRule : SmartViewRule<TaskPriority>
    {
        private static readonly List<SmartViewFilter> supportedFilters = new List<SmartViewFilter> { SmartViewFilter.Is, SmartViewFilter.IsNot };
        private static readonly List<SmartViewField> supportedFields = new List<SmartViewField> { SmartViewField.Priority };

        public override List<SmartViewField> SupportedFields
        {
            get { return supportedFields; }
        }

        public override List<SmartViewFilter> SupportedFilters
        {
            get { return supportedFilters; }
        }

        public SmartViewPriorityRule()
        {
        }

        public SmartViewPriorityRule(SmartViewFilter filter, SmartViewField field, TaskPriority value)
            : base(filter, field, value)
        {
        }

        public override bool IsMatch(ITask task)
        {
            if (this.Field != SmartViewField.Priority)
                throw new NotImplementedException("Incorrect field");

            if (this.Filter == SmartViewFilter.Is)
                return task.Priority == this.Value;
            else
                return task.Priority != this.Value;
        }

        public override SmartViewRule Read(SmartViewFilter filter, SmartViewField field, string value)
        {
            return new SmartViewPriorityRule(filter, field, value.ParseAsEnum<TaskPriority>() );
        }
    }
}
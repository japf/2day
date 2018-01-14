using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public class SmartViewProgressRule : SmartViewRule<double>
    {
        private static readonly List<SmartViewFilter> supportedFilters = new List<SmartViewFilter> { SmartViewFilter.Is, SmartViewFilter.IsNot, SmartViewFilter.IsMoreThan, SmartViewFilter.IsLessThan };
        private static readonly List<SmartViewField> supportedFields = new List<SmartViewField> { SmartViewField.Progress };

        public override List<SmartViewField> SupportedFields
        {
            get { return supportedFields; }
        }

        public override List<SmartViewFilter> SupportedFilters
        {
            get { return supportedFilters; }
        }

        public SmartViewProgressRule()
        {
        }

        public SmartViewProgressRule(SmartViewFilter filter, SmartViewField field, double value)
            : base(filter, field, value)
        {
        }

        public override bool IsMatch(ITask task)
        {
            if (this.Field != SmartViewField.Progress)
                throw new NotImplementedException("Incorrect field");

            switch (this.Filter)
            {
                case SmartViewFilter.Is:
                    return (task.Progress * 100) == this.Value;
                case SmartViewFilter.IsNot:
                    return (task.Progress * 100) != this.Value;
                case SmartViewFilter.IsMoreThan:
                    return (task.Progress * 100) > this.Value;
                case SmartViewFilter.IsLessThan:
                    return task.Progress == null || (task.Progress * 100) < this.Value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override SmartViewRule Read(SmartViewFilter filter, SmartViewField field, string value)
        {
            return new SmartViewProgressRule(filter, field, double.Parse(value));
        }
    }
}
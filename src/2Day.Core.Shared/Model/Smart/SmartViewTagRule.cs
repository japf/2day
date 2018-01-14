using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public class SmartViewTagRule : SmartViewRule<string>
    {
        private static readonly List<SmartViewFilter> supportedFilters = new List<SmartViewFilter> { SmartViewFilter.Is, SmartViewFilter.IsNot, SmartViewFilter.Contains, SmartViewFilter.DoesNotContains, SmartViewFilter.Exists, SmartViewFilter.DoesNotExist };
        private static readonly List<SmartViewField> supportedFields = new List<SmartViewField> { SmartViewField.Tags };

        public override List<SmartViewField> SupportedFields
        {
            get { return supportedFields; }
        }

        public override List<SmartViewFilter> SupportedFilters
        {
            get { return supportedFilters; }
        }

        public SmartViewTagRule()
        {
        }

        public SmartViewTagRule(SmartViewFilter filter, SmartViewField field, string value)
            : base(filter, field, value)
        {
        }

        public override bool IsMatch(ITask task)
        {
            if (this.Field != SmartViewField.Tags)
                throw new NotImplementedException("Incorrect field");

            switch (this.Filter)
            {
                case SmartViewFilter.Is:
                    return task.Tags != null && task.Tags.Equals(this.Value, StringComparison.OrdinalIgnoreCase);
                case SmartViewFilter.IsNot:
                    return task.Tags == null || (task.Tags != null && !task.Tags.Equals(this.Value, StringComparison.OrdinalIgnoreCase));
                case SmartViewFilter.Contains:
                    return task.Tags != null && task.Tags.Contains(this.Value);
                case SmartViewFilter.DoesNotContains:
                    return string.IsNullOrWhiteSpace(task.Tags) || (task.Tags != null && !task.Tags.Contains(this.Value));
                case SmartViewFilter.Exists:
                    return task.ReadTags().Count > 0;
                case SmartViewFilter.DoesNotExist:
                    return task.ReadTags().Count == 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override SmartViewRule Read(SmartViewFilter filter, SmartViewField field, string value)
        {
            return new SmartViewTagRule(filter, field, value);
        }
    }
}
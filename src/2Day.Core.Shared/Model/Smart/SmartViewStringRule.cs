using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public class SmartViewStringRule : SmartViewRule<string>
    {
        private static readonly List<SmartViewFilter> supportedFilters = new List<SmartViewFilter> { SmartViewFilter.Contains, SmartViewFilter.Is, SmartViewFilter.BeginsWith, SmartViewFilter.EndsWith, SmartViewFilter.DoesNotContains, SmartViewFilter.IsNot };
        private static readonly List<SmartViewField> supportedFields = new List<SmartViewField> { SmartViewField.Title, SmartViewField.Note };

        public override List<SmartViewField> SupportedFields
        {
            get { return supportedFields; }
        }

        public override List<SmartViewFilter> SupportedFilters
        {
            get { return supportedFilters; }
        }

        public SmartViewStringRule()
        {
        }

        public SmartViewStringRule(SmartViewFilter filter, SmartViewField field, string value)
            : base(filter, field, value)
        {
        }

        public override bool IsMatch(ITask task)
        {
            string value = string.Empty;
            if (this.Field == SmartViewField.Title)
                value = task.Title;
            else if (this.Field == SmartViewField.Note)
                value = task.Note ?? string.Empty;
            else
                throw new NotSupportedException("Incorrect field");

            switch (this.Filter)
            {
                case SmartViewFilter.Contains:
                    return value.Contains(this.Value);
                case SmartViewFilter.BeginsWith:
                    return value.StartsWith(this.Value, StringComparison.OrdinalIgnoreCase);
                case SmartViewFilter.EndsWith:
                    return value.EndsWith(this.Value, StringComparison.OrdinalIgnoreCase);
                case SmartViewFilter.DoesNotContains:
                    return !value.Contains(this.Value);
                case SmartViewFilter.Is:
                    return value.Equals(this.Value, StringComparison.OrdinalIgnoreCase);
                case SmartViewFilter.IsNot:
                    return !value.Equals(this.Value, StringComparison.OrdinalIgnoreCase);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override SmartViewRule Read(SmartViewFilter filter, SmartViewField field, string value)
        {
            return new SmartViewStringRule(filter, field, value);
        }
    }
}

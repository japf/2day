using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public class SmartViewContextRule : SmartViewRule<string>
    {
        private static readonly List<SmartViewFilter> supportedFilters = new List<SmartViewFilter> { SmartViewFilter.Is, SmartViewFilter.IsNot, SmartViewFilter.Exists, SmartViewFilter.DoesNotExist };
        private static readonly List<SmartViewField> supportedFields = new List<SmartViewField> { SmartViewField.Context };

        public override List<SmartViewField> SupportedFields
        {
            get { return supportedFields; }
        }

        public override List<SmartViewFilter> SupportedFilters
        {
            get { return supportedFilters; }
        }

        public SmartViewContextRule()
        {
        }

        public SmartViewContextRule(SmartViewFilter filter, SmartViewField field, string value)
            : base(filter, field, value)
        {
        }

        public override bool IsMatch(ITask task)
        {
            if (this.Field != SmartViewField.Context)
                throw new NotImplementedException("Incorrect field"); 
            
            switch (this.Filter)
            {
                case SmartViewFilter.Is:
                    return task.Context != null && task.Context.Name.Equals(this.Value, StringComparison.OrdinalIgnoreCase);
                case SmartViewFilter.IsNot:
                    return task.Context == null || (task.Context != null && !task.Context.Name.Equals(this.Value, StringComparison.OrdinalIgnoreCase));
                case SmartViewFilter.Exists:
                    return task.Context != null;
                case SmartViewFilter.DoesNotExist:
                    return task.Context == null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override SmartViewRule Read(SmartViewFilter filter, SmartViewField field, string value)
        {
            return new SmartViewContextRule(filter, field, value);
        }
    }
}
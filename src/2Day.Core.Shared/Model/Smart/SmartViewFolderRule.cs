using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public class SmartViewFolderRule : SmartViewRule<string>
    {
        private static readonly List<SmartViewFilter> supportedFilters = new List<SmartViewFilter> { SmartViewFilter.Is, SmartViewFilter.IsNot };
        private static readonly List<SmartViewField> supportedFields = new List<SmartViewField> { SmartViewField.Folder };

        public override List<SmartViewField> SupportedFields
        {
            get { return supportedFields; }
        }

        public override List<SmartViewFilter> SupportedFilters
        {
            get { return supportedFilters; }
        }

        public SmartViewFolderRule()
        {
        }

        public SmartViewFolderRule(SmartViewFilter filter, SmartViewField field, string value)
            : base(filter, field, value)
        {
        }

        public override bool IsMatch(ITask task)
        {
            if (this.Field != SmartViewField.Folder)
                throw new NotImplementedException("Incorrect field");

            bool isMatch = task.Folder.Name.Equals(this.Value, StringComparison.OrdinalIgnoreCase);
            if (this.Filter == SmartViewFilter.Is)
                return isMatch;
            else
                return !isMatch;
        }

        public override SmartViewRule Read(SmartViewFilter filter, SmartViewField field, string value)
        {
            return new SmartViewFolderRule(filter, field, value);
        }
    }
}
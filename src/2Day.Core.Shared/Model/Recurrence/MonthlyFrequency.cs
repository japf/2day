using System;
using System.Globalization;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public class MonthlyFrequency : CustomFrequencyBase
    {
        #region Private fields
        private DateTime? referenceDate;
        #endregion

        #region Public properties
        public override FrequencyType FrequencyType
        {
            get { return FrequencyType.Monthly; }
        }

        public override string DefaultTitle
        {
            get { return StringResources.CustomFrequency_Monthly; }
        }

        public override bool IsCustomizable
        {
            get { return false; }
        }

        public override bool IsValidValue
        {
            get { return true; }
        }
        #endregion

        #region Public methods
        public override DateTime ComputeNextDate(DateTime fromDate)
        {
            return fromDate.AddMonths(1);
        }

        public override void SetReferenceDate(DateTime? date)
        {
            if (date != this.referenceDate)
            {
                this.referenceDate = date;
                this.invalidateDisplayValue = true;
            }
        }
        protected override void ParseValue(string value)
        {
            // Do nothing
        }
#endregion

        #region Private methods
        protected override void UpdateDisplayValue()
        {
            if (this.referenceDate == null)
                this.displayValue = StringResources.CustomFrequency_Monthly;
            else
            {
                this.displayValue = string.Format(CultureInfo.InvariantCulture, StringResources.CustomFrequency_Each,
                                                  this.referenceDate.Value.Day);

            }
            this.invalidateDisplayValue = false;
        }
        #endregion

        public override bool Equals(ICustomFrequency other)
        {
            return other is MonthlyFrequency;
        }
    }
}

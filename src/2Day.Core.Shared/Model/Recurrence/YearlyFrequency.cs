using System;
using System.Globalization;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public class YearlyFrequency : CustomFrequencyBase
    {
        private DateTime? referenceDate;

        public override FrequencyType FrequencyType
        {
            get { return FrequencyType.Yearly; }
        }

        public override string DefaultTitle
        {
            get { return StringResources.CustomFrequency_Yearly; }
        }

        public override bool IsCustomizable
        {
            get { return false; }
        }

        public override bool IsValidValue
        {
            get { return true; }
        }

        public override DateTime ComputeNextDate(DateTime fromDate)
        {
            return fromDate.AddYears(1);
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
        }

        protected override void UpdateDisplayValue()
        {
            if (this.referenceDate == null)
                this.displayValue = StringResources.CustomFrequency_Yearly;
            else
            {
                string dateString = this.referenceDate.Value.ToString("M");
                this.displayValue = string.Format(CultureInfo.InvariantCulture, StringResources.CustomFrequency_Each, dateString);

            }
            this.invalidateDisplayValue = false;
        }

        public override bool Equals(ICustomFrequency other)
        {
            return other is YearlyFrequency;
        }
    }
}

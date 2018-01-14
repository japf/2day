using System;
using System.Globalization;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public class DailyFrequency : CustomFrequencyBase
    {
        public override FrequencyType FrequencyType
        {
            get { return FrequencyType.Daily; }
        }

        public override string DefaultTitle
        {
            get { return StringResources.CustomFrequency_Daily; }
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
            return fromDate.AddDays(1);
        }

        protected override void ParseValue(string value)
        {
            // Do nothing
        }

        protected override void UpdateDisplayValue()
        {
            this.displayValue = string.Format(CultureInfo.InvariantCulture, StringResources.CustomFrequency_Each,
                                 StringResources.CustomFrequency_Day);
            this.invalidateDisplayValue = false;
        }

        public DailyFrequency()
        {
            this.invalidateDisplayValue = true;
        }

        public override bool Equals(ICustomFrequency other)
        {
            return other is DailyFrequency;
        }
    }
}

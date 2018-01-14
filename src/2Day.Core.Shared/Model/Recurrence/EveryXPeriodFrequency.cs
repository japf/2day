using System;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public class EveryXPeriodFrequency : CustomFrequencyBase
    {
        private CustomFrequencyScale scale;
        private int rate;

        public override FrequencyType FrequencyType
        {
            get { return FrequencyType.EveryXPeriod; }
        }

        public int Rate
        {
            get
            {
                return this.rate;
            }
            set
            {
                if (this.rate != value)
                {
                    this.rate = value;
                    this.UpdateValue();
                }
            }
        }

        public CustomFrequencyScale Scale
        {
            get
            {
                return this.scale;
            }
            set
            {
                if (this.scale != value)
                {
                    this.scale = value;
                    this.UpdateValue();
                }
            }
        }

        public override string DefaultTitle
        {
            get { return StringResources.CustomFrequency_EveryXPeriodTitle; }
        }

        public override bool IsCustomizable
        {
            get { return true; }
        }

        public override bool IsValidValue
        {
            get
            {
                return this.rate > 0 && this.scale != CustomFrequencyScale.None;
            }
        }

        public EveryXPeriodFrequency()
        {
            this.ParseValue(String.Empty);
        }

        public override DateTime ComputeNextDate(DateTime fromDate)
        {
            DateTime newDate = new DateTime();
            switch (this.scale)
            {
                case CustomFrequencyScale.Day:
                    newDate = fromDate.AddDays(this.rate);
                    break;
                case CustomFrequencyScale.Week:
                    newDate = fromDate.AddDays(this.rate * 7);
                    break;
                case CustomFrequencyScale.Month:
                    newDate = fromDate.AddMonths(this.rate);
                    break;
                case CustomFrequencyScale.Year:
                    newDate = fromDate.AddYears(this.rate);
                    break;
            }
            return newDate;
        }

        private void UpdateValue()
        {
            this.value = String.Concat(this.rate.ToString(),
                                        valueSeparator,
                                        ((int)this.scale).ToString()
                                     );
            this.invalidateDisplayValue = true;
        }

        protected override void ParseValue(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                // Should occur only at initialization
                this.rate = 1;
                this.scale = CustomFrequencyScale.Day;
            }
            else
            {
                string[] values = value.Split(valueSeparator);
                if (values.Length != 2)
                    throw new ArgumentException("Invalid value", "value");
                if (!int.TryParse(values[0], out this.rate))
                    throw new ArgumentException("Invalid rate value", "value");
                try
                {
                    int scaleInt;
                    if (!int.TryParse(values[1], out scaleInt))
                        throw new ArgumentException("Invalid scale value", "value");
                    this.scale = (CustomFrequencyScale)scaleInt;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("Invalid scale value", "value");
                }

            }
            this.invalidateDisplayValue = true;
        }

        protected override void UpdateDisplayValue()
        {
            string pattern = this.Rate == 1
                                 ? StringResources.CustomFrequency_Each
                                 : StringResources.CustomFrequency_Every;
            string scale = string.Empty;
            switch (this.Scale)
            {
                case CustomFrequencyScale.Day:
                    scale = this.Rate == 1 ? StringResources.CustomFrequency_Day : StringResources.CustomFrequency_Days;
                    break;
                case CustomFrequencyScale.Week:
                    scale = this.Rate == 1 ? StringResources.CustomFrequency_Week : StringResources.CustomFrequency_Weeks;
                    break;
                case CustomFrequencyScale.Month:
                    scale = this.Rate == 1 ? StringResources.CustomFrequency_Month : StringResources.CustomFrequency_Months;
                    break;
                case CustomFrequencyScale.Year:
                    scale = this.Rate == 1 ? StringResources.CustomFrequency_Year : StringResources.CustomFrequency_Years;
                    break;
            }
            this.displayValue = string.Format(pattern, (this.Rate == 1 ? string.Empty : this.Rate + " ") + scale);
            this.invalidateDisplayValue = false;

        }
    }
}

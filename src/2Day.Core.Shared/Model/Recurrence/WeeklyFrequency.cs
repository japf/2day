using System;
using System.Globalization;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public class WeeklyFrequency : CustomFrequencyBase
    {
        #region Private fields
        private DateTime? referenceDate;
        #endregion

        #region Public properties
        public override FrequencyType FrequencyType
        {
            get { return FrequencyType.Weekly; }
        }

        public override string DefaultTitle
        {
            get { return StringResources.CustomFrequency_Weekly; }
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
            return fromDate.AddDays(7);
        }

        public override void SetReferenceDate(DateTime? date)
        {
            if (date != this.referenceDate)
            {
                this.referenceDate = date;
                this.invalidateDisplayValue = true;
            }
        }
        #endregion

        #region Private methods
        protected override void ParseValue(string value)
        {
            // Do nothing
        }

        protected override void UpdateDisplayValue()
        {
            if (this.referenceDate == null)
                this.displayValue = StringResources.CustomFrequency_Weekly;
            else
            {
                string dayString = this.referenceDate.Value.GetDayOfWeekString();
                this.displayValue = string.Format(CultureInfo.InvariantCulture, StringResources.CustomFrequency_Each,
                                                  dayString);

            }
            this.invalidateDisplayValue = false;
        }
        #endregion

        public override bool Equals(ICustomFrequency other)
        {
            return other is WeeklyFrequency;
        }
    }
}

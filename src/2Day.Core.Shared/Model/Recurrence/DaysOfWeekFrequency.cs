using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public class DaysOfWeekFrequency : CustomFrequencyBase
    {
        private bool isMonday;
        private bool isTuesday;
        private bool isWednesday;
        private bool isThursday;
        private bool isFriday;
        private bool isSaturday;
        private bool isSunday;

        public override FrequencyType FrequencyType
        {
            get { return FrequencyType.DaysOfWeek; }
        }

        public override string DefaultTitle
        {
            get { return StringResources.CustomFrequency_DaysOfWeekTitle; }
        }

        public override bool IsCustomizable
        {
            get { return true; }
        }

        public override bool IsValidValue
        {
            get { // at least one day selected
                return this.isMonday || 
                         this.isTuesday || 
                         this.isWednesday || 
                         this.isThursday || 
                         this.isFriday || 
                         this.IsSaturday || 
                         this.isSunday; 
            }
        }

        public bool IsMonday
        {
            get { return this.isMonday; }
            set
            {
                if (this.isMonday != value)
                {
                    this.isMonday = value;
                    this.UpdateValue();
                }
            }
        }

        public bool IsTuesday
        {
            get { return this.isTuesday; }
            set
            {
                if (this.isTuesday != value)
                {
                    this.isTuesday = value;
                    this.UpdateValue();
                }
            }
        }

        public bool IsWednesday
        {
            get { return this.isWednesday; }
            set
            {
                if (this.isWednesday != value)
                {
                    this.isWednesday = value;
                    this.UpdateValue();
                }
            }
        }

        public bool IsThursday
        {
            get { return this.isThursday; }
            set
            {
                if (this.isThursday != value)
                {
                    this.isThursday = value;
                    this.UpdateValue();
                }
            }
        }

        public bool IsFriday
        {
            get { return this.isFriday; }
            set
            {
                if (this.isFriday != value)
                {
                    this.isFriday = value;
                    this.UpdateValue();
                }
            }
        }

        public bool IsSaturday
        {
            get { return this.isSaturday; }
            set
            {
                if (this.isSaturday != value)
                {
                    this.isSaturday = value;
                    this.UpdateValue();
                }
            }
        }

        public bool IsSunday
        {
            get { return this.isSunday; }
            set
            {
                if (this.isSunday != value)
                {
                    this.isSunday = value;
                    this.UpdateValue();
                }
            }
        }

        public override DateTime ComputeNextDate(DateTime fromDate)
        {
            if (!this.IsValidValue)
            {
                TrackingManagerHelper.Trace("Unable to compute next date of days of week frequency because it's not valid");
                return fromDate;
            }

            DateTime currentDate = fromDate;
            bool dateOk = false;
            while (!dateOk)
            {
                currentDate = currentDate.AddDays(1);
                // Not very elegant... but working
                if ((currentDate.DayOfWeek == DayOfWeek.Monday && this.isMonday) ||
                    (currentDate.DayOfWeek == DayOfWeek.Tuesday && this.isTuesday) ||
                    (currentDate.DayOfWeek == DayOfWeek.Wednesday && this.isWednesday) ||
                    (currentDate.DayOfWeek == DayOfWeek.Thursday && this.isThursday) ||
                    (currentDate.DayOfWeek == DayOfWeek.Friday && this.isFriday) ||
                    (currentDate.DayOfWeek == DayOfWeek.Saturday && this.isSaturday) ||
                    (currentDate.DayOfWeek == DayOfWeek.Sunday && this.isSunday))
                {
                    dateOk = true;
                }
            }
            return currentDate;
        }

        private void UpdateValue()
        {
            this.value = string.Concat(this.isMonday.ToString(),
                                        valueSeparator,
                                        this.isTuesday.ToString(),
                                        valueSeparator,
                                        this.isWednesday.ToString(),
                                        valueSeparator,
                                        this.isThursday.ToString(),
                                        valueSeparator,
                                        this.isFriday.ToString(),
                                        valueSeparator,
                                        this.isSaturday.ToString(),
                                        valueSeparator,
                                        this.isSunday.ToString()
                                    );

            this.invalidateDisplayValue = true;
        }

        private void Reset()
        {
            this.isMonday = false;
            this.isTuesday = false;
            this.isWednesday = false;
            this.isThursday = false;
            this.isFriday = false;
            this.isSaturday = false;
            this.isSunday = false;
        }

        protected override void ParseValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // Should occur only at initialization
                this.Reset();
            }
            else
            {
                string[] values = value.Split(valueSeparator);
                if (values.Length != 7)
                    throw new ArgumentException("Invalid value", "value");
                bool error = false;
                error |= !bool.TryParse(values[0], out this.isMonday);
                error |= !bool.TryParse(values[1], out this.isTuesday);
                error |= !bool.TryParse(values[2], out this.isWednesday);
                error |= !bool.TryParse(values[3], out this.isThursday);
                error |= !bool.TryParse(values[4], out this.isFriday);
                error |= !bool.TryParse(values[5], out this.isSaturday);
                error |= !bool.TryParse(values[6], out this.isSunday);
                if (error)
                    throw new ArgumentException("Invalid value", "value");
            }
            this.invalidateDisplayValue = true;
        }

        protected override void UpdateDisplayValue()
        {
            List<string> selectedDays = new List<string>();
            if (this.isMonday) selectedDays.Add(DateTimeExtensions.MondayShortString);
            if (this.isTuesday) selectedDays.Add(DateTimeExtensions.TuesdayShortString);
            if (this.isWednesday) selectedDays.Add(DateTimeExtensions.WednesdayShortString);
            if (this.isThursday) selectedDays.Add(DateTimeExtensions.ThursdayShortString);
            if (this.isFriday) selectedDays.Add(DateTimeExtensions.FridayShortString);
            if (this.isSaturday) selectedDays.Add(DateTimeExtensions.SaturdayShortString);
            if (this.isSunday) selectedDays.Add(DateTimeExtensions.SundayShortString);

            if (selectedDays.Any())
            {
                if (selectedDays.Count == 7)
                    this.displayValue = string.Format(StringResources.CustomFrequency_Each,
                                                      StringResources.CustomFrequency_Day);
                else if (selectedDays.Count == 2 && this.isSaturday && this.isSunday)
                    this.displayValue = string.Format(StringResources.CustomFrequency_Every,
                                                      StringResources.CustomFrequency_Weekend);
                else if (selectedDays.Count == 5 && !this.isSaturday && !this.isSunday)
                    this.displayValue = string.Format(StringResources.CustomFrequency_Every,
                                                      StringResources.CustomFrequency_Weekday);
                else
                {
                    this.displayValue = string.Format(StringResources.CustomFrequency_Every, string.Join(", ", selectedDays));
                }
            }
            else
            {
                this.displayValue = string.Empty;
            }
            this.invalidateDisplayValue = false;

        }
    }
}

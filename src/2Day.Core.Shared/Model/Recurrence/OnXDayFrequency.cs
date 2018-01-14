using System;
using System.Globalization;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public class OnXDayFrequency : CustomFrequencyBase
    {
        private DayOfWeek dayOfWeek;
        private RankingPosition rankingPosition;

        public DayOfWeek DayOfWeek
        {
            get { return this.dayOfWeek; }
            set
            {
                if (this.dayOfWeek != value)
                {
                    this.dayOfWeek = value;
                    this.UpdateValue();
                }
            }
        }

        public RankingPosition RankingPosition
        {
            get { return this.rankingPosition; }
            set
            {
                if (this.rankingPosition != value)
                {
                    this.rankingPosition = value;
                    this.UpdateValue();
                }
            }
        }

        public override FrequencyType FrequencyType
        {
            get { return FrequencyType.OnXDayOfEachMonth; }
        }

        public override string DefaultTitle
        {
            get { return StringResources.CustomFrequency_XthDayOfTheMonthTitle; }
        }

        public override bool IsCustomizable
        {
            get { return true; }
        }

        public override bool IsValidValue
        {
            get { return true; }
        }

        public override DateTime ComputeNextDate(DateTime fromDate)
        {
            DateTime currentMonthDate = GetCorrespondingDayInCurrentMonth(this.RankingPosition, this.DayOfWeek, fromDate);
            if (currentMonthDate > fromDate)
                return currentMonthDate;

            // We return the corresponding date for next month
            return GetCorrespondingDayInCurrentMonth(this.RankingPosition, this.DayOfWeek, fromDate.AddMonths(1));
        }

        private static DateTime GetCorrespondingDayInCurrentMonth(RankingPosition rankingPosition, DayOfWeek dayOfWeek, DateTime currentMonth)
        {
            // Look at the first occurence of the day of week in the month
            DateTime matchingDayOfWeek = new DateTime(currentMonth.Year, currentMonth.Month, 1, currentMonth.Hour, currentMonth.Minute, currentMonth.Second);
            while (matchingDayOfWeek.DayOfWeek != dayOfWeek)
                matchingDayOfWeek = matchingDayOfWeek.AddDays(1);
            // Here we have the correct day of week
            // Now we add the number of weeks corresponding to the rankingPosition parameter value
            DateTime correspondingDate = matchingDayOfWeek;
            switch (rankingPosition)
            {
                case RankingPosition.First:
                    // Do nothing
                    break;
                case RankingPosition.Second:
                    correspondingDate = correspondingDate.AddDays(7);
                    break;
                case RankingPosition.Third:
                    correspondingDate = correspondingDate.AddDays(14);
                    break;
                case RankingPosition.Fourth:
                    correspondingDate = correspondingDate.AddDays(21);
                    break;
                case RankingPosition.Last:
                    // Try to add 4 weeks
                    correspondingDate = correspondingDate.AddDays(28);
                    if (correspondingDate.Month > currentMonth.Month)
                        // If we are on the next month, subtract 1 week
                        correspondingDate = correspondingDate.AddDays(-7);
                    break;
            }
            return correspondingDate;

        }

        private void UpdateValue()
        {
            this.value = String.Concat((int)this.RankingPosition,
                                        valueSeparator,
                                        (int)this.DayOfWeek
                                    );
            this.invalidateDisplayValue = true;
        }

        protected override void UpdateDisplayValue()
        {
            string positionString = GetStringValue(this.RankingPosition);
            string dayString = DateTimeExtensions.GetDayOfWeekString(this.DayOfWeek);
            this.displayValue = string.Format(CultureInfo.InvariantCulture,
                                              StringResources.CustomFrequency_XthDayOfTheMonth, positionString,
                                              dayString);
            this.invalidateDisplayValue = false;
        }

        protected override void ParseValue(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                this.rankingPosition = RankingPosition.First;
                this.dayOfWeek = DayOfWeek.Monday;
            }
            else
            {
                string[] values = value.Split(valueSeparator);
                if (values.Length != 2)
                    throw new ArgumentException("Invalid value", "value");
                try
                {
                    int rankingInt;
                    if (!int.TryParse(values[0], out rankingInt))
                        throw new ArgumentException("Invalid scale value", "value");
                    this.rankingPosition = (RankingPosition)rankingInt;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("Invalid scale value", "value");
                }
                try
                {
                    int dayInt;
                    if (!int.TryParse(values[1], out dayInt))
                        throw new ArgumentException("Invalid scale value", "value");
                    this.dayOfWeek = (DayOfWeek)dayInt;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("Invalid scale value", "value");
                }
            }
            this.invalidateDisplayValue = true;
        }

        private static string GetStringValue(RankingPosition position)
        {
            switch (position)
            {
                case RankingPosition.First:
                    return StringResources.CustomFrequency_First;
                case RankingPosition.Second:
                    return StringResources.CustomFrequency_Second;
                case RankingPosition.Third:
                    return StringResources.CustomFrequency_Third;
                case RankingPosition.Fourth:
                    return StringResources.CustomFrequency_Fourth;
                case RankingPosition.Last:
                    return StringResources.CustomFrequency_Last;
            }
            // We never go here
            return String.Empty;
        }
    }
}

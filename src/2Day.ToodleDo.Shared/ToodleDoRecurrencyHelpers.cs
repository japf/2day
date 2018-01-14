using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Chartreuse.Today.Core.Shared.Model.Recurrence;

namespace Chartreuse.Today.ToodleDo
{
    public class ToodleDoRecurrencyHelpers
    {
        public static string GetToodleDoRecurrency(ICustomFrequency frequency)
        {
            if (frequency == null)
                return ToodleDoConstants.RecurrencyNone;

            switch (frequency.FrequencyType)
            {
                case FrequencyType.Once:
                    return ToodleDoConstants.RecurrencyNone;
                case FrequencyType.Daily:
                    return ToodleDoConstants.RecurrencyDaily;
                case FrequencyType.Weekly:
                    return ToodleDoConstants.RecurrencyWeekly;
                case FrequencyType.Monthly:
                    return ToodleDoConstants.RecurrencyMonthly;
                case FrequencyType.Yearly:
                    return ToodleDoConstants.RecurrencyYearly;
                case FrequencyType.DaysOfWeek:
                    return GetToodleDoDaysOfWeekRecurrency(frequency);
                case FrequencyType.EveryXPeriod:
                    return GetToodleDoEveryRecurrency(frequency);
                case FrequencyType.OnXDayOfEachMonth:
                    return GetToodleDoOnXDayRecurrency(frequency);
            }
            return ToodleDoConstants.RecurrencyNone;
        }

        private static string GetToodleDoDaysOfWeekRecurrency(ICustomFrequency customFrequency)
        {
            DaysOfWeekFrequency frequency = customFrequency as DaysOfWeekFrequency;
            if (frequency == null)
                return ToodleDoConstants.RecurrencyNone;
            // Join all selected days of week
            List<string> selectedDays = new List<string>();
            if (frequency.IsMonday)
                selectedDays.Add(ToodleDoConstants.RecurrencyMonday);
            if (frequency.IsTuesday)
                selectedDays.Add(ToodleDoConstants.RecurrencyTuesday);
            if (frequency.IsWednesday)
                selectedDays.Add(ToodleDoConstants.RecurrencyWednesday);
            if (frequency.IsThursday)
                selectedDays.Add(ToodleDoConstants.RecurrencyThursday);
            if (frequency.IsFriday)
                selectedDays.Add(ToodleDoConstants.RecurrencyFriday);
            if (frequency.IsSaturday)
                selectedDays.Add(ToodleDoConstants.RecurrencySaturday);
            if (frequency.IsSunday)
                selectedDays.Add(ToodleDoConstants.RecurrencySunday);

            string daysString = String.Empty;
            if (selectedDays.Count == 7)
                daysString = ToodleDoConstants.RecurrencyDay;
            else if (selectedDays.Count == 2 && frequency.IsSaturday && frequency.IsSunday)
                daysString = ToodleDoConstants.RecurrencyWeekend;
            else if (selectedDays.Count == 5 && !frequency.IsSaturday && !frequency.IsSunday)
                daysString = ToodleDoConstants.RecurrencyWeekday;
            else
            {
                daysString = string.Join(ToodleDoConstants.RecurrencyDaySeparator, selectedDays);
            }

            return string.Format(CultureInfo.InvariantCulture, ToodleDoConstants.RecurrencyDaysOfWeekPattern, daysString);
        }

        private static string GetToodleDoEveryRecurrency(ICustomFrequency customFrequency)
        {
            EveryXPeriodFrequency frequency = customFrequency as EveryXPeriodFrequency;
            if (frequency == null)
                return ToodleDoConstants.RecurrencyNone;

            string periodString = String.Empty;
            switch (frequency.Scale)
            {
                case CustomFrequencyScale.Day:
                    periodString = frequency.Rate > 1
                                       ? ToodleDoConstants.RecurrencyDay
                                       : ToodleDoConstants.RecurrencyDays;
                    break;

                case CustomFrequencyScale.Week:
                    periodString = frequency.Rate > 1
                                       ? ToodleDoConstants.RecurrencyWeek
                                       : ToodleDoConstants.RecurrencyWeeks;
                    break;
                case CustomFrequencyScale.Month:
                    periodString = frequency.Rate > 1
                                       ? ToodleDoConstants.RecurrencyMonth
                                       : ToodleDoConstants.RecurrencyMonths;
                    break;

                case CustomFrequencyScale.Year:
                    periodString = frequency.Rate > 1
                                       ? ToodleDoConstants.RecurrencyYear
                                       : ToodleDoConstants.RecurrencyYears;
                    break;

            }
            return string.Format(CultureInfo.InvariantCulture, ToodleDoConstants.RecurrencyEveryPeriodPattern, frequency.Rate.ToString(), periodString);
        }

        private static string GetToodleDoOnXDayRecurrency(ICustomFrequency customFrequency)
        {
            OnXDayFrequency frequency = customFrequency as OnXDayFrequency;
            if (frequency == null)
                return ToodleDoConstants.RecurrencyNone;

            string positionString = String.Empty;
            switch (frequency.RankingPosition)
            {
                case RankingPosition.First:
                    positionString = ToodleDoConstants.RecurrencyFirst;
                    break;
                case RankingPosition.Second:
                    positionString = ToodleDoConstants.RecurrencySecond;
                    break;
                case RankingPosition.Third:
                    positionString = ToodleDoConstants.RecurrencyThird;
                    break;
                case RankingPosition.Fourth:
                    positionString = ToodleDoConstants.RecurrencyFourth;
                    break;
                case RankingPosition.Last:
                    positionString = ToodleDoConstants.RecurrencyLast;
                    break;
            }
            return string.Format(
                CultureInfo.InvariantCulture, 
                ToodleDoConstants.RecurrencyOnXDayPattern, 
                positionString, 
                GetDayOfWeekString(frequency.DayOfWeek));
        }

        private static string GetDayOfWeekString(DayOfWeek day)
        {
            string dayString = String.Empty;
            switch (day)
            {
                case DayOfWeek.Monday:
                    return ToodleDoConstants.RecurrencyMonday;
                case DayOfWeek.Tuesday:
                    return ToodleDoConstants.RecurrencyTuesday;
                case DayOfWeek.Wednesday:
                    return ToodleDoConstants.RecurrencyWednesday;
                case DayOfWeek.Thursday:
                    return ToodleDoConstants.RecurrencyThursday;
                case DayOfWeek.Friday:
                    return ToodleDoConstants.RecurrencyFriday;
                case DayOfWeek.Saturday:
                    return ToodleDoConstants.RecurrencySaturday;
                case DayOfWeek.Sunday:
                    return ToodleDoConstants.RecurrencySunday;
            }
            return dayString;
        }

        private static IList<Func<string, ICustomFrequency>> toodleDoFrequencyConverters = new List<Func<string, ICustomFrequency>>
            {
                Get2DayNoneFrequency,
                Get2DayStandardFrequency,
                Get2DayDaysOfWeekFrequency,
                Get2DayOnXDayFrequency,
                Get2DayEveryPeriodFrequency
            };

        public static ICustomFrequency Get2DayRecurrency(string toodleDoFrequency)
        {
            ICustomFrequency result = null;
            foreach (var func in toodleDoFrequencyConverters)
            {
                result = func(toodleDoFrequency);
                if (result != null)
                    return result;
            }
            Debug.WriteLine("Could not detect ToodleDo recurrency '{0}", toodleDoFrequency);
            return new OnceOnlyFrequency();

        }

        private static ICustomFrequency Get2DayNoneFrequency(string toodleDoFrequency)
        {
            if (String.IsNullOrEmpty(toodleDoFrequency) || ToodleDoConstants.RecurrencyToodleDoNone.Equals(toodleDoFrequency, StringComparison.OrdinalIgnoreCase))
                return new OnceOnlyFrequency();
            return null;
        }

        private static ICustomFrequency Get2DayStandardFrequency(string toodleDoFrequency)
        {
            if (ToodleDoConstants.RecurrencyToodleDoDaily.Equals(toodleDoFrequency, StringComparison.OrdinalIgnoreCase))
                return new DailyFrequency();
            if (ToodleDoConstants.RecurrencyToodleDoWeekly.Equals(toodleDoFrequency, StringComparison.OrdinalIgnoreCase))
                return new WeeklyFrequency();
            if (ToodleDoConstants.RecurrencyToodleDoBiweekly.Equals(toodleDoFrequency, StringComparison.OrdinalIgnoreCase))
                return new EveryXPeriodFrequency()
                    {
                        Rate = 2,
                        Scale = CustomFrequencyScale.Week
                    };
            if (ToodleDoConstants.RecurrencyToodleDoMonthly.Equals(toodleDoFrequency, StringComparison.OrdinalIgnoreCase))
                return new MonthlyFrequency();
            if (ToodleDoConstants.RecurrencyToodleDoBimonthly.Equals(toodleDoFrequency, StringComparison.OrdinalIgnoreCase))
                return new EveryXPeriodFrequency()
                {
                    Rate = 2,
                    Scale = CustomFrequencyScale.Month
                };
            if (ToodleDoConstants.RecurrencyToodleDoQuarterly.Equals(toodleDoFrequency, StringComparison.OrdinalIgnoreCase))
                return new EveryXPeriodFrequency()
                {
                    Rate = 3,
                    Scale = CustomFrequencyScale.Month
                };
            if (ToodleDoConstants.RecurrencyToodleDoSemiannually.Equals(toodleDoFrequency, StringComparison.OrdinalIgnoreCase))
                return new EveryXPeriodFrequency()
                {
                    Rate = 6,
                    Scale = CustomFrequencyScale.Month
                };
            if (ToodleDoConstants.RecurrencyToodleDoYearly.Equals(toodleDoFrequency, StringComparison.OrdinalIgnoreCase))
                return new YearlyFrequency();

            return null;
        }

        private static ICustomFrequency Get2DayOnXDayFrequency(string toodleDoFrequency)
        {
            Regex regex = new Regex(ToodleDoConstants.RecurrencyToodleDoOnXDayRegex, RegexOptions.IgnoreCase);
            Match match = regex.Match(toodleDoFrequency);
            if (match == null || !match.Success)
                return null;
            OnXDayFrequency frequency = new OnXDayFrequency();
            string position = match.Groups["Position"].Value;
            string day = match.Groups["DayOfWeek"].Value;

            #region Position
            if (position.Equals(ToodleDoConstants.RecurrencyFirstShort, StringComparison.OrdinalIgnoreCase))
                frequency.RankingPosition = RankingPosition.First;
            else if (position.Equals(ToodleDoConstants.RecurrencySecondShort, StringComparison.OrdinalIgnoreCase))
                frequency.RankingPosition = RankingPosition.Second;
            else if (position.Equals(ToodleDoConstants.RecurrencyThirdShort, StringComparison.OrdinalIgnoreCase))
                frequency.RankingPosition = RankingPosition.Third;
            else if (position.Equals(ToodleDoConstants.RecurrencyFourthShort, StringComparison.OrdinalIgnoreCase))
                frequency.RankingPosition = RankingPosition.Fourth;
            else if (position.Equals(ToodleDoConstants.RecurrencyFifthShort, StringComparison.OrdinalIgnoreCase))
                frequency.RankingPosition = RankingPosition.Last;
            else if (position.Equals(ToodleDoConstants.RecurrencyLast, StringComparison.OrdinalIgnoreCase))
                frequency.RankingPosition = RankingPosition.Last;
            else
                return null;
            #endregion

            #region Day

            DayOfWeek? dayOfWeek = GetDayOfWeekFromShortString(day);
            if (!dayOfWeek.HasValue)
                return null;
            frequency.DayOfWeek = dayOfWeek.Value;

            #endregion

            return frequency;
        }

        private static ICustomFrequency Get2DayDaysOfWeekFrequency(string toodleDoFrequency)
        {
            Regex regex = new Regex(ToodleDoConstants.RecurrencyToodleDoDaysOfWeekRegex, RegexOptions.IgnoreCase);
            Match match = regex.Match(toodleDoFrequency);
            if (match == null || !match.Success)
                return null;
            DaysOfWeekFrequency frequency = new DaysOfWeekFrequency();
            CaptureCollection days = match.Groups["DayOfWeek"].Captures;
            foreach (var day in days)
            {
                DayOfWeek? dayOfWeek = GetDayOfWeekFromShortString(day.ToString());
                if (dayOfWeek != null)
                {
                    switch (dayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            frequency.IsMonday = true;
                            break;
                        case DayOfWeek.Tuesday:
                            frequency.IsTuesday = true;
                            break;
                        case DayOfWeek.Wednesday:
                            frequency.IsWednesday = true;
                            break;
                        case DayOfWeek.Thursday:
                            frequency.IsThursday = true;
                            break;
                        case DayOfWeek.Friday:
                            frequency.IsFriday = true;
                            break;
                        case DayOfWeek.Saturday:
                            frequency.IsSaturday = true;
                            break;
                        case DayOfWeek.Sunday:
                            frequency.IsSunday = true;
                            break;

                    }
                }
                else if (day.ToString().Equals(ToodleDoConstants.RecurrencyWeekday, StringComparison.OrdinalIgnoreCase))
                {
                    frequency.IsMonday = true;
                    frequency.IsTuesday = true;
                    frequency.IsWednesday = true;
                    frequency.IsThursday = true;
                    frequency.IsFriday = true;
                }
                else if (day.ToString().Equals(ToodleDoConstants.RecurrencyWeekend, StringComparison.OrdinalIgnoreCase))
                {
                    frequency.IsSaturday = true;
                    frequency.IsSunday = true;
                }
            }
            return frequency;
        }

        private static ICustomFrequency Get2DayEveryPeriodFrequency(string toodleDoFrequency)
        {
            Regex regex = new Regex(ToodleDoConstants.RecurrencyToodleDoEveryPeriodRegex, RegexOptions.IgnoreCase);
            Match match = regex.Match(toodleDoFrequency);
            if (match == null || !match.Success)
                return null;
            int rate = int.Parse(match.Groups["Rate"].Value);
            string scaleString = match.Groups["Scale"].Value;
            CustomFrequencyScale scale = GetFrequencyScaleFromString(scaleString);
            if (scale == CustomFrequencyScale.None)
                return null;

            if (rate <= 1)
            {
                // We've got a standard frequency
                switch (scale)
                {
                    case CustomFrequencyScale.Day:
                        return new DailyFrequency();
                    case CustomFrequencyScale.Week:
                        return new WeeklyFrequency();
                    case CustomFrequencyScale.Month:
                        return new MonthlyFrequency();
                    case CustomFrequencyScale.Year:
                        return new YearlyFrequency();
                    default:
                        return null;
                }
            }
            else
            {
                EveryXPeriodFrequency frequency = new EveryXPeriodFrequency();
                frequency.Rate = rate;
                frequency.Scale = scale;
                return frequency;

            }
        }

        private static CustomFrequencyScale GetFrequencyScaleFromString(string scale)
        {
            if (scale.Equals(ToodleDoConstants.RecurrencyDay, StringComparison.OrdinalIgnoreCase))
                return CustomFrequencyScale.Day;
            else if (scale.Equals(ToodleDoConstants.RecurrencyWeek, StringComparison.OrdinalIgnoreCase))
                return CustomFrequencyScale.Week;
            else if (scale.Equals(ToodleDoConstants.RecurrencyMonth, StringComparison.OrdinalIgnoreCase))
                return CustomFrequencyScale.Month;
            else if (scale.Equals(ToodleDoConstants.RecurrencyYear, StringComparison.OrdinalIgnoreCase))
                return CustomFrequencyScale.Year;
            return CustomFrequencyScale.None;
        }

        private static DayOfWeek? GetDayOfWeekFromShortString(string shortDayString)
        {
            if (shortDayString.Equals(ToodleDoConstants.RecurrencyMonday, StringComparison.OrdinalIgnoreCase))
                return DayOfWeek.Monday;
            else if (shortDayString.Equals(ToodleDoConstants.RecurrencyTuesday, StringComparison.OrdinalIgnoreCase))
                return DayOfWeek.Tuesday;
            else if (shortDayString.Equals(ToodleDoConstants.RecurrencyWednesday, StringComparison.OrdinalIgnoreCase))
                return DayOfWeek.Wednesday;
            else if (shortDayString.Equals(ToodleDoConstants.RecurrencyThursday, StringComparison.OrdinalIgnoreCase))
                return DayOfWeek.Thursday;
            else if (shortDayString.Equals(ToodleDoConstants.RecurrencyFriday, StringComparison.OrdinalIgnoreCase))
                return DayOfWeek.Friday;
            else if (shortDayString.Equals(ToodleDoConstants.RecurrencySaturday, StringComparison.OrdinalIgnoreCase))
                return DayOfWeek.Saturday;
            else if (shortDayString.Equals(ToodleDoConstants.RecurrencySunday, StringComparison.OrdinalIgnoreCase))
                return DayOfWeek.Sunday;
            return null;
        }
    }
}

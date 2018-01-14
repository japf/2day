using System;
using System.Collections.Generic;
using System.Globalization;

namespace Chartreuse.Today.Core.Shared.Tools.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly string[] ReplaceYearPatterns = new string[]
        {
            "/yyyy",
            "yyyy/",
            ".yyyy",
            "yyyy.",
            " yyyy",
            "yyyy ",
            "yyyy-",
            "-yyyy",
            "/yy",
            "yy/"        
        };

        private const string MonthFormatAbbreviation = "MMM";
        private const string MonthFormatFull = "MMMM";
        private const string DayFormatLong = "dddd ";

        private const char CharSingleSlash = '/';
        private const char CharSingleDot = '.';
        private const char CharWhitespace = ' ';
        private const string FormatMonthNumber2 = "MM";
        private const string FormatMonthNumber1 = "M";

        private static readonly DateTime StartUnixDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private static readonly string mondayString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Monday].FirstLetterToUpper();
        private static readonly string tuesdayString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Tuesday].FirstLetterToUpper();
        private static readonly string wednesdayString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Wednesday].FirstLetterToUpper();
        private static readonly string thursdayString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Thursday].FirstLetterToUpper();
        private static readonly string fridayString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Friday].FirstLetterToUpper();
        private static readonly string saturdayString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Saturday].FirstLetterToUpper();
        private static readonly string sundayString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Sunday].FirstLetterToUpper();

        private static readonly string mondayShortString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Monday].FirstLetterToUpper();
        private static readonly string tuesdayShortString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Tuesday].FirstLetterToUpper();
        private static readonly string wednesdayShortString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Wednesday].FirstLetterToUpper();
        private static readonly string thursdayShortString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Thursday].FirstLetterToUpper();
        private static readonly string fridayShortString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Friday].FirstLetterToUpper();
        private static readonly string saturdayShortString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Saturday].FirstLetterToUpper();
        private static readonly string sundayShortString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DayOfWeek.Sunday].FirstLetterToUpper();

        public static string MondayString { get { return mondayString; } }
        public static string TuesdayString { get { return tuesdayString; } }
        public static string WednesdayString { get { return wednesdayString; } }
        public static string ThursdayString { get { return thursdayString; } }
        public static string FridayString { get { return fridayString; } }
        public static string SaturdayString { get { return saturdayString; } }
        public static string SundayString { get { return sundayString; } }

        public static IEnumerable<string> DaysOfTheWeek
        {
            get { return new[] { mondayString, tuesdayString, wednesdayString, thursdayString, fridayString, saturdayString, sundayString }; }
        }

        public static string MondayShortString { get { return mondayShortString; } }
        public static string TuesdayShortString { get { return tuesdayShortString; } }
        public static string WednesdayShortString { get { return wednesdayShortString; } }
        public static string ThursdayShortString { get { return thursdayShortString; } }
        public static string FridayShortString { get { return fridayShortString; } }
        public static string SaturdayShortString { get { return saturdayShortString; } }
        public static string SundayShortString { get { return sundayShortString; } }

        public static string FormatWeekDayMonthDay(this DateTime dateTime)
        {
            // format is "Tue 03 08"
            return string.Format("{0} {1}", dateTime.ToString("ddd"), dateTime.FormatMonthDay(MonthDisplay.Number, dateTime.Year != DateTime.Now.Year));
        }

        public static string FormatMonthDay(this DateTime dateTime, MonthDisplay monthDisplay, bool includeYear = false)
        {
            string pattern = DateTimeFormatInfo.CurrentInfo != null
                                 ? DateTimeFormatInfo.CurrentInfo.ShortDatePattern
                                 : DateTimeFormatInfo.InvariantInfo.ShortDatePattern;

            // short date pattern examples:
            //  CULTURE   PROPERTY VALUE
            //  en-US     M/d/yyyy
            //  ja-JP     yyyy/MM/dd
            //  fr-FR     dd/MM/yyyy

            // remove the year pattern from the pattern
            if (!includeYear)
            {
                foreach (string replaceYearPattern in ReplaceYearPatterns)
                {
                    if (pattern.Contains(replaceYearPattern))
                        pattern = pattern.Replace(replaceYearPattern, string.Empty);
                }                
            }

            if (monthDisplay == MonthDisplay.Abbreviation)
            {
                pattern = ReplaceMonthFormat(pattern, MonthFormatAbbreviation);
                pattern = pattern.Replace(CharSingleSlash, CharWhitespace);
                pattern = pattern.Replace(CharSingleDot, CharWhitespace);

            }
            else if (monthDisplay == MonthDisplay.Full)
            {
                pattern = ReplaceMonthFormat(pattern, MonthFormatFull);
                pattern = pattern.Replace(CharSingleSlash, CharWhitespace);
                pattern = pattern.Replace(CharSingleDot, CharWhitespace);
                pattern = DayFormatLong + pattern;
            }

            return dateTime.ToString(pattern).FirstLetterToUpper();
        }

        /// <summary>
        /// Returns a long string representation of a nullable datetime. If the year of the input parameter
        /// is the same has the current year ("now"), year is ommited
        /// </summary>
        /// <param name="datetime">Input datetime</param>
        /// <param name="includeTime">Include time</param>
        /// <returns>Long string representation of the input datetime</returns>
        public static string FormatLong(this DateTime? datetime, bool includeTime)
        {
            if (!datetime.HasValue)
                return string.Empty;

            return datetime.Value.FormatLong(includeTime);
        }

        /// <summary>
        /// Returns a long string representation of a nullable datetime. If the year of the input parameter
        /// is the same has the current year ("now"), year is ommited
        /// </summary>
        /// <param name="datetime">Input datetime</param>
        /// <param name="includeTime">Include time</param>
        /// <returns>Long string representation of the input datetime</returns>
        public static string FormatLong(this DateTime datetime, bool includeTime)
        {
            if (datetime.Year == DateTime.UtcNow.Year)
            {
                if (includeTime)
                    return string.Format("{0} {1} {2}", datetime.ToString("dddd"), datetime.ToString("M"), datetime.ToString("t"));

                return string.Format("{0} {1}", datetime.ToString("dddd"), datetime.ToString("M"));            
            }
            else
            {
                if (includeTime)
                    return datetime.ToString("f");
                
                return datetime.ToString("D");
            }
        }

        private static string ReplaceMonthFormat(string input, string change)
        {
            if (input.Contains(FormatMonthNumber2))
                return input.Replace(FormatMonthNumber2, change);
            else if (input.Contains(FormatMonthNumber1))
                return input.Replace(FormatMonthNumber1, change);

            return input;
        }

        public static DateTime? TryParseNullableTimestamp(this string timestamp, bool convertLocal = true)
        {
            int t;
            if (Int32.TryParse(timestamp, out t))
            {
                var date = TimestampToDateTime(t);
                if (date.Date == StartUnixDateTime.Date)
                    return null;

                if (convertLocal)
                    return date.ToLocalTime();
                else
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Local);
            }

            return null;
        }

        public static DateTime TryParseTimestamp(this string timestamp)
        {
            var date = TryParseNullableTimestamp(timestamp);
            if (date == null)
                return DateTime.MinValue;

            return date.Value;
        }

        public static DateTime TimestampToDateTime(int timestamp)
        {
            return StartUnixDateTime.AddSeconds((double)timestamp);
        }

        public static int DateTimeToTimestamp(this DateTime date)
        {
            TimeSpan diff = date - StartUnixDateTime;
            return (int)Math.Floor(diff.TotalSeconds);
        }

        public static int TimeToUnixTimestamp(this DateTime dateTime)
        {
            double seconds = dateTime.TimeOfDay.TotalSeconds;
            return StartUnixDateTime.AddSeconds(seconds).DateTimeToTimestamp();
        }

        public static string GetDayOfWeekString(this DateTime date)
        {
            return GetDayOfWeekString(date.DayOfWeek);
        }

        public static DayOfWeek GetDayOfWeek(string input)
        {
            if (input == mondayString || input == mondayShortString)
                return DayOfWeek.Monday;
            if (input == tuesdayString || input == tuesdayShortString)
                return DayOfWeek.Tuesday;
            if (input == wednesdayString || input == wednesdayShortString)
                return DayOfWeek.Wednesday;
            if (input == thursdayString || input == thursdayShortString)
                return DayOfWeek.Thursday;
            if (input == fridayString || input == fridayShortString)
                return DayOfWeek.Friday;
            if (input == saturdayString || input == saturdayShortString)
                return DayOfWeek.Saturday;
            
            return DayOfWeek.Sunday;
        }

        public static string GetDayOfWeekString(DayOfWeek day)
        {
            string dayString = String.Empty;
            switch (day)
            {
                case DayOfWeek.Monday:
                    dayString = MondayString;
                    break;
                case DayOfWeek.Tuesday:
                    dayString = TuesdayString;
                    break;
                case DayOfWeek.Wednesday:
                    dayString = WednesdayString;
                    break;
                case DayOfWeek.Thursday:
                    dayString = ThursdayString;
                    break;
                case DayOfWeek.Friday:
                    dayString = FridayString;
                    break;
                case DayOfWeek.Saturday:
                    dayString = SaturdayString;
                    break;
                case DayOfWeek.Sunday:
                    dayString = SundayString;
                    break;
            }
            return dayString;
        }        
    }
}

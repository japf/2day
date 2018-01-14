using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.Core.Shared.Tools.Converter
{
    public class RelativeDateConverter
    {
        private static ISettings settings;
        private static Dictionary<double, Func<double, DateTime, DateTime, string>> convertGrouped;
        private static Dictionary<double, Func<double, DateTime, DateTime, string>> convertFlat;
        private readonly bool useGroupedDate;

        static RelativeDateConverter()
        {
            LoadResources();
        }

        public static void LoadResources()
        {
            const string format = " {0}";
            string formatYesterday = StringResources.ConverterDate_Yesterday + format;
            string formatToday = StringResources.ConverterDate_Today + format;
            string formatTomorrow = StringResources.ConverterDate_Tomorrow + format;

            convertGrouped = new Dictionary<double, Func<double, DateTime, DateTime, string>>
            {
                {0.0, (d, t, n) => StringResources.ConverterDate_Late},
                {1.0, (d, t, n) => StringResources.ConverterDate_Today},
                {2.0, (d, t, n) => StringResources.ConverterDate_Tomorrow},
                {8.0, (d, t, n) => StringResources.ConverterDate_Next7Days},
                {double.MaxValue, (d, t, n) => StringResources.ConverterDate_Future}
            };

            convertFlat = new Dictionary<double, Func<double, DateTime, DateTime, string>>
            {
                {-1.0, (d, t, n) => FormatDate(t, n)},
                {0.0, (d, t, n) => string.Format(formatYesterday, FormatDate(t, n))},
                {1.0, (d, t, n) => string.Format(formatToday, FormatDate(t, n))},
                {2.0, (d, t, n) => string.Format(formatTomorrow, FormatDate(t, n))},
                {double.MaxValue, (d, t, n) => FormatDate(t, n)}
            };
        }

        public RelativeDateConverter(ISettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            this.useGroupedDate = settings.GetValue<bool>(CoreSettings.UseGroupedDates);
        }

        public string Convert(DateTime? dateTime)
        {
            return ConverterRelativeCore(dateTime, useGroupedDate);
        }

        private static string FormatDate(DateTime date, DateTime now)
        {
            bool displayYear = date.Year != now.Year;
            if (displayYear)
                return date.FormatMonthDay(MonthDisplay.Full, includeYear: true);
            else
                return date.FormatMonthDay(MonthDisplay.Full, includeYear: false);
        }

        public static string ConvertRelative(DateTime? dateTime)
        {
            if (settings == null)
                settings = Ioc.Resolve<IWorkbook>().Settings;

            bool useGroupedDates = settings.GetValue<bool>(CoreSettings.UseGroupedDates);

            return ConverterRelativeCore(dateTime, useGroupedDates);
        }

        public static string ConverterRelativeCore(DateTime? dateTime, bool useGroupedDates)
        {
            if (!dateTime.HasValue)
                return StringResources.ConverterDate_NoDate;

            var today = DateTime.Today;
            if (StaticTestOverrides.Now.HasValue)
                today = StaticTestOverrides.Now.Value;

            TimeSpan difference = dateTime.Value - today;

            var dictionary = useGroupedDates ? convertGrouped : convertFlat;

            return dictionary.First(t => (difference.TotalDays < t.Key)).Value(difference.TotalDays, dateTime.Value, today);
        }
    }
}

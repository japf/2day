using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class StartDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value is DateTime)
            {
                var dateTime = (DateTime)value;
                if (dateTime.TimeOfDay == TimeSpan.Zero)
                    return dateTime.FormatWeekDayMonthDay();
                else
                    return dateTime.FormatWeekDayMonthDay() + " " + dateTime.ToString("t");
            }

            // not visible in the app, only used for x:Bind compatibility
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}

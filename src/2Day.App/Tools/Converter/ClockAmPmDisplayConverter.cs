using System;
using Windows.UI.Xaml.Data;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class ClockAmPmDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTime time = (DateTime)value;

            if (time.Hour >= 12)
                return "PM";

            return "AM";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
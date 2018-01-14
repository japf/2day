using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class LongDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value is DateTime)
            {
                var dateTime = ((DateTime)value);
                return string.Format("{0} {1}", dateTime.ToString("ddd"), dateTime.ToString("d"));
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
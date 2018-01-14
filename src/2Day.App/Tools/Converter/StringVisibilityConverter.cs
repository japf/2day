using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class StringVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!string.IsNullOrEmpty(value as string) || (value is double && (double)value > 0) || (value is int && (int)value > 0))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

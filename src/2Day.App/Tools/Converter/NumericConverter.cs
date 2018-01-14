using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class NumericConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                return (int) (((double) value)*100);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                int v = -1;
                if (int.TryParse((string)value, out v))
                    return (double)v / 100;
            }

            return DependencyProperty.UnsetValue;
        }
    }
}

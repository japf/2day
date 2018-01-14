using System;
using Windows.UI.Xaml.Data;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class TextUppercaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                return value.ToString().ToUpperInvariant();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

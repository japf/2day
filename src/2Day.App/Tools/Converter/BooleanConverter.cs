using System;
using Windows.UI.Xaml.Data;

namespace Chartreuse.Today.App.Tools.Converter
{
    /// <summary>
    /// Value converter that does nothing to a boolean but useful to use x:Bind binding with a nullable boolean
    /// </summary>
    public sealed class BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool)value);
        }
    }
}
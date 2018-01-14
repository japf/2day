using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Converter;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class WinRelativeDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime)
            {
                return RelativeDateConverter.ConvertRelative((DateTime) value);
            }
            else if (parameter as string == "ShowNoDate")
            {
                return StringResources.ConverterDate_NoDate;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

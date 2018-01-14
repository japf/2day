using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class SortOrderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SortOrder)
                return ((SortOrder)value).GetDescription();

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
                return Core.Shared.Model.SortOrderConverter.FromDescription((string)value);

            return value;
        }
    }
}

using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class SortMenuConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof (string))
            {
                return Core.Shared.Model.TaskGroupConverter.FromName((string)parameter).GetDescription();
            }
            else if (targetType == typeof (bool) && value is TaskGroup)
            {
                return ((TaskGroup)value).ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}

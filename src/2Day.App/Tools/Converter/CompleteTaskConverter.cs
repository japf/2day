using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class CompleteTaskConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && (bool) value)
                return StringResources.Action_Uncomplete;
            else
                return StringResources.Action_Complete;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class IsFolderToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IFolder)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

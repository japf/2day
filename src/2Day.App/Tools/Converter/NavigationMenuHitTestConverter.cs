using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.ViewModel;

namespace Chartreuse.Today.App.Tools.Converter
{
    public sealed class NavigationMenuHitTestConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FolderItemViewModel)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}

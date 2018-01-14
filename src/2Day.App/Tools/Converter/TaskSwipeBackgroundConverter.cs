using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class TaskSwipeBackgroundConverter : IValueConverter
    {
        private static SolidColorBrush completeBrush;
        private static SolidColorBrush uncompleteBrush;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (completeBrush == null)
            {
                completeBrush = (SolidColorBrush) Application.Current.Resources["ListViewActionBarCompleteBrush"];
                uncompleteBrush = (SolidColorBrush) Application.Current.Resources["ListViewActionBarUncompleteBrush"];
            }

            if (value is bool && (bool) value)
                return uncompleteBrush;
            else
                return completeBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
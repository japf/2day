using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class TaskForegroundConverter : IValueConverter
    {
        private static SolidColorBrush normalBrush;
        private static SolidColorBrush completedBrush;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (normalBrush == null)
            {
                normalBrush = Application.Current.Resources["TaskTitleForegroundBrush"] as SolidColorBrush;
                completedBrush = Application.Current.Resources["TaskTitleCompletedForegroundBrush"] as SolidColorBrush;
            }

            if (value is bool && (bool) value)
                return completedBrush;
            else
                return normalBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

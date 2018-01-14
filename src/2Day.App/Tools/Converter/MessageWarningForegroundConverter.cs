using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class MessageWarningForegroundConverter : IValueConverter
    {
        private static SolidColorBrush foregroundBrush;
        private static SolidColorBrush warningForegroundBrush;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (foregroundBrush == null)
            {
                foregroundBrush = Application.Current.Resources["ForegroundBrush"] as SolidColorBrush;
                warningForegroundBrush = Application.Current.Resources["ForegroundWarningBrush"] as SolidColorBrush;
            }

            if (value is bool && (bool) value)
                return warningForegroundBrush;
            else
                return foregroundBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Universal.Model;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class BooleanToForegroundConverter : IValueConverter
    {
        private static SolidColorBrush foregroundBrush;
        private static SolidColorBrush backgroundBrush;
        private static bool useDarkTheme;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (foregroundBrush == null)
            {
                if (WinSettings.Instance.GetValue<bool>(CoreSettings.UseDarkTheme))
                {
                    useDarkTheme = true;
                    foregroundBrush = new SolidColorBrush(Colors.White);
                    backgroundBrush = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    foregroundBrush = new SolidColorBrush(Colors.Black);
                    backgroundBrush = new SolidColorBrush(Colors.White);                    
                }
            }

            // special "noTheme" parameter for snapped mode (MainPage.xaml)
            if (parameter as string == "noTheme" && !useDarkTheme)
            {
                if (value is bool && (bool)value)
                    return backgroundBrush;
                else
                    return foregroundBrush;
            }
            else
            {
                if (value is bool && (bool)value)
                    return foregroundBrush;
                else
                    return backgroundBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

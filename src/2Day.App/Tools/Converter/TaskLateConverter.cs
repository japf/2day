using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class TaskLateConverter : IValueConverter
    {
        private static SolidColorBrush LateBrush;
        private static SolidColorBrush NormalBrush;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (LateBrush == null)
                LateBrush = Application.Current.Resources["TaskLateBorderBrush"] as SolidColorBrush;
            if (NormalBrush == null)
                NormalBrush = Application.Current.Resources["CheckBoxBorderThemeBrush"] as SolidColorBrush;

            if (value is bool && ((bool) value))
                return LateBrush;
            else
                return NormalBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

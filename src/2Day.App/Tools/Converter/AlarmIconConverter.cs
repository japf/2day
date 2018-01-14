using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Icons;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class AlarmIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime)
            {
                if ((DateTime) value < DateTime.Now)
                    return AppIconType.CommonAlarmDisabled;

                return AppIconType.CommonAlarm;
            }
            
            // not visible in the app, only used for x:Bind compatibility
            return AppIconType.CommonAddFolder;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

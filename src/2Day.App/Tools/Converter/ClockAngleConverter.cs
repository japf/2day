using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.App.Controls;

namespace UwpMaterialClock.Converters
{
    public class ClockAngleConverter : IValueConverter
    {
        public ClockItemMember ItemMember { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTime time = (DateTime)value;

            return this.ItemMember == ClockItemMember.Hours
                ? ((time.Hour > 13) ? time.Hour - 12 : time.Hour) * (360 / 12)
                : (time.Minute == 0 ? 60 : time.Minute) * (360 / 60);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
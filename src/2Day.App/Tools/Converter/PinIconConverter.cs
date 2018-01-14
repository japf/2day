using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class PinIconConverter : IValueConverter
    {
        private static readonly SymbolIcon pin = new SymbolIcon(Symbol.Pin);
        private static readonly SymbolIcon unpin = new SymbolIcon(Symbol.UnPin);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && ((bool) value))
                return unpin;
            else
                return pin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

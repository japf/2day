using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Universal.Model;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class LightToDarkConverter : IValueConverter
    {
        private static readonly bool useDarkTheme;

        static LightToDarkConverter()
        {
            useDarkTheme = WinSettings.Instance.GetValue<bool>(CoreSettings.UseDarkTheme);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                string code = (string) value;

                // special "selected" parameter for snapped mode (MainPage.xaml)
                if (parameter != null && parameter.Equals("selected"))
                {
                    if (code.Equals(ResourcesLocator.ViewColor) || code.Equals(ResourcesLocator.ContextColor))
                        return "white";

                }
                else
                {
                    if (code.Equals("white", StringComparison.OrdinalIgnoreCase) || code.Equals("#ffffff", StringComparison.OrdinalIgnoreCase) || code.Equals("#ffffffff", StringComparison.OrdinalIgnoreCase))
                        return "black";
                }

            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
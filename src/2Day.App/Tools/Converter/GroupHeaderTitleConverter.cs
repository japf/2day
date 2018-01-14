using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class GroupHeaderTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string param = parameter as string;
            string output = value as string;
            if (string.IsNullOrEmpty(output))
                return string.Empty;

            if (output.Contains(StringResources.ConverterDate_Yesterday))
            {
                if (param == "Main")
                    return output.Replace(StringResources.ConverterDate_Yesterday + " ", string.Empty).ToUpperInvariant();

                return StringResources.ConverterDate_Yesterday;
            }
            if (output.Contains(StringResources.ConverterDate_Today))
            {
                if (param == "Main")
                    return output.Replace(StringResources.ConverterDate_Today + " ", string.Empty).ToUpperInvariant();
                
                return StringResources.ConverterDate_Today;
            }
            if (output.Contains(StringResources.ConverterDate_Tomorrow))
            {
                if (param == "Main")
                    return output.Replace(StringResources.ConverterDate_Tomorrow + " ", string.Empty).ToUpperInvariant();
                
                return StringResources.ConverterDate_Tomorrow;
            }
            if (output.Contains(StringResources.ConverterDate_Yesterday))
            {
                if (param == "Main")
                    return output.Replace(StringResources.ConverterDate_Yesterday + " ", string.Empty).ToUpperInvariant();

                return StringResources.ConverterDate_Yesterday;
            }

            if (param == "Main")
                return output.ToUpperInvariant();
            
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}



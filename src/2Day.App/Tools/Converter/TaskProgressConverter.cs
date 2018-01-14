using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class TaskProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return StringResources.ConverterProgress_None;
            }
            else
            {
                double? progress = (double?)value;
                return string.Format("{0}%", progress.Value * 100);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

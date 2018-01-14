using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Icons;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class FolderToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int)
                return FontIconHelper.GetAppIcon((int) value);

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}



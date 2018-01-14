using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var list = value as IList;
            if (list != null && list.Count > 0)
            {
                List<EnumLocalizedValue> allLocalizedValues = EnumHelper.GetLocalizedValues(list[0].GetType());
                return allLocalizedValues.Where(v => list.Contains(v.Value)).ToList();
            }
            else
            {
                return EnumHelper.GetLocalizedValue(value.GetType(), value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            EnumLocalizedValue localizedValue = value as EnumLocalizedValue;
            if (localizedValue != null)
                return localizedValue.Value;

            return value;
        }
    }
}

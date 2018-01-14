using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Icons;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class PriorityToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            TaskPriority taskPriority = TaskPriority.Star;

            if (value is TaskPriority)
                taskPriority = (TaskPriority)value;

            switch (taskPriority)
            {
                case TaskPriority.None:
                    return AppIconType.PriorityNone;
                case TaskPriority.Low:
                    return AppIconType.PriorityLow;
                case TaskPriority.Medium:
                    return AppIconType.PriorityMedium;
                case TaskPriority.High:
                    return AppIconType.PriorityHigh;
                case TaskPriority.Star:
                    return AppIconType.PriorityStar;
                default:
                    return AppIconType.PriorityNone;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}

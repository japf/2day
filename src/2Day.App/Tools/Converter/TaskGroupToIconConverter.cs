using System;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Icons;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class TaskGroupToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TaskGroup taskGroup = TaskGroup.DueDate;
            
            if (value is TaskGroup)
            {
                taskGroup = (TaskGroup)value;
            }
            else if (parameter is string)
            {
                Enum.TryParse((string) parameter, out taskGroup);
            }
            
            switch (taskGroup)
            {
                case TaskGroup.DueDate:
                    return AppIconType.GroupDueDate;
                case TaskGroup.Priority:
                    return AppIconType.GroupPriority;
                case TaskGroup.Status:
                    return AppIconType.GroupStatus;
                case TaskGroup.Folder:
                    return AppIconType.GroupFolder;
                case TaskGroup.Action:
                    return AppIconType.GroupAction;
                case TaskGroup.Progress:
                    return AppIconType.GroupProgress;
                case TaskGroup.Context:
                    return AppIconType.GroupContext;
                case TaskGroup.StartDate:
                    return AppIconType.GroupStartDate;
                case TaskGroup.Modified:
                    return AppIconType.GroupModified;
                case TaskGroup.Completed:
                    return AppIconType.GroupCompleted;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

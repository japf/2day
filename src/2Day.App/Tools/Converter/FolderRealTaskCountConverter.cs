using System;
using System.Linq;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.App.Tools.Converter
{
    /// <summary>
    /// A converter that returns the "actual" visible tasks count in a folder. By "actual", it means that depending
    /// on the settings completed tasks and tasks with start date in the future are not taken into account for total
    /// count
    /// </summary>
    public class FolderRealTaskCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IAbstractFolder)
            {
                var folder = (IAbstractFolder) value;
                var settings = Ioc.Resolve<IWorkbook>().Settings;

                var tasks = folder.Tasks;
                DateTime now = DateTime.Now;

                if (settings.GetValue<CompletedTaskMode>(CoreSettings.CompletedTasksMode) == CompletedTaskMode.Hide)
                    tasks = tasks.Where(t => !t.IsCompleted);

                if (!settings.GetValue<bool>(CoreSettings.ShowFutureStartDates))
                    tasks = tasks.Where(t => !t.Start.HasValue || t.Start <= now);

                return tasks.Count();
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

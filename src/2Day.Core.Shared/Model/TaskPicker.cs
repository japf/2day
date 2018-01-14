using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model.Groups;
using Chartreuse.Today.Core.Shared.Tools.Collection;

namespace Chartreuse.Today.Core.Shared.Model
{
    public static class TaskPicker
    {
        public static List<ITask> SelectTasks(IAbstractFolder folder, ISettings settings)
        {
            var groupBuilder = GroupBuilderFactory.GetGroupBuilder(folder, settings);
            bool showCompleted = (folder is ISystemView) && ((ISystemView) folder).ViewKind == ViewKind.Completed;
            bool showTaskWithStartDateInFuture = settings.GetValue<bool>(CoreSettings.ShowFutureStartDates);

            DateTime dateTimeNow = DateTime.Now;

            var smartCollection = new SmartCollection<ITask>(
                folder.Tasks,
                groupBuilder,
                t =>
                {
                    return (!(folder is ISystemView) || t.Folder.ShowInViews.HasValue && t.Folder.ShowInViews.Value)
                                && (showCompleted || !t.IsCompleted)
                                && (showTaskWithStartDateInFuture || (!t.Start.HasValue || t.Start.Value <= dateTimeNow))
                                && t.ParentId == null; // exlude subtasks
                });

            return smartCollection.Items.SelectMany(g => g).ToList();
        }
    }
}

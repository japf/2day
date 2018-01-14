using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.Core.Shared.Model
{
    public static class ModelHelper
    {
        private static readonly IList<ITask> EmptyTasks = new List<ITask>();

        public static int SoftEditPending { get; private set; }

        public static IList<string> ReadTags(this ITask task)
        {
            IList<string> tags = new List<string>();

            if (!string.IsNullOrEmpty(task.Tags))
            {
                foreach (string tag in task.Tags.Split(Constants.TagSeparator))
                    tags.Add(tag);
            }

            return tags;
        }

        /// <summary>
        /// Returns a dictionary where each key is a tag and the associated key is the number of time
        /// the tag is used (in any tasks)
        /// </summary>
        /// <param name="workbook">Workbook</param>
        /// <returns>Dictionary</returns>
        public static IDictionary<string, int> GetTagsUsage(this IWorkbook workbook)
        {
            Dictionary<string, int> tagUsage = new Dictionary<string, int>();
            foreach (var task in workbook.Tasks)
            {
                var tags = task.ReadTags();
                foreach (string tag in tags)
                {
                    if (!tagUsage.ContainsKey(tag))
                        tagUsage.Add(tag, 1);
                    else
                        tagUsage[tag] += 1;
                }
            }

            return tagUsage;
        } 

        public static string GetTags(IList<string> tags)
        {
            if (tags.Count > 0)
                return tags.Aggregate((t1, t2) => t1.Trim() + Constants.TagSeparator + t2.Trim());
            else
                return string.Empty;
        }

        public static void WriteTags(this ITask task, IList<string> tags)
        {
            task.Tags = GetTags(tags);
        }

        public static TaskCreationParameters GetTaskCreationParameters(this IAbstractFolder folder)
        {
            var parameters = new TaskCreationParameters();

            if (folder is ISystemView)
            {
                var view = (ISystemView) folder;
                switch (view.ViewKind)
                {
                    case ViewKind.Today:
                        parameters.Due = DateTime.Today;
                        break;
                    case ViewKind.Tomorrow:
                        parameters.Due = DateTime.Today.AddDays(1.0);
                        break;
                    case ViewKind.Starred:
                        parameters.Priority = TaskPriority.Star;
                        break;
                    case ViewKind.NoDate:
                        parameters.Due = null;
                        break;
                }
            }
            else if (folder is IFolder)
            {
                parameters.Folder = (IFolder) folder;
            }
            else if (folder is IContext)
            {
                parameters.Context = (IContext) folder;
            }
            else if (folder is ITag)
            {
                parameters.Tag = folder.Name;
            }

            return parameters;
        }
        
        public static IContext GetDefaultContext(IWorkbook workbook)
        {
            int defaultContextId = workbook.Settings.GetValue<int>(CoreSettings.DefaultContext);
            return workbook.Contexts.FirstOrDefault(c => c.Id == defaultContextId);
        }

        public static DateTime? GetDefaultDueDate(ISettings settings)
        {
            return GetDefaultDate(settings, CoreSettings.DefaultDueDate);
        }

        public static DateTime? GetDefaultStartDate(ISettings settings)
        {
            return GetDefaultDate(settings, CoreSettings.DefaultStartDate);
        }

        private static DateTime? GetDefaultDate(ISettings settings, string key)
        {
            switch (settings.GetValue<DefaultDate>(key))
            {
                case DefaultDate.None:
                    return null;
                case DefaultDate.Today:
                    return DateTime.Now.Date;
                case DefaultDate.Tomorrow:
                    return DateTime.Now.Date.AddDays(1.0);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static void SetDueAndAdjustReminder(this ITask task, DateTime? newDate, IEnumerable<ITask> impactedTasks, bool showNotification = false)
        {
            INotificationService notificationService = Ioc.Resolve<INotificationService>();
            ISynchronizationManager syncManager = Ioc.Resolve<ISynchronizationManager>();

            var oldDate = task.Due;

            // change due date (and if we sync with Exchange, start date might be updated too)
            task.Due = newDate;
            syncManager.SetDueDate(newDate, task.Start, v => task.Start = v);
            task.Modified = DateTime.Now;

            // check if we must update reminder date
            if (!task.Alarm.HasValue || !newDate.HasValue || !oldDate.HasValue)
            {
                if (showNotification)
                {
                    if (newDate.HasValue)
                    {
                        var dateLong = string.Format("{0} {1}", newDate.Value.ToString("ddddd"), newDate.Value.ToString("M"));
                        notificationService.ShowNotification(string.Format(StringResources.Notification_DueDateUpdatedFormat, dateLong));
                    }
                    else
                    {
                        notificationService.ShowNotification(StringResources.Notification_DueDateRemoved);
                    }
                }

                return;
            }

            int days = (int) (newDate.Value - oldDate.Value).TotalDays;
            task.Alarm = task.Alarm.Value.AddDays(days);

            if (showNotification)
            {
                string newReminderDatetime = string.Format("{0} {1}", task.Alarm.Value.ToString("M"), task.Alarm.Value.ToString("t"));
                string toastMessage = string.Format(StringResources.Dialog_ReminderUpdatedDateFormat, newReminderDatetime);
                if (impactedTasks.Count(t => t.Alarm.HasValue) > 1)
                    toastMessage = StringResources.Dialog_RemindersUpdated;

                notificationService.ShowNotification(toastMessage);
            }
        }
        
        public static void TryRename(this IContext context, IWorkbook workbook, string newName)
        {
            if (!string.IsNullOrEmpty(newName) && workbook.Contexts.All(c => !c.Name.Equals(newName, StringComparison.Ordinal)))
                context.Name = newName;
        }

        public static void TryRename(this ITag tag, IWorkbook workbook, string newName)
        {
            if (!string.IsNullOrEmpty(newName) && workbook.Contexts.All(c => !c.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                string oldName = tag.Name;
                foreach (ITask task in workbook.Tasks)
                {
                    IList<string> tags = task.ReadTags();
                    for (int i = 0; i < tags.Count; i++)
                    {
                        if (tags[i].Equals(oldName, StringComparison.OrdinalIgnoreCase))
                            tags[i] = newName;
                    }

                    task.WriteTags(tags);
                }
            }
        }

        public static bool IsEqualsTo(this IFolder folder, string name, string color, TaskGroup group, bool ascending, bool showInViews)
        {
            return string.Equals(folder.Name, name) &&
                   string.Equals(folder.Color, color, StringComparison.OrdinalIgnoreCase) &&
                   folder.TaskGroup.Equals(group) &&
                   folder.GroupAscending.Equals(ascending) &&
                   folder.ShowInViews.Equals(showInViews);
        }

        public static bool IsEqualsTo(this ITask task, 
            string title, string note, string tags, 
            DateTime? due, DateTime? start, DateTime? alarm,
            ICustomFrequency customFrequency, bool useFixedDate,
            TaskPriority priority, double? progress, bool isCompleted,
            TaskAction action, string actionName, string actionValue, 
            IContext context, 
            IFolder folder,
            IList<ITask> subtasks)
        {
            bool match = string.Equals(task.Title, title) && 
                task.Note.IsEqualsOrEmptyTo(note) &&
                task.Tags.IsEqualsOrEmptyTo(tags) &&
                ((!task.Due.HasValue && !due.HasValue) || (task.Due.HasValue && due.HasValue && task.Due.Value.Date.Equals(due.Value.Date))) &&
                ((!task.Start.HasValue && !start.HasValue) || (task.Start.HasValue && start.HasValue && task.Start.Equals(start))) &&
                task.Completed.HasValue.Equals(isCompleted) &&
                (
                       (task.CustomFrequency == null && (customFrequency == null || customFrequency is OnceOnlyFrequency)) 
                    || ((task.CustomFrequency is OnceOnlyFrequency || task.CustomFrequency == null) && (customFrequency == null))
                    || (task.CustomFrequency != null && task.CustomFrequency.Equals(customFrequency) && task.UseFixedDate.Equals(useFixedDate))
                ) &&                    
                task.Priority == priority && 
                task.Action == action && 
                task.ActionName.IsEqualsOrEmptyTo(actionName) &&
                task.ActionValue.IsEqualsOrEmptyTo(actionValue) && 
                task.Alarm.Equals(alarm) && 
                task.Progress.Equals(progress) && 
                Equals(task.Context, context) && 
                Equals(task.Folder, folder);

            if (!match)
                return false;
            
            if (task.Children.Count != subtasks.Count)
                return false;

            for (int i = 0; i < task.Children.Count; i++)
            {
                var subtask = task.Children[i];
                var other = subtasks[i];
                bool subtaskMatch = IsEqualsTo(subtask, other.Title, other.Note, other.Tags, other.Due, other.Start,
                    other.Alarm, other.CustomFrequency, other.UseFixedDate, other.Priority, other.Progress, other.IsCompleted,
                    other.Action, other.ActionName, other.ActionValue, other.Context, subtask.Folder, EmptyTasks);
                // use subtask.Folder while it should be other.Folder because we want to skip the folder comparison
                // reason is that when we edit a task, we edit a copy of its subtask and the folder is not set

                if (!subtaskMatch)
                    return false;
            }

            return true;
        }

        public static void CopyToNewTask(this ITask source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // order matters here !
            // FrequencyType must be set after FrequencyValue because it computes value 
            // of the CustomFrequency object property
            var destination = new Impl.Task
            {
                Added = DateTime.Now,
                Modified = DateTime.Now,
                Title = source.Title,
                Folder = source.Folder,
                Note = source.Note,
                Tags = source.Tags,
                Due = source.Due,
                Start = source.Start,
                Completed = source.Completed,
                Priority = source.Priority,
                Action = source.Action,
                ActionName = source.ActionName,
                ActionValue = source.ActionValue,
                Alarm = source.Alarm,
                Progress = source.Progress,
                Context = source.Context,
                FrequencyValue = source.FrequencyValue,
                FrequencyType = source.FrequencyType,
                UseFixedDate = source.UseFixedDate
            };            
        }

        public static void CloneFolder(IFolder destination, IFolder source)
        {
            destination.Name = source.Name;
            destination.IconId = source.IconId;
            destination.Color = source.Color;
            destination.Order = source.Order;
            destination.ShowInViews = source.ShowInViews;
        }

        public static void CloneTask(ITask destination, ITask source, IWorkbook workbook)
        {
            destination.SyncId = source.SyncId;

            destination.Title = source.Title;

            destination.Note = source.Note;
            destination.Priority = source.Priority;
            destination.Tags = source.Tags;
            destination.Progress = source.Progress;

            destination.Added = source.Added;
            destination.Modified = source.Modified;
            destination.Due = source.Due;
            destination.Start = source.Start;
            destination.Completed = source.Completed;
            destination.Alarm = source.Alarm;

            destination.FrequencyType = source.FrequencyType.HasValue ? source.FrequencyType.Value : FrequencyType.Once;
            destination.FrequencyValue = source.FrequencyValue;
            destination.UseFixedDate = source.UseFixedDate;

            destination.Action = source.Action;
            destination.ActionName = source.ActionName;
            destination.ActionValue = source.ActionValue;

            var folder = workbook.Folders.FirstOrDefault(f => f.Name.Equals(source.Folder.Name, StringComparison.OrdinalIgnoreCase));
            if (folder == null) // in case this particular folder were deleted, use the first one available
                folder = workbook.Folders.FirstOrDefault();

            destination.Folder = folder;

            if (source.Context != null && !string.IsNullOrEmpty(source.Context.Name))
            {
                var context = workbook.Contexts.FirstOrDefault(c => c.Name.Equals(source.Context.Name, StringComparison.OrdinalIgnoreCase));
                if (context != null)
                    destination.Context = context;
            }
        }

        public static async Task SoftComplete(IEnumerable<ITask> tasks)
        {
            var notificationService = Ioc.Resolve<INotificationService>();
            var workbook = Ioc.Resolve<IWorkbook>();

            bool isPeriodic = tasks.Any(t => t.IsPeriodic);
            bool completeAll = tasks.All(t => !t.IsCompleted);

            if (isPeriodic)
            {
                // if there is at least one period task in the selection, undo is not supported
                foreach (var task in tasks)
                {
                    task.IsCompleted = !task.IsCompleted;

                    if (workbook.Settings.GetValue<bool>(CoreSettings.CompleteTaskSetProgress))
                    {
                        if (task.IsCompleted)
                            task.Progress = 1;
                        else
                            task.Progress = 0;
                    }
                }
                return;
            }
            
            var editions = new List<TaskEdition>();
            foreach (var task in tasks)
            {
                editions.Add(new TaskEdition(task));
                task.IsCompleted = !task.IsCompleted;

                if (workbook.Settings.GetValue<bool>(CoreSettings.CompleteTaskSetProgress))
                {
                    if (task.IsCompleted)
                        task.Progress = 1;
                    else
                        task.Progress = 0;
                }
            }

            SoftEditPending++;

            string message;
            if (completeAll)
            {
                if (tasks.Count() == 1)
                    message = string.Format(StringResources.Notification_TaskCompletedFormat, tasks.ElementAt(0).Title);
                else
                    message = StringResources.TaskGroup_Completed;
            }
            else
            {
                if (tasks.Count() == 1)
                    message = StringResources.Notification_TaskUpdated;
                else
                    message = StringResources.Notification_TasksUpdated;
            }

            await notificationService.ShowNotification(message, ToastType.Info, () =>
            {
                foreach (var edition in editions)
                {
                    edition.Task.IsCompleted = !edition.Task.IsCompleted;
                    edition.Task.Completed = edition.Completed;
                    edition.Task.Progress = edition.Progress;
                }

                SoftEditPending--;
            });

            SoftEditPending--;
        }

        public static async Task SoftChangeDueDate(IEnumerable<ITask> tasks, DateTime? dueDate)
        {
            var notificationService = Ioc.Resolve<INotificationService>();

            var editions = new List<TaskEdition>();
            foreach (var task in tasks)
            {
                editions.Add(new TaskEdition(task));

                task.Due = dueDate;
                task.Modified = DateTime.Now;
            }

            SoftEditPending++;

            string message;
            if (dueDate.HasValue)
                message = string.Format(StringResources.Notification_DueDateUpdatedFormat, dueDate.Value);
            else
                message = StringResources.Notification_DueDateRemoved;

            await notificationService.ShowNotification(message, ToastType.Info, () =>
            {
                foreach (var edition in editions)
                {
                    edition.Task.Due = edition.Due;
                    edition.Task.Modified = edition.Modified;
                }

                SoftEditPending--;
            });

            SoftEditPending--;
        }

        public static async Task SoftDelete(IEnumerable<ITask> tasks)
        {
            var notificationService = Ioc.Resolve<INotificationService>();

            var editions = new List<TaskEdition>();
            foreach (var task in tasks)
            {
                editions.Add(new TaskEdition(task));
                task.Context = null;
                task.Folder = null;
            }

            SoftEditPending++;

            await notificationService.ShowNotification(StringResources.Notification_Deleted, ToastType.Warning, () =>
            {
                foreach (var edition in editions)
                {
                    edition.Task.Folder = edition.Folder;
                    edition.Task.Context = edition.Context;
                }

                SoftEditPending--;
            });

            SoftEditPending--;
        }

        private struct TaskEdition
        {
            public ITask Task { get; }
            public IFolder Folder { get; }
            public IContext Context { get; }
            public DateTime? Due { get; }
            public DateTime? Completed { get; }
            public DateTime Modified { get; }
            public double? Progress { get; set; }

            public TaskEdition(ITask task)
            {
                this.Task = task;
                this.Folder = task.Folder;
                this.Context = task.Context;
                this.Due = task.Due;
                this.Completed = task.Completed;
                this.Modified = task.Modified;
                this.Progress = task.Progress;
            }
        }
    }
}

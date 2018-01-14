using System;
using Chartreuse.Today.Core.Shared.Model.Impl;

namespace Chartreuse.Today.Core.Shared.Model
{
    public static class RecurringTaskHelper
    {
        public static ITask CreateNewTask(ITask source)
        {
            if (source.CustomFrequency == null)
                return null;

            var now = DateTime.Now;

            var task = new Task
            {
                Action = source.Action,
                ActionName = source.ActionName,
                ActionValue = source.ActionValue,
                CustomFrequency = source.CustomFrequency,
                Note = source.Note,
                Title = source.Title,
                Priority = source.Priority,
                Added = now,
                Modified = now,
                UseFixedDate = source.UseFixedDate,
                HasRecurringOrigin = true,
                Progress = null,
                Tags = source.Tags
            };

            if (source.Due.HasValue)
            {
                // compute next due date
                DateTime refDate;
                if (source.UseFixedDate)
                    refDate = source.Due.Value;
                else
                    refDate = now.Date.AddHours(source.Due.Value.Hour).AddMinutes(source.Due.Value.Minute);

                task.Due = task.CustomFrequency.ComputeNextDate(refDate);
            }

            if (source.Start.HasValue)
            {
                // compute next start date
                DateTime refDate;
                if (source.UseFixedDate)
                    refDate = source.Start.Value;
                else
                    refDate = now.Date.AddHours(source.Start.Value.Hour).AddMinutes(source.Start.Value.Minute);

                task.Start = task.CustomFrequency.ComputeNextDate(refDate);
            }

            // compute alarm date
            if (source.Alarm.HasValue)
            {
                DateTime? oldAlarm = source.Alarm;
                // rerieve the time of the alarm
                int reminderHour = oldAlarm.Value.Hour;
                int reminderMinutes = oldAlarm.Value.Minute;

                // remove old alarm before setting the new one (prevents from reaching 50 reminders limit on WP)
                source.Alarm = null;
                task.Alarm = oldAlarm;

                DateTime? sourceDate = null;
                DateTime? newDate = null;

                if (task.Due.HasValue && source.Due.HasValue)
                {
                    sourceDate = source.Due.Value;
                    newDate = task.Due.Value;
                }
                else if (task.Start.HasValue && source.Start.HasValue)
                {
                    sourceDate = source.Start.Value;
                    newDate = task.Start.Value;
                }

                if (sourceDate.HasValue && newDate.HasValue)
                {
                    // compute the time span between the alarm and the due/start date
                    TimeSpan reminderSpanDate = sourceDate.Value.Date.Subtract(oldAlarm.Value.Date);
                    DateTime reminderDate = newDate.Value.Subtract(reminderSpanDate);

                    task.Alarm = new DateTime(reminderDate.Year, reminderDate.Month, reminderDate.Day, reminderHour, reminderMinutes, 0);
                }
            }
            
            // by setting the folder of the task, it will register the task in the workbook
            task.Folder = source.Folder;

            // set Context <after> the task has a well define folder, otherwise we'll get in big troubles !
            task.Context = source.Context;

            // check subtasks
            foreach (var subtask in source.Children)
            {
                var copy = new Task
                {
                    Action = subtask.Action,
                    ActionName = subtask.ActionName,
                    ActionValue = subtask.ActionValue,
                    CustomFrequency = subtask.CustomFrequency,
                    Note = subtask.Note,
                    Title = subtask.Title,
                    Priority = subtask.Priority,
                    Added = subtask.Added,
                    Modified = subtask.Modified,
                    UseFixedDate = subtask.UseFixedDate,
                    HasRecurringOrigin = subtask.HasRecurringOrigin,
                    Progress = subtask.Progress,
                    Tags = subtask.Tags,
                    Context = subtask.Context
                };
                task.AddChild(copy);

                // important: set folder once the task is a child of the parent task
                copy.Folder = subtask.Folder;
            }

            return task;
        }
    }
}

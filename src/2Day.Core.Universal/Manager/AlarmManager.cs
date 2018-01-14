using System;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Notifications;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.Core.Universal.Manager
{
    public class AlarmManager : IAlarmManager
    {
        public AlarmManager(IWorkbook workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            workbook.TaskAdded += this.OnTaskAdded;
            workbook.TaskRemoved += this.OnTaskRemoved;
            workbook.TaskChanged += this.OnTaskChanged;
        }

        private ToastNotifier CreateToastNotifier()
        {
            try
            {
                return ToastNotificationManager.CreateToastNotifier();
            }
            catch (Exception e)
            {
                TrackingManagerHelper.Exception(e, "Error while creating toast notifier in AlarmManager");
            }

            return null;
        }

        private void OnTaskAdded(object sender, EventArgs<ITask> e)
        {
            var task = e.Item;

            if (task.Alarm.HasValue)
                this.AddReminder(task);
        }

        private void OnTaskRemoved(object sender, EventArgs<ITask> e)
        {
            var task = e.Item;

            if (task.Alarm.HasValue)
                this.RemoveReminder(task);
        }

        private void OnTaskChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Alarm" && e.PropertyName != "Title" && e.PropertyName != "Note")
                return;

            try
            {
                // alarm, title or note has changed, rebuild the notification
                var task = (ITask)sender;

                this.RemoveReminder(task);
                this.AddReminder(task);
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, string.Format("Exception AlarmManager.OnTaskChanged: {0}", ex.ToString()));
            }            
        }

        private void AddReminder(ITask task)
        {
            if (!task.Alarm.HasValue || task.Alarm.Value <= DateTime.Now)
            {
                LogService.Log("AlarmManager", "Cannot add reminder because alarm date is already past");
                return;
            }
            
            var notification = NotificationContentBuilder.CreateTaskToastNotification(task);
            if (notification != null)
            {
                ScheduledToastNotification toast = new ScheduledToastNotification(notification, task.Alarm.Value)
                {
                    Id = task.Id.ToString()
                };

                var toastNotifier = this.CreateToastNotifier();
                if (toastNotifier != null)
                {
                    // check if a schedule notification does not already exist for this task
                    var scheduledToastNotifications = toastNotifier.GetScheduledToastNotifications();
                    var existingNotification = scheduledToastNotifications.FirstOrDefault(n => n.Id == toast.Id);
                    if (existingNotification != null)
                        toastNotifier.RemoveFromSchedule(existingNotification);

                    toastNotifier.AddToSchedule(toast);
                }
            }

            LogService.Log("AlarmManager", string.Format("Reminder of task {0} added for {1}", task.Title, task.Alarm.Value));
        }

        private void RemoveReminder(ITask task)
        {
            var toastNotifier = this.CreateToastNotifier();
            if (toastNotifier != null)
            {
                var scheduledToastNotifications = toastNotifier.GetScheduledToastNotifications();
                for (int j = 0; j < scheduledToastNotifications.Count; j++)
                {
                    if (scheduledToastNotifications[j].Id == task.Id.ToString())
                    {
                        toastNotifier.RemoveFromSchedule(scheduledToastNotifications[j]);
                    }
                }
            }
        }
    }
}

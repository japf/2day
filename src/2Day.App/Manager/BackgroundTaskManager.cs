using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Chartreuse.Today.App.BackgroundService;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.IO;

namespace Chartreuse.Today.App.Manager
{
    public class BackgroundTaskManager : IBackgroundTaskManager
    {
        private static readonly string BackgroundTaskTimerEntryPoint = typeof(TimerBackgroundService).FullName;
        private const string BackgroundTaskTimerName = "2DayTimerBackgroundTask";

        private static readonly string BackgroundTaskNotificationEntryPoint = typeof(NotificationBackgroundService).FullName;
        private const string BackgroundTaskNotificationName = "2DayNotificationBackgroundTask";

        private const int BackgroundTaskRefreshTimeMin = 15;

        private readonly IWorkbook workbook;

        public BackgroundTaskManager(IWorkbook workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            this.workbook = workbook;
        }

        public async Task InitializeAsync()
        {
            bool backgroundTaskAccess = await this.RegisterBackgroundTaskAsync();

            this.workbook.Settings.SetValue(CoreSettings.BackgroundAccess, backgroundTaskAccess);

            if (!backgroundTaskAccess)
                return;

            KeyValuePair<Guid, IBackgroundTaskRegistration> taskTimer = BackgroundTaskRegistration.AllTasks.FirstOrDefault(t => t.Value.Name == BackgroundTaskTimerName);
            if (taskTimer.Key == Guid.Empty || taskTimer.Value == null)
            {
                try
                {
                    var builder = new BackgroundTaskBuilder { Name = BackgroundTaskTimerName, TaskEntryPoint = BackgroundTaskTimerEntryPoint };
                    builder.SetTrigger(new TimeTrigger(BackgroundTaskRefreshTimeMin, false));
                    builder.Register();
                }
                catch (Exception ex)
                {
                    TrackingManagerHelper.Exception(ex, "Error while registering timer background task");
                }
            }

            KeyValuePair<Guid, IBackgroundTaskRegistration> taskNotification = BackgroundTaskRegistration.AllTasks.FirstOrDefault(t => t.Value.Name == BackgroundTaskNotificationName);
            if (taskNotification.Key == Guid.Empty || taskNotification.Value == null)
            {
                try
                {
                    var builder = new BackgroundTaskBuilder { Name = BackgroundTaskNotificationName, TaskEntryPoint = BackgroundTaskNotificationEntryPoint };
                    builder.SetTrigger(new ToastNotificationActionTrigger());
                    builder.Register();
                }
                catch (Exception ex)
                {
                    TrackingManagerHelper.Exception(ex, "Error while registering notification background task");
                }
            }

            // wait handle that is triggered when the workbook changes because of 
            // Cortana's background service (eg. voice command to create a task)
            // OR notification toast (eg. complete a task with the toast shown
            WaitHandleHelper.RegisterForSignal(Constants.SyncPrimitiveBackgroundEvent, this.OnBackgroundEvent);
        }

        private void OnBackgroundEvent()
        {
            try
            {
                var lastPersistence = new WinPersistenceLayer();
                IWorkbook newWorkbook = lastPersistence.Open();
                using (this.workbook.WithDuplicateProtection())
                {
                    foreach (ITask newTask in newWorkbook.Tasks)
                    {
                        if (this.workbook.Tasks.All(t => t.Id != newTask.Id))
                        {
                            // this is a new task we don't have in the "current" workbook
                            var oldTask = this.workbook.CreateTask(newTask.Id);
                            ModelHelper.CloneTask(oldTask, newTask, this.workbook);
                        }

                        var existingTask = this.workbook.Tasks.FirstOrDefault(t => t.Id == newTask.Id);
                        if (existingTask != null && existingTask.Modified < newTask.Modified)
                        {
                            // this is a task that has been updated
                            ModelHelper.CloneTask(existingTask, newTask, this.workbook);
                        }
                    }
                    lastPersistence.CloseDatabase();
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "BackgroundTaskManager.OnBackgroundEvent");
            }
        }

        private async Task<bool> RegisterBackgroundTaskAsync()
        {
            try
            {
                await BackgroundExecutionManager.RequestAccessAsync();
                return true;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "BackgroundTaskManager.RegisterBackgroundTaskAsync");
                return false;
            }
        }        
    }
}

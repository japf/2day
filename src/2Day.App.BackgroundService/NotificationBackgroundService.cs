using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.LaunchArguments;
using Chartreuse.Today.Core.Universal.IO;
using Chartreuse.Today.Core.Universal.Manager;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.BackgroundService
{
    public sealed class NotificationBackgroundService : IBackgroundTask
    {
        private static readonly EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, Constants.SyncPrimitiveBackgroundEvent);

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            if (details != null && !string.IsNullOrWhiteSpace(details.Argument))
            {
                var persistenceLayer = new WinPersistenceLayer(automaticSave: false);
                var workbook = persistenceLayer.Open(tryUpgrade: true);
                workbook.Initialize();

                var platformService = new PlatformService(
                    ApplicationVersion.GetAppVersion(),
                    workbook.Settings.GetValue<string>(CoreSettings.DeviceId),
                    () => string.Empty);

                // important: load alarm manager so that we update reminders properly is a recurring task is created
                var alarmManager = new AlarmManager(workbook);

                LaunchArgumentDescriptor descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(workbook, details.Argument);
                if (descriptor.Task != null && descriptor.Type == LaunchArgumentType.CompleteTask)
                {
                    descriptor.Task.IsCompleted = true;
                    persistenceLayer.Save();

                    UpdateSyncMetadata(workbook, platformService, descriptor);

                    var tileManager = new TileManager(workbook, new TrackingManager(false, DeviceFamily.Unkown), null, true);
                    tileManager.LoadSecondaryTilesAsync().Wait(500);
                    tileManager.UpdateTiles();

                    // signal changes (usefull if app is currently running)
                    waitHandle.Set();
                }
            }

            taskInstance.GetDeferral().Complete();
        }

        private static void UpdateSyncMetadata(IWorkbook workbook, PlatformService platformService, LaunchArgumentDescriptor descriptor)
        {
            // update sync metadata if appropriate
            var loadFileAsync = platformService.LoadFileAsync<SynchronizationMetadata>(SynchronizationMetadata.Filename);
            loadFileAsync.Wait(500);
            if (loadFileAsync.IsCompleted && loadFileAsync.Result != null)
            {
                SynchronizationMetadata syncMetadata = loadFileAsync.Result;

                // update the descriptor of the edited task
                if (!syncMetadata.EditedTasks.ContainsKey(descriptor.Task.Id))
                    syncMetadata.EditedTasks.Add(descriptor.Task.Id, TaskProperties.Completed);
                else
                    syncMetadata.EditedTasks[descriptor.Task.Id] |= TaskProperties.Completed;

                // check if the completion of this tasks created a new task that should be flagged for future sync as "new task"
                // (that happens if the task is recurring)
                var newTask = workbook.Tasks.FirstOrDefault(t => t.Folder == descriptor.Task.Folder && t.Title == descriptor.Task.Title && t.SyncId == null);
                if (newTask != null && !syncMetadata.AddedTasks.Contains(newTask.Id))
                    syncMetadata.AddedTasks.Add(newTask.Id);

                platformService.SaveFileAsync(syncMetadata, SynchronizationMetadata.Filename).Wait(500);
            }
        }
    }
}
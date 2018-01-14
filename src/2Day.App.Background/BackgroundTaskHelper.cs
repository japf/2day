using System;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.System.Profile;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.IO;
using Chartreuse.Today.Core.Universal.Manager;
using Chartreuse.Today.Core.Universal.Model;
using Chartreuse.Today.Core.Universal.Notifications;
using Chartreuse.Today.Core.Universal.Tools.Logging;

namespace Chartreuse.Today.App.Background
{
    public class BackgroundTaskHelper
    {
        private IPersistenceLayer persistenceLayer;
        private SynchronizationManager synchronizationManager;
        private BackgroundTaskDeferral deferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                this.deferral = taskInstance.GetDeferral();

                this.SafeRun(taskInstance);
            }
            catch (Exception ex)
            {
                ReportStatus(BackgroundExecutionStatus.CompletedRootException);
                this.deferral.Complete();

                DeviceFamily deviceFamily = DeviceFamily.Unkown;
                if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop", StringComparison.OrdinalIgnoreCase))
                    deviceFamily = DeviceFamily.WindowsDesktop;
                else if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile", StringComparison.OrdinalIgnoreCase))
                    deviceFamily = DeviceFamily.WindowsMobile;

                var trackingManager = new TrackingManager(false, deviceFamily);
                trackingManager.Exception(ex, "Exception Background task helper root");
            }
        }
        public async void SafeRun(IBackgroundTaskInstance taskInstance)
        {
            bool hadException = false;

            // Register to receive an event if Cortana dismisses the background task. This will
            // occur if the task takes too long to respond, or if Cortana's UI is dismissed.
            // Any pending operations should be cancelled or waited on to clean up where possible.
            taskInstance.Canceled += this.OnTaskCanceled;

            WinSettings.Instance.SetValue(CoreSettings.BackgroundLastStartExecution, DateTime.Now);
            ReportStatus(BackgroundExecutionStatus.Started);

            LogService.Initialize(new WinLogHandler());
            LogService.Level = WinSettings.Instance.GetValue<LogLevel>(CoreSettings.LogLevel);

            ResourcesLocator.Initialize("ms-appx://", UriKind.Absolute);

            var mutex = new Mutex(false, Constants.SyncPrimitiveAppRunningForeground);
            if (!mutex.WaitOne(1))
            {
                LogService.Log("Agent", "Skipping background agent because app is foreground");
                await LogService.SaveAsync();
                this.deferral.Complete();

                ReportStatus(BackgroundExecutionStatus.Skipped);

                return;
            }

            ITrackingManager trackingManager = null;
            try
            {
                this.persistenceLayer = new WinPersistenceLayer(automaticSave: false);

                var deviceFamily = DeviceFamily.Unkown;

                if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop", StringComparison.OrdinalIgnoreCase))
                    deviceFamily = DeviceFamily.WindowsDesktop;
                else if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile", StringComparison.OrdinalIgnoreCase))
                    deviceFamily = DeviceFamily.WindowsMobile;

                trackingManager = new TrackingManager(false, deviceFamily);

                var workbook = this.persistenceLayer.Open(tryUpgrade: true) as Workbook;

                if (workbook != null)
                {
                    ReportStatus(BackgroundExecutionStatus.WorkbookLoaded);

                    Ioc.RegisterInstance<IWorkbook, Workbook>(workbook);
                    workbook.Initialize();

                    // important: load alarm manager so that we update reminders properly is a recurring task is created
                    var alarmManager = new AlarmManager(workbook);

                    var backgroundSyncManager = new BackgroundSynchronizationManager(workbook, trackingManager, ToastHelper.ToastMessage);
                    await backgroundSyncManager.SetupAsync();

                    if (backgroundSyncManager.CanSync())
                    {
                        ReportStatus(BackgroundExecutionStatus.SyncStarted);

                        this.synchronizationManager = backgroundSyncManager.SynchronizationManager;

                        if (workbook.Settings.GetValue<bool>(CoreSettings.BackgroundToast))
                        {
                            this.synchronizationManager.OperationCompleted += (s, e) => ToastHelper.ToastMessage(e.Item);
                            this.synchronizationManager.OperationFailed += (s, e) => ToastHelper.ToastMessage(e.Message);
                        }

                        bool result = await backgroundSyncManager.TrySyncAsync(this.persistenceLayer);

                        if(result)
                            ReportStatus(BackgroundExecutionStatus.SyncCompleted);
                        else
                            ReportStatus(BackgroundExecutionStatus.SyncError);
                    }

                    // update tiles
                    ReportStatus(BackgroundExecutionStatus.TileStarted);

                    TileManager timeManager = new TileManager(workbook, trackingManager, null, true);
                    await timeManager.LoadSecondaryTilesAsync();

                    timeManager.UpdateTiles();

                    ReportStatus(BackgroundExecutionStatus.TileCompleted);

                    // save log
                    await LogService.SaveAsync();
                }
                else
                {
                    ReportStatus(BackgroundExecutionStatus.WorkbookNotLoaded);
                }
            }
            catch (Exception ex)
            {
                hadException = true;

                LogService.Level |= LogLevel.Debug;
                LogService.Log("WinBackgroundTaskHelper", "Exception during background execution: " + ex.GetAllMessages());

                if (trackingManager != null)
                    trackingManager.Exception(ex, "Exception Background task helper");

                ReportStatus(BackgroundExecutionStatus.CompletedException);
            }

            if (!hadException)
                ReportStatus(BackgroundExecutionStatus.CompletedSuccess);

            await LogService.SaveAsync();
            WinSettings.Instance.SetValue(CoreSettings.BackgroundLastEndExecution, DateTime.Now);

            this.deferral.Complete();
        }

        private async void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.persistenceLayer != null)
                this.persistenceLayer.Save();

            if (this.synchronizationManager != null)
                await this.synchronizationManager.SaveAsync();

            await LogService.SaveAsync();
            ReportStatus(BackgroundExecutionStatus.Cancelled);

            if (this.deferral != null)
                this.deferral.Complete();
        }

        private static void ReportStatus(BackgroundExecutionStatus status)
        {
            WinSettings.Instance.SetValue(CoreSettings.BackgroundLastStatus, status);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Notifications;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Tools
{
    /// <summary>
    /// This class is responsible for triggering timer and performs jobs in the apps
    /// Current jobs are
    ///  - update task visibility because of start date (when start date is reached task is shown)
    ///  - update task group (when we group by due date, a group name 'tomorrow Monday...' can change to 'today Monday' during the night)
    ///  - automatic sync (periodic and after changes)
    /// </summary>
    public class AppJobScheduler
    {
        private const int UpdateTaskIntervalSeconds = 90;
        private const int SyncIntervalSeconds = 120;
        private const int TimerIntervalMs = 5000;

        private readonly IWorkbook workbook;
        private readonly ISynchronizationManager synchronizationManager;
        private readonly Func<IEnumerable<FolderItemViewModel>> getFolderItems;

        private readonly ThreadPoolTimer updateTasksTimer;
        private readonly ThreadPoolTimer syncTimer;
        private readonly DispatcherTimer afterChangeTimer;

        private DateTime lastCheck;

        internal AppJobScheduler(IWorkbook workbook, ISynchronizationManager synchronizationManager, Func<IEnumerable<FolderItemViewModel>> getFolderItems)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (synchronizationManager == null)
                throw new ArgumentNullException(nameof(synchronizationManager));
            if (getFolderItems == null)
                throw new ArgumentNullException(nameof(getFolderItems));

            this.workbook = workbook;
            this.workbook.FolderAdded += (s, e) => this.HandleChange();
            this.workbook.FolderChanged += (s, e) => this.HandleChange();
            this.workbook.FolderRemoved += (s, e) => this.HandleChange();

            this.workbook.ContextAdded += (s, e) => this.HandleChange();
            this.workbook.ContextChanged += (s, e) => this.HandleChange();
            this.workbook.ContextRemoved += (s, e) => this.HandleChange();

            foreach (var view in workbook.Views)
                view.PropertyChanged += (s, e) => this.HandleChange();

            this.synchronizationManager = synchronizationManager;
            this.getFolderItems = getFolderItems;

            var dispatcher = Window.Current.Dispatcher;

            // use timer created through ThreadPoolTimer so that timer runs even when the app is minimized - this is not the case of a DispatcherTimer
            // because of this, we need to switch back to the dispatcher context when the timer tick so that it's safe to call UI related stuff (from binding updates
            // for example)
            this.updateTasksTimer = ThreadPoolTimer.CreatePeriodicTimer(
                (d) => dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.OnUpdateTasksTimerTick()),
                TimeSpan.FromSeconds(UpdateTaskIntervalSeconds));
            
            this.syncTimer = ThreadPoolTimer.CreatePeriodicTimer(
                (d) => dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.OnSyncTimerTick()), 
                TimeSpan.FromSeconds(SyncIntervalSeconds));

            this.afterChangeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TimerIntervalMs) };
            this.afterChangeTimer.Tick += this.OnAfterChangeTimerTick;

            this.lastCheck = DateTime.MinValue;
        }

        private void OnAfterChangeTimerTick(object sender, object o)
        {
            this.afterChangeTimer.Stop();

            if (this.CanStartSync() && this.synchronizationManager.Metadata.HasChanges && ModelHelper.SoftEditPending < 1)
            {
                this.synchronizationManager.Sync();
            }
        }

        private void HandleChange()
        {
            if (!this.afterChangeTimer.IsEnabled)
            {
                this.afterChangeTimer.Start();
            }
            else
            {
                this.afterChangeTimer.Stop();
                this.afterChangeTimer.Start();
            }
        }

        private bool CanStartSync()
        {
            return this.synchronizationManager.IsSyncConfigured && !this.synchronizationManager.IsSyncRunning 
                && this.workbook.Settings.GetValue<bool>(CoreSettings.SyncAutomatic)
                && NetworkHelper.IsNetworkAvailable
                && Window.Current.Content is Frame && ((Frame)Window.Current.Content).Content is MainPage;
        }

        internal async Task OnSyncTimerTick()
        {
            try
            {
                // no automatic sync with ToodleDo because the API has rate limit (2 requests / min)
                if (this.CanStartSync() && ModelHelper.SoftEditPending < 1 && this.synchronizationManager.ActiveService != SynchronizationService.ToodleDo)
                {
                    if (this.workbook.Settings.GetValue<bool>(CoreSettings.BackgroundToast))
                    {
                        ToastHelper.ToastMessage("Automatic sync started");
                    }

                    await this.synchronizationManager.Sync();
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "AppJobScheduler.OnSyncTimerTick");
            }
        }

        internal void OnUpdateTasksTimerTick()
        {
            try
            {
                if (this.synchronizationManager.IsSyncRunning)
                    return;

                DateTime now = DateTime.Now;
                if (StaticTestOverrides.Now.HasValue)
                    now = StaticTestOverrides.Now.Value;

                // check if the date has changed since the last time
                if (this.lastCheck != DateTime.MinValue && this.lastCheck.Date != now.Date)
                {
                    foreach (var folderViewModel in this.getFolderItems())
                        folderViewModel.Rebuild();
                }

                // update tasks regarding start date field, find tasks with a start date that is just overdue
                var tasks = this.workbook.Tasks.Where(t => t.Start.HasValue && t.Start <= now);
                if (tasks.Any())
                {
                    foreach (var folderViewModel in this.getFolderItems())
                    {
                        foreach (ITask task in tasks)
                        {
                            if (folderViewModel.Folder.Tasks.Contains(task))
                                folderViewModel.SmartCollection.InvalidateItem(task);
                        }
                    }
                }

                this.lastCheck = now;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "AppJobScheduler.OnUpdateTasksTimerTick");
            }
        }
    }
}
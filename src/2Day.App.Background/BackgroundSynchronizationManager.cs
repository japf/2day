using System;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.IO;
using Chartreuse.Today.Core.Universal.Tools;
using Chartreuse.Today.Core.Universal.Tools.Security;
using Chartreuse.Today.Shared.Sync.Vercors;

namespace Chartreuse.Today.App.Background
{
    public class BackgroundSynchronizationManager
    {
        private readonly IWorkbook workbook;
        private readonly ITrackingManager trackingManager;
        private readonly Action<string> toastMessage;
        private readonly bool isBackgroundSyncEnabled;
        private readonly bool toastBackgroundSync;
        private SynchronizationManager synchronizationManager;

        public bool IsBackgroundSyncEnabled
        {
            get { return this.isBackgroundSyncEnabled; }
        }

        public SynchronizationManager SynchronizationManager
        {
            get
            {
                if (this.synchronizationManager == null)
                    throw new NotSupportedException("Synchronization manager is not available yet");

                return this.synchronizationManager;
            }
        }
        
        public BackgroundSynchronizationManager(IWorkbook workbook, ITrackingManager trackingManager, Action<string> toastMessage)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));

            this.workbook = workbook;
            this.trackingManager = trackingManager;
            this.toastMessage = toastMessage;

            this.toastBackgroundSync = workbook.Settings.GetValue<bool>(CoreSettings.BackgroundToast);
            this.isBackgroundSyncEnabled = workbook.Settings.GetValue<bool>(CoreSettings.BackgroundSync);

            if (this.isBackgroundSyncEnabled)
            {
                // check the last time the app was run
                var lastRun = workbook.Settings.GetValue<DateTime>(CoreSettings.LastRunDateTime);
                if ((DateTime.UtcNow - lastRun).TotalDays > 14)
                {
                    this.isBackgroundSyncEnabled = false;
                    this.TrackEvent("Sync", "Skipping background sync, app wasn't ran in the last 14 days");
                }
            }
        }

        protected void TrackEvent(string category, string message)
        {
            this.trackingManager.Event(TrackingSource.Background, category, message);
        }

        public async Task SetupAsync()
        {
            if (!this.isBackgroundSyncEnabled)
                return;

            var cryptoService = Ioc.RegisterInstance<ICryptoService, WinCryptoService>(new WinCryptoService());
            var vercorsService = new VercorsService();
            var platformService = new PlatformService(
                ApplicationVersion.GetAppVersion(), 
                this.workbook.Settings.GetValue<string>(CoreSettings.DeviceId),
                () => string.Empty);

            this.synchronizationManager = new SynchronizationManager(platformService, this.trackingManager, "win", true);

            Ioc.RegisterInstance<ISynchronizationManager, SynchronizationManager>(this.synchronizationManager);

            await SynchronizationManagerBootstraper.InitializeAsync(this.workbook, this.synchronizationManager, cryptoService, vercorsService, true);
        }

        public bool CanSync()
        {
            if (this.synchronizationManager != null && !this.synchronizationManager.IsSyncConfigured)
                LogService.Log("BackgroundSync", "Sync is not configured");

            return this.synchronizationManager != null && this.synchronizationManager.IsSyncConfigured;
        }

        public async Task<bool> TrySyncAsync(IPersistenceLayer persistenceLayer)
        {
            if (!this.CanSync())
                throw new NotSupportedException("Background sync is not available");

            try
            {
                LogService.Log("BackgroundSync", string.Format("Synchronization starting using {0}", NetworkHelper.GetNetworkStatus()));

                await this.synchronizationManager.Sync();
                
                if (this.HasSyncProducedChanges(this.synchronizationManager.Metadata))
                    this.workbook.Settings.SetValue(CoreSettings.SyncBackgroundOccured, true);

                persistenceLayer.Save();
                await this.WriteSyncMetadataAsync((SynchronizationMetadata) this.synchronizationManager.Metadata);

                LogService.Log("BackgroundSync", "Synchronization completed");
                return true;
            }
            catch (Exception ex)
            {
                var message = "Exception will doing background sync: " + ex.Message + " " + ex.StackTrace;

                if (this.toastBackgroundSync && this.toastMessage != null)
                    this.toastMessage(message);

                TrackingManagerHelper.Exception(ex, "TrySyncAsync background");
                return false;
            }
        }

        private bool HasSyncProducedChanges(ISynchronizationMetadata metadata)
        {
            return metadata.AfterSyncEditedTasks.Count > 0 || metadata.AfterSyncEditedFolders.Count > 0 || metadata.AfterSyncEditedContexts.Count > 0 || metadata.AfterSyncEditedSmartViews.Count > 0;
        }

        public async Task TryUpdateWorkbookAsync()
        {
            try
            {
                // check if the background sync produced changes
                var metadata = await this.ReadSyncMetadataAsync();

                if (metadata == null)
                    return;

                var syncManager = (SynchronizationManager)Ioc.Resolve<ISynchronizationManager>();

                if (this.HasSyncProducedChanges(metadata))
                {
                    using (this.workbook.WithDuplicateProtection())
                    {
                        await this.UpdateWorkbookAsync(metadata);
                    }
                }

                // update the metadata with the latest version
                syncManager.Metadata = metadata;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "TryUpdateWorkbookAsync");
            }
        }

        private async Task UpdateWorkbookAsync(SynchronizationMetadata metadata)
        {
            // load a new workbook from the persisted db to have the latest version
            var lastPersistence = new WinPersistenceLayer();
            IWorkbook lastWorkbook = lastPersistence.Open();

            foreach (string folderName in metadata.AfterSyncEditedFolders)
            {
                var newFolder = lastWorkbook.Folders.FirstOrDefault(f => f.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));
                var oldFolder = this.workbook.Folders.FirstOrDefault(f => newFolder != null ? f.Id == newFolder.Id : f.Name == folderName);

                if (newFolder == null)
                {
                    // folder was deleted
                    this.workbook.RemoveFolder(folderName);
                }
                else if (oldFolder != null)
                {
                    // folder was updated
                    ModelHelper.CloneFolder(oldFolder, newFolder);
                }
                else
                {
                    // folder was created
                    oldFolder = this.workbook.AddFolder(folderName, newFolder.SyncId, newFolder.Id);
                    ModelHelper.CloneFolder(oldFolder, newFolder);
                }
            }
            metadata.AfterSyncEditedFolders.Clear();

            foreach (string contextName in metadata.AfterSyncEditedContexts)
            {
                var newContext = lastWorkbook.Contexts.FirstOrDefault(c => c.Name.Equals(contextName, StringComparison.OrdinalIgnoreCase));
                var oldContext = this.workbook.Contexts.FirstOrDefault(c => newContext != null ? c.Id == newContext.Id : c.Name == contextName);

                if (newContext == null)
                {
                    // context was deleted
                    this.workbook.RemoveContext(contextName);
                }
                else if (oldContext != null)
                {
                    // context was updated
                    oldContext.Name = newContext.Name;
                    oldContext.Order = newContext.Order;
                }
                else
                {
                    // context was created
                    this.workbook.AddContext(contextName, newContext.SyncId, newContext.Id);
                }
            }
            metadata.AfterSyncEditedContexts.Clear();

            foreach (string smartviewName in metadata.AfterSyncEditedSmartViews)
            {
                var newSmartView = lastWorkbook.SmartViews.FirstOrDefault(c => c.Name.Equals(smartviewName, StringComparison.OrdinalIgnoreCase));
                var oldSmartView = this.workbook.SmartViews.FirstOrDefault(c => newSmartView != null ? c.Id == newSmartView.Id : c.Name == smartviewName);

                if (newSmartView == null)
                {
                    // smart view was deleted
                    this.workbook.RemoveSmartView(smartviewName);
                }
                else if (oldSmartView != null)
                {
                    // smart view was updated
                    oldSmartView.Name = newSmartView.Name;
                    oldSmartView.Order = newSmartView.Order;
                    oldSmartView.Rules = newSmartView.Rules;
                }
                else
                {
                    // smart view was created
                    var smartview = this.workbook.AddSmartView(smartviewName, newSmartView.Rules, newSmartView.Id);
                    smartview.SyncId = newSmartView.SyncId;
                }
            }
            metadata.AfterSyncEditedSmartViews.Clear();

            foreach (int id in metadata.AfterSyncEditedTasks)
            {
                var oldTask = this.workbook.Tasks.FirstOrDefault(t => t.Id == id);
                var newTask = lastWorkbook.Tasks.FirstOrDefault(t => t.Id == id);
                if (newTask == null)
                {
                    // task was deleted
                    if (oldTask != null)
                        oldTask.Delete();
                }
                else if (oldTask != null)
                {
                    // task was updated
                    ModelHelper.CloneTask(oldTask, newTask, this.workbook);
                }
                else
                {
                    // task was created
                    oldTask = this.workbook.CreateTask(newTask.Id);
                    ModelHelper.CloneTask(oldTask, newTask, this.workbook);
                }
            }
            metadata.AfterSyncEditedTasks.Clear();

            lastPersistence.CloseDatabase();
            await this.WriteSyncMetadataAsync(metadata);
        }

        private async Task<SynchronizationMetadata> ReadSyncMetadataAsync()
        {
            return await WinIsolatedStorage.RestoreAsync<SynchronizationMetadata>(SynchronizationMetadata.Filename);
        }

        private async Task WriteSyncMetadataAsync(SynchronizationMetadata metadata)
        {
            await WinIsolatedStorage.SaveAsync(metadata, SynchronizationMetadata.Filename);
        }        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Model;
using System.ComponentModel;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Shared.Sync
{
    public class SynchronizationManager : NotifyPropertyChanged, ISynchronizationManager
    {
        private readonly IPlatformService platformService;
        private readonly string platform;
        private readonly bool isBackground;
        private readonly ITrackingManager trackingManager;

        private readonly Dictionary<SynchronizationService, Func<ISynchronizationProvider>> providerFactory;
        private readonly Dictionary<SynchronizationService, ISynchronizationProvider> providerCache;

        protected SynchronizationMetadata metadata;
        private IWorkbook workbook;
        private bool isRunning;
        private bool saveInProgress;

        private ISynchronizationProvider provider;

        public event EventHandler OperationStarted;
        public event EventHandler<EventArgs<string>> OperationCompleted;
        public event EventHandler<SyncFailedEventArgs> OperationFailed;
        public event EventHandler<EventArgs<string>> OperationProgressChanged;

        public IWorkbook Workbook
        {
            get { return this.workbook; }
        }

        public ISynchronizationProvider ActiveProvider
        {
            get { return this.provider; }
        }

        public ISynchronizationMetadata Metadata
        {
            get
            {
                return this.metadata;
            }
            set
            {
                if (this.metadata != value)
                    this.metadata = (SynchronizationMetadata)value;
            }
        }

        public bool ActiveProviderSupportFeature(SyncFeatures features)
        {
            if (this.provider == null)
                return false;
            else
                return (this.provider.SupportedFeatures & features) == features;
        }

        public bool CanSyncOnStartup
        {
            get { return this.Workbook.Settings.GetValue<bool>(CoreSettings.SyncOnStartup) && this.IsSyncConfigured; }
        }

        public bool IsSyncConfigured
        {
            get
            {
                return this.ActiveService != SynchronizationService.None;
            }
        }

        public bool IsSyncRunning
        {
            get
            {
                return this.isRunning;
            }
        }

        public string LoginInfo
        {
            get
            {
                if (this.metadata.ActiveProvider == SynchronizationService.None || this.provider == null)
                    return string.Empty;

                return this.provider.LoginInfo;
            }
        }

        public string ServerInfo
        {
            get
            {
                if (this.metadata.ActiveProvider == SynchronizationService.None || this.provider == null)
                    return string.Empty;

                return this.provider.ServerInfo;
            }
        }

        public string FolderInfo
        {
            get
            {
                if (this.metadata.ActiveProvider == SynchronizationService.None || this.provider == null)
                    return string.Empty;

                return this.provider.FolderInfo;
            }
        }

        public SynchronizationService ActiveService
        {
            get
            {
                return this.metadata.ActiveProvider;
            }
            set
            {
                if (this.metadata.ActiveProvider != value)
                {
                    this.metadata.ActiveProvider = value;

                    // perform cleanup if we turn off sync
                    if (this.metadata.ActiveProvider == SynchronizationService.None)
                    {
                        this.metadata.Reset();
                    }
                    else
                    {
                        this.provider = this.GetProvider(this.metadata.ActiveProvider);
                    }

                    this.OnPropertyChanged("ActiveService");
                    this.OnPropertyChanged("ActiveProvider");
                }
            }
        }
        
        public bool IsBackground
        {
            get { return this.isBackground; }
        }

        public string Platform
        {
            get { return this.platform; }
        }

        public SyncPrioritySupport SyncPrioritySupport
        {
            get
            {
                if (this.ActiveService == SynchronizationService.OutlookActiveSync ||
                    this.ActiveService == SynchronizationService.ActiveSync ||
                    this.ActiveService == SynchronizationService.Exchange ||
                    this.ActiveService == SynchronizationService.ExchangeEws)
                {
                    return SyncPrioritySupport.LowMediumHigh;
                }

                return SyncPrioritySupport.Normal;
            }
        }

        public SynchronizationManager(IPlatformService platformService, ITrackingManager trackingManager, string platform, bool isBackground)
        {
            if (platformService == null)
                throw new ArgumentNullException(nameof(platformService));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));

            this.platformService = platformService;
            this.platform = platform;
            this.isBackground = isBackground;

            this.providerFactory = new Dictionary<SynchronizationService, Func<ISynchronizationProvider>>();
            this.providerCache = new Dictionary<SynchronizationService, ISynchronizationProvider>();
            this.trackingManager = trackingManager;
        }

        public void TrackSyncEvent(string eventName, DateTime? start = null)
        {
            var attributes = new Dictionary<string, string>
            {
                { "service", this.ActiveService.ToString() },
                { "background", this.isBackground.ToString() }
            };

            if (start.HasValue)
            {
                double durationSec = (DateTime.UtcNow - start.Value).TotalSeconds;

                string durationDescription;
                if (durationSec <= 1)
                    durationDescription = "<1s";
                else if (durationSec <= 5)
                    durationDescription = "<5s";
                else if (durationSec <= 10)
                    durationDescription = "<10s";
                else if (durationSec <= 20)
                    durationDescription = "<20s";
                else if (durationSec <= 30)
                    durationDescription = "<30s";
                else
                    durationDescription = ">30s";

                attributes.Add("duration", durationDescription);
            }

            this.trackingManager.TagEvent(eventName, attributes);
        }

        public void TrackEvent(string category, string section = null)
        {
            if (section == null)
                section = string.Empty;

            var source = this.isBackground ? TrackingSource.SyncBackground : TrackingSource.SyncMessage;
            this.trackingManager.Event(source, category, section);
        }

        public async Task AdvancedSync(SyncAdvancedMode mode)
        {
            switch (mode)
            {
                case SyncAdvancedMode.Replace:
                    this.trackingManager.TagEvent("Sync replace", new Dictionary<string, string> { { "service", this.ActiveService.ToString() } });
                    var replaceManager = new AdvancedReplaceSyncronizationManager(this.Workbook, this, this.trackingManager);
                    await replaceManager.AdvancedSync();
                    break;
                case SyncAdvancedMode.Merge:
                    /*this.Event("Advanced", "Start merge " + this.ActiveService);
                    var mergeManager = new AdvancedMergeSyncronizationManager(this.Workbook, this);
                    await mergeManager.AdvancedSync();
                    this.Event("Advanced", "End merge " + this.ActiveService);*/
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        public void AttachWorkbook(IWorkbook target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (this.workbook != null)
                throw new InvalidOperationException("Workbook is already attached");

            this.workbook = target;

            if (this.metadata != null)
            {
                this.ActiveService = this.metadata.ActiveProvider;
            }
            else
            {
                this.metadata = new SynchronizationMetadata();
            }

            this.provider = this.GetProvider(this.metadata.ActiveProvider);

            if (this.provider != null)
            {
                // make sure we have valid information for syncing
                if (string.IsNullOrEmpty(this.provider.ServerInfo) || string.IsNullOrEmpty(this.provider.LoginInfo))
                {
                    this.ActiveService = SynchronizationService.None;
                }
            }

            // listen for changes in the workbook
            this.RegisterWorkbookEvents();
        }

        public void Cancel()
        {
            if (this.provider != null && this.isRunning)
                this.provider.Cancel();
        }

        public async Task InitializeAsync()
        {
            try
            {
                this.metadata = await this.platformService.LoadFileAsync<SynchronizationMetadata>(SynchronizationMetadata.Filename);
                if (this.metadata == null)
                {
                    // do not use this.Workbook as it's null at this point because AttachWorkbook wasn't called yet
                    int launchCount = this.platformService.GetSettingValue<int>(CoreSettings.LaunchCount);                    
                    if (launchCount > 5)
                    {
                        TrackingManagerHelper.Trace("SynchronizationManager.InitializeAsync: null metadata while launch count is " + launchCount);
                    }
                    this.metadata = new SynchronizationMetadata();
                }
            }
            catch (Exception)
            {
                this.metadata = new SynchronizationMetadata();
            }
        }

        public async Task PrepareProviderAsync()
        {
            if (this.provider != null)
                await this.provider.PrepareAsync();
        }

        public async Task Sync()
        {
            // check that network connectivity is available
            if (!this.platformService.IsNetworkAvailable)
            {
                this.OnProviderSynchronizationFailed(this, StringResources.Sync_NoNetworkConnectivity);
                return;
            }

            this.OperationStarted.Raise(this);

            this.OnProviderProgressChanged(this, new EventArgs<string>(StringResources.Sync_PreparingSync));
                
            var ready = await this.PrepareSync();

            if (!ready)
            {
                this.OnProviderSynchronizationFailed(this, StringResources.Sync_Misconfigured);
            }
            else
            {
                DateTime start = DateTime.UtcNow;

                // await the async call to be able to catch any exceptions...
                try
                {
                    this.TrackSyncEvent("Sync start");

                    using (this.workbook.WithTransaction())
                    {
                        await this.provider.SyncAsync();
                    }

                    this.workbook.CommitEditedChanges();

                    this.TrackSyncEvent("Sync success", start);
                }
                catch (OperationCanceledException e)
                {
                    this.TrackSyncEvent("Sync cancel", start);

                    this.OnProviderSynchronizationFailed(this, string.Format(StringResources.Sync_Cancelled, e.Message));
                }
                catch (Exception e)
                {
                    this.TrackSyncEvent("Sync fail", start);
                    
                    if (e.ToString().Contains(Constants.WinCollectionUIExceptionContent))
                        throw;

                    this.OnProviderSynchronizationFailed(this, e.Message, e);
                }
                finally
                {
                    if (this.provider != null)
                    {
                        this.provider.OperationProgressChanged -= this.OnProviderProgressChanged;
                        this.provider.OperationCompleted -= this.OnProviderSynchronizationCompleted;
                        this.provider.OperationFailed -= this.OnCurrentProviderSynchronizationFailed;
                    }
                }

                this.metadata.ClearIgnoreList();
                await this.SaveAsync();

                this.isRunning = false;
            }
        }
        
        public void Reset(bool clearSettings = true)
        {
            if (this.provider != null)
                this.provider.Reset(clearSettings);

            foreach (var folder in this.workbook.Folders)
                folder.SyncId = null;

            foreach (var task in this.workbook.Tasks)
                task.SyncId = null;

            foreach (var context in this.workbook.Contexts)
                context.SyncId = null;

            foreach (var smartview in this.workbook.SmartViews)
                smartview.SyncId = null;

            this.Workbook.Settings.SetValue(CoreSettings.SyncWarningContextNotSupported, false);
            this.Workbook.Settings.SetValue<string>(CoreSettings.SyncAuthToken, null);

            this.metadata.Reset();

            this.ActiveService = SynchronizationService.None;
        }

        public void RegisterProvider(SynchronizationService newProvider, Func<ISynchronizationProvider> createProvider)
        {
            if (createProvider == null)
                throw new ArgumentNullException("createProvider");
            if (newProvider == SynchronizationService.None)
                throw new ArgumentException("Provider cannot be set to None");

            if (this.providerFactory.ContainsKey(newProvider))
                throw new ArgumentException("This provider is already registered");

            this.providerFactory.Add(newProvider, createProvider);
        }

        public async Task SaveAsync()
        {
            if (this.saveInProgress || this.metadata == null)
            {
                return;
            }

            this.saveInProgress = true;

            await this.platformService.SaveFileAsync(this.metadata, SynchronizationMetadata.Filename);

            this.saveInProgress = false;
        }

        private void RegisterWorkbookEvents()
        {
            this.workbook.FolderChanged += this.OnFolderPropertyChanged;
            this.workbook.FolderAdded += this.OnFolderAdded;
            this.workbook.FolderRemoved += this.OnFolderRemoved;

            this.workbook.ContextChanged += this.OnContextPropertyChanged;
            this.workbook.ContextAdded += this.OnContextAdded;
            this.workbook.ContextRemoved += this.OnContextRemoved;

            this.workbook.SmartViewChanged += this.OnSmartViewPropertyChanged;
            this.workbook.SmartViewAdded += this.OnSmartViewAdded;
            this.workbook.SmartViewRemoved += this.OnSmartViewRemoved;

            this.workbook.TaskChanged += this.OnTaskPropertyChanged;
            this.workbook.TaskAdded += this.OnTaskAdded;
            this.workbook.TaskRemoved += this.OnTaskRemoved;
        }

        private async Task<bool> PrepareSync()
        {
            this.isRunning = true;

            if (this.metadata.ActiveProvider == SynchronizationService.None || this.provider == null)
                return false;

            // if this is the first sync, clear the metada
            if (this.metadata.LastSync == DateTime.MinValue)
                this.metadata.Reset();

            this.metadata.ClearIgnoreList();

            // check if we have task in the Edited list with no sync id
            // in this case, move those tasks in the Added list instead
            foreach (var taskId in this.metadata.EditedTasks.Select(t => t.Key).ToList())
            {
                var task = this.workbook.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null && task.SyncId == null)
                {
                    TrackingManagerHelper.Trace(string.Format("SyncManagerBase - Task '{0}' in the edited task list without id, adding as new task", task.Title));
                    this.metadata.EditedTasks.Remove(taskId);
                    this.metadata.AddedTasks.Add(taskId);
                }
            }

            this.provider.OperationProgressChanged += this.OnProviderProgressChanged;
            this.provider.OperationCompleted += this.OnProviderSynchronizationCompleted;
            this.provider.OperationFailed += this.OnCurrentProviderSynchronizationFailed;

            return true;
        }
        
        public ISynchronizationProvider GetProvider(SynchronizationService service)
        {
            if (this.providerCache.ContainsKey(service))
                return this.providerCache[service];

            if (this.providerFactory.ContainsKey(service))
            {
                var newProvider = this.providerFactory[service]();
                this.providerCache[service] = newProvider;

                return newProvider;
            }

            return null;
        }

        public void SetDueDate(DateTime? due, DateTime? start, Action<DateTime?> setStart)
        {
            if (this.ActiveProvider != null)
                this.ActiveProvider.SetDueDate(due, start, setStart);
        }

        public void SetStartDate(DateTime? due, DateTime? start, Action<DateTime?> setDue)
        {
            if (this.ActiveProvider != null)
                this.ActiveProvider.SetStartDate(due, start, setDue);
        }

        private void OnProviderProgressChanged(object sender, EventArgs<string> e)
        {
            this.OperationProgressChanged.Raise(this, e);
        }

        protected void OnProviderSynchronizationFailed(object sender, string message, Exception exception = null)
        {
            this.OperationFailed.Raise(this, new SyncFailedEventArgs(message, exception));
        }

        private void OnProviderSynchronizationCompleted(object sender, EventArgs<string> e)
        {
            this.metadata.LastSync = DateTime.Now;

            this.OperationCompleted.Raise(this, e);
        }

        private void OnCurrentProviderSynchronizationFailed(object sender, EventArgs<string> e)
        {
            this.OnProviderSynchronizationFailed(sender, e.Item);
        }

        private void OnFolderAdded(object sender, EventArgs<IFolder> e)
        {
            this.metadata.FolderAdded(e.Item);
        }

        private void OnFolderRemoved(object sender, EventArgs<IFolder> e)
        {
            this.metadata.FolderRemoved(e.Item);
        }

        private void OnFolderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender as IFolder == null || e.PropertyName != "Modified")
                return;

            this.metadata.FolderEdited((IFolder)sender);
        }

        private void OnContextAdded(object sender, EventArgs<IContext> e)
        {
            this.metadata.ContextAdded(e.Item);
        }

        private void OnContextRemoved(object sender, EventArgs<IContext> e)
        {
            this.metadata.ContextRemoved(e.Item);
        }

        private void OnContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender as IContext == null || e.PropertyName != "Name")
                return;

            this.metadata.ContextEdited((IContext)sender);
        }

        private void OnSmartViewAdded(object sender, EventArgs<ISmartView> e)
        {
            this.metadata.SmartViewAdded(e.Item);
        }

        private void OnSmartViewRemoved(object sender, EventArgs<ISmartView> e)
        {
            this.metadata.SmartViewRemoved(e.Item);
        }

        private void OnSmartViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender as ISmartView == null || e.PropertyName == "TaskCount" || e.PropertyName == "SyncId")
                return;

            this.metadata.SmartViewEdited((ISmartView)sender);
        }

        private void OnTaskAdded(object sender, EventArgs<ITask> e)
        {
            this.metadata.TaskAdded(e.Item);
        }

        private void OnTaskRemoved(object sender, EventArgs<ITask> e)
        {
            this.metadata.TaskRemoved(e.Item);
        }

        private void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender as ITask == null)
                return;

            this.metadata.TaskEdited((ITask)sender, e.PropertyName);
        }
    }
}

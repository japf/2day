using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;

namespace Chartreuse.Today.App.Test.Impl
{
    public class TestSynchronizationManager : ISynchronizationManager
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler OperationStarted;
        public event EventHandler<EventArgs<string>> OperationCompleted;
        public event EventHandler<SyncFailedEventArgs> OperationFailed;
        public event EventHandler<EventArgs<string>> OperationProgressChanged;

        public bool IsSyncRunning { get; set; }
        public void Cancel()
        {
        }

        public IWorkbook Workbook { get; }
        public string LoginInfo { get; }
        public string ServerInfo { get; }
        public string FolderInfo { get; }
        public SynchronizationService ActiveService { get; set; }
        public ISynchronizationProvider ActiveProvider { get; }
        public ISynchronizationMetadata Metadata { get; }
        public bool ActiveProviderSupportFeature(SyncFeatures features)
        {
            return false;
        }

        public bool CanSyncOnStartup { get; }
        public bool CanSyncInBackground { get; }
        public bool IsSyncConfigured { get; }
        public bool IsBackground { get; }
        public string Platform { get; }
        public SyncPrioritySupport SyncPrioritySupport { get; }

        public Task InitializeAsync()
        {
            return null;
        }

        public Task PrepareProviderAsync()
        {
            return null;
        }

        public Task Sync()
        {
            return null;
        }

        public Task AdvancedSync(SyncAdvancedMode mode)
        {
            return null;
        }

        public void AttachWorkbook(IWorkbook target)
        {
        }

        public void Reset(bool clearSettings = true)
        {
        }

        public Task SaveAsync()
        {
            return null;
        }

        public void RegisterProvider(SynchronizationService provider, Func<ISynchronizationProvider> createProvider)
        {
        }

        public ISynchronizationProvider GetProvider(SynchronizationService synchronizationProvider)
        {
            return null;
        }

        public void SetDueDate(DateTime? due, DateTime? start, Action<DateTime?> setStart)
        {
        }

        public void SetStartDate(DateTime? due, DateTime? start, Action<DateTime?> setDue)
        {
        }

        public void TrackEvent(string category, string section)
        {
        }
    }
}
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Shared.Sync
{
    public interface ISynchronizationManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when the synchronization has started
        /// </summary>
        event EventHandler OperationStarted;

        /// <summary>
        /// Occurs when the synchronization has completed
        /// </summary>
        event EventHandler<EventArgs<string>> OperationCompleted;

        /// <summary>
        /// Occurs when the synchronisation has failed
        /// </summary>
        event EventHandler<SyncFailedEventArgs> OperationFailed;

        /// <summary>
        /// Occurs when the synchronization progress has changed
        /// </summary>
        event EventHandler<EventArgs<string>> OperationProgressChanged;

        bool IsSyncRunning { get; }

        void Cancel();

        IWorkbook Workbook { get; }

        string LoginInfo { get; }
        string ServerInfo { get; }
        string FolderInfo { get; }

        SynchronizationService ActiveService { get; set; }
        ISynchronizationProvider ActiveProvider { get; }
        ISynchronizationMetadata Metadata { get; }

        bool ActiveProviderSupportFeature(SyncFeatures features);

        bool CanSyncOnStartup { get; }
        bool IsSyncConfigured { get; }
        bool IsBackground { get; }
        string Platform { get; }

        SyncPrioritySupport SyncPrioritySupport { get; }

        Task InitializeAsync();
        Task PrepareProviderAsync();

        Task Sync();
        Task AdvancedSync(SyncAdvancedMode mode);

        void AttachWorkbook(IWorkbook target);

        void Reset(bool clearSettings = true);
        Task SaveAsync();

        void RegisterProvider(SynchronizationService provider, Func<ISynchronizationProvider> createProvider);

        ISynchronizationProvider GetProvider(SynchronizationService synchronizationProvider);

        void SetDueDate(DateTime? due, DateTime? start, Action<DateTime?> setStart);
        void SetStartDate(DateTime? due, DateTime? start, Action<DateTime?> setDue);

        void TrackEvent(string category, string section);
    }
}
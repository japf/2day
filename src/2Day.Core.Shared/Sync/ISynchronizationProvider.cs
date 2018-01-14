using System;
using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Sync
{
    public interface ISynchronizationProvider
    {
        /// <summary>
        /// Occurs when the synchronization has completed
        /// </summary>
        event EventHandler<EventArgs<string>> OperationCompleted;

        /// <summary>
        /// Occurs when the synchronisation has failed
        /// </summary>
        event EventHandler<EventArgs<string>> OperationFailed;

        /// <summary>
        /// Occurs when the synchronization progress has changed
        /// </summary>
        event EventHandler<EventArgs<string>> OperationProgressChanged;

        Task<bool> SyncAsync();
        Task<string> DeleteAccountAync();
        Task<bool> CheckLoginAsync();

        string LoginInfo { get; }
        string ServerInfo { get; }
        string FolderInfo { get; }
        string DefaultFolderName { get; }

        string Name { get; }
        string Headline { get; }
        string Description { get; }
        bool CanDeleteAccount { get; }

        SynchronizationService Service { get; }
        SyncFeatures SupportedFeatures { get;}

        Task PrepareAsync();
        void Reset(bool clearSettings);
        void Cancel();
        
        void SetDueDate(DateTime? due, DateTime? start, Action<DateTime?> setStart);
        void SetStartDate(DateTime? due, DateTime? start, Action<DateTime?> setDue);
    }
}

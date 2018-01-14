using System.Threading.Tasks;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange.Providers.SyncService
{
    public interface IExchangeSyncService
    {
        string TaskFolderId { get; }

        string TaskFolderName { get; }

        Task<ExchangeAuthorizationResult> LoginAsync(ExchangeConnectionInfo connectionInfo);

        Task<ExchangeSyncResult> ExecuteSyncAsync(ExchangeConnectionInfo connectionInfo, ExchangeChangeSet changeSet, string syncState, string folderId);

        Task<ExchangeSyncResult> ExecuteFirstSyncAsync(ExchangeConnectionInfo connectionInfo, ExchangeChangeSet changeSet);

        void CancelCurrentOperation();

        void ResetCache();
    }
}

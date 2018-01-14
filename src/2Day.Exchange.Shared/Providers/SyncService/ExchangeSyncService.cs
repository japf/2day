using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange.Providers.SyncService
{
    public class ExchangeSyncService : IExchangeSyncService
    {
        private readonly string serverUri;
        private readonly WebRequestDefaultBuilder requestBuilder;

        private string taskFolderName;

        public string TaskFolderId
        {
            get { return string.Empty; }
        }

        public string TaskFolderName
        {
            get { return this.taskFolderName; }
        }

        public ExchangeSyncService(string serverUri)
        {
            if (string.IsNullOrEmpty(serverUri))
                throw new ArgumentNullException("serverUri");

            this.serverUri = serverUri;
            this.requestBuilder = new WebRequestDefaultBuilder();
        }

        public async Task<ExchangeAuthorizationResult> LoginAsync(ExchangeConnectionInfo connectionInfo)
        {
            var content = await this.requestBuilder.PostJsonAsync<ExchangeConnectionInfo, ExchangeAuthorizationResult>(this.serverUri + "/Api/Sync/Login", connectionInfo);

            return content;
        }

        public async Task<ExchangeSyncResult> ExecuteSyncAsync(ExchangeConnectionInfo connectionInfo, ExchangeChangeSet changeSet, string syncState, string folderId)
        {
            var syncRequest = new ExchangeSyncRequest
            {
                ConnectionInfo = connectionInfo,
                ChangeSet = changeSet,
                SyncState = syncState
            };
            var content = await this.requestBuilder.PostJsonAsync<ExchangeSyncRequest, ExchangeSyncResult>(this.serverUri + "/Api/Sync/Sync", syncRequest);

            this.taskFolderName = content.FolderName;

            return content;
        }

        public async Task<ExchangeSyncResult> ExecuteFirstSyncAsync(ExchangeConnectionInfo connectionInfo, ExchangeChangeSet changeSet)
        {
            return await this.ExecuteSyncAsync(connectionInfo, changeSet, null, null);
        }

        public void CancelCurrentOperation()
        {
            // not supported
        }

        public void ResetCache()
        {
        }
    }
}
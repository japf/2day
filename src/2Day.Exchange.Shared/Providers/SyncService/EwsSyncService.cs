using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Exchange.ActiveSync.Exceptions;
using Chartreuse.Today.Exchange.Ews;
using Chartreuse.Today.Exchange.Ews.AutoDiscover;
using Chartreuse.Today.Exchange.Ews.Commands;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange.Providers.SyncService
{
    public class EwsSyncService : IExchangeSyncService
    {
        private const string SearchFolderName = "2Day Flagged Items";

        private readonly IWorkbook workbook;

        public string TaskFolderId { get; private set; }

        public string TaskFolderName
        {
            get { return string.Empty; }
        }

        public EwsSyncService(IWorkbook workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            this.workbook = workbook;
        }

        public async Task<ExchangeAuthorizationResult> LoginAsync(ExchangeConnectionInfo connectionInfo)
        {
            GetFolderIdentifiersResult result;

            var autoDiscoverResult = await this.EnsureAutoDiscover(connectionInfo);
            if (!IsExchangeSyncResultValid(autoDiscoverResult))
                return autoDiscoverResult.AuthorizationResult;
            
            try
            {
                var server = new EwsSyncServer(connectionInfo.CreateEwsSettings());
                result = await server.GetRootFolderIdentifiersAsync(useCache: false);
            }
            catch (Exception e)
            {
                return HandleLoginException(e);
            }

            if (result != null && result.TaskFolderIdentifier != null)
            {
                return new ExchangeAuthorizationResult
                {
                    AuthorizationStatus = ExchangeAuthorizationStatus.OK,
                    IsOperationSuccess = true,
                    ServerUri = connectionInfo.ServerUri
                };
            }
            else
            {
                return new ExchangeAuthorizationResult
                {
                    AuthorizationStatus = ExchangeAuthorizationStatus.Unkown,
                    ErrorMessage = "Unable to find mailbox identifiers",
                    IsOperationSuccess = false
                };
            }
        }

        private static ExchangeAuthorizationResult HandleLoginException(Exception e)
        {
            var authResult = new ExchangeAuthorizationResult
            {
                AuthorizationStatus = ExchangeAuthorizationStatus.Unkown,
                ErrorMessage = e.Message,
                IsOperationSuccess = false
            };

            if (e is CommandException)
            {
                var commandException = (CommandException)e;
                if (commandException.InnerMessage != null)
                    authResult.ErrorMessage = commandException.InnerMessage;

                if (commandException.Response != null)
                {
                    switch (commandException.Response.StatusCode)
                    {
                        case HttpStatusCode.Forbidden:
                            authResult.AuthorizationStatus = ExchangeAuthorizationStatus.ServerAccessForbidden;
                            break;
                        case HttpStatusCode.MethodNotAllowed:
                            authResult.AuthorizationStatus = ExchangeAuthorizationStatus.ServerMethodNotAllowed;
                            break;
                        case HttpStatusCode.Unauthorized:
                            authResult.AuthorizationStatus = ExchangeAuthorizationStatus.UserCredentialsInvalid;
                            break;
                    }
                }
            }

            if (e is CommandAuthorizationException || e.InnerException is CommandAuthorizationException)
                authResult.AuthorizationStatus = ExchangeAuthorizationStatus.UserCredentialsInvalid;

            if (e.InnerException is HttpRequestException)
                authResult.AuthorizationStatus = ExchangeAuthorizationStatus.HostNotFound;

            return authResult;
        }

        public async Task<ExchangeSyncResult> ExecuteSyncAsync(ExchangeConnectionInfo connectionInfo, ExchangeChangeSet changeSet, string syncState, string folderId)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException(nameof(connectionInfo));
            if (changeSet == null)
                throw new ArgumentNullException(nameof(changeSet));

            var outChangeSet = new ExchangeChangeSet();

            var autoDiscoverResult = await this.EnsureAutoDiscover(connectionInfo);
            if (!IsExchangeSyncResultValid(autoDiscoverResult))
                return autoDiscoverResult;
            
            var server = new EwsSyncServer(connectionInfo.CreateEwsSettings());

            var identifiers = await server.GetRootFolderIdentifiersAsync();
            //var subFolders = await server.GetSubFoldersAsync(identifiers.TaskFolderIdentifier);

            //var flaggedItemFolders = await this.EnsureSearchFolder(server);
            /*
            var a = await server.EnumerateFolderContentAsync(flaggedItemFolders);
            var d = await server.DownloadFolderContentAsync(a);
            */

            var mapId = new List<ExchangeMapId>();

            await this.PushLocalChanges(changeSet, server, mapId, identifiers.TaskFolderIdentifier);

            var remoteIdentifiers = await this.ApplyRemoteChanges(outChangeSet, server, mapId, identifiers.TaskFolderIdentifier, EwsItemType.Task);
            /*foreach (var subFolder in subFolders.Folders)
            {
                await this.ApplyRemoteChanges(outChangeSet, server, mapId, subFolder);                
            }*/

            if (connectionInfo.SyncFlaggedItems)
            {
                try
                {
                    var identifier = await this.EnsureSearchFolderAsync(server);
                    var otherRemoteIdentifiers = await this.ApplyRemoteChanges(outChangeSet, server, mapId, identifier, EwsItemType.Item);
                    remoteIdentifiers.AddRange(otherRemoteIdentifiers);
                }
                catch (Exception ex)
                {
                    LogService.Log("EwsSyncService", $"Exception while syncing flagged items: {ex}");
                    TrackingManagerHelper.Exception(ex, "Exception while syncing flagged items");
                }
            }

            // check for deleted items
            foreach (var task in this.workbook.Tasks.Where(t => t.SyncId != null))
            {
                var remoteId = remoteIdentifiers.FirstOrDefault(i => i.Id == task.SyncId.GetId());
                if (remoteId == null)
                {
                    // local item not found on server because it was deleted => delete
                    outChangeSet.DeletedTasks.Add(new ServerDeletedAsset(task.SyncId));
                }
            }

            var result = new ExchangeSyncResult
            {
                AuthorizationResult = new ExchangeAuthorizationResult
                {
                    Status = "OK",
                    AuthorizationStatus = ExchangeAuthorizationStatus.OK,
                    IsOperationSuccess = true,
                    ServerUri = connectionInfo.ServerUri
                },
                ChangeSet = outChangeSet,
                MapId = mapId,
                SyncState = identifiers.TaskFolderIdentifier.ChangeKey,
                OperationResult = new ServiceOperationResult
                {
                    IsOperationSuccess = true
                }
            };

            return result;
        }

        public async Task<EwsFolderIdentifier> EnsureSearchFolderAsync(EwsSyncServer server)
        {
            var searchFolders = await server.GetSubFoldersAsync(EwsKnownFolderIdentifiers.SearchFolders);
            var searchFolder = searchFolders.Folders.FirstOrDefault(f => f.DisplayName.Equals(SearchFolderName, StringComparison.OrdinalIgnoreCase));
            if (searchFolder != null)
                return searchFolder;

            // search folder does not exist, create it
            var result = await server.CreateSearchFolderAsync(SearchFolderName);
            if (result.Identifiers.Count == 1)
            {
                LogService.Log("EwsSyncService", "Search folder created");
                return result.Identifiers[0];
            }

            LogService.Log("EwsSyncService", "Unable to create search folder");

            return null;
        }

        private async Task<ExchangeSyncResult> EnsureAutoDiscover(ExchangeConnectionInfo connectionInfo)
        {
            var success = new ExchangeSyncResult
            {
                AuthorizationResult = new ExchangeAuthorizationResult
                {
                    AuthorizationStatus = ExchangeAuthorizationStatus.OK,
                    IsOperationSuccess = true
                },
                OperationResult = new ServiceOperationResult
                {
                    IsOperationSuccess = true
                },
            };

            // check Office 365 server uri is correct (ie. https://outlook.office365.com/EWS/Exchange.asmx)
            if (connectionInfo.ServerUri != null)
            {
                string uri = connectionInfo.ServerUri.ToString().ToLowerInvariant();
                if (uri.Contains("outlook.office365.com") && !uri.EndsWith("/ews/exchange.asmx"))
                    connectionInfo.ServerUri = SafeUri.Get("https://outlook.office365.com/EWS/Exchange.asmx");

                if (uri.Contains("/ews/exchange.asmx/ews/exchange.asmx"))
                    connectionInfo.ServerUri = SafeUri.Get(uri.Replace("/ews/exchange.asmx/ews/exchange.asmx", "/EWS/Exchange.asmx"));

                // check if the server uri looks correct, if it doesn't, run autodiscover
                var server = new EwsSyncServer(connectionInfo.CreateEwsSettings());
                try
                {
                    var identifiers = await server.GetRootFolderIdentifiersAsync(useCache: false);
                    if (identifiers != null)
                        return success;
                }
                catch (Exception ex)
                {
                    if (connectionInfo.ServerUri != null && !connectionInfo.ServerUri.ToString().EndsWith("/ews/exchange.asmx", StringComparison.OrdinalIgnoreCase))
                    {
                        // one more try with the /ews/exchange.asmx path
                        connectionInfo.ServerUri = SafeUri.Get(uri.TrimEnd('/') + "/ews/exchange.asmx");
                        var secondResult = await this.EnsureAutoDiscover(connectionInfo);
                        if (IsExchangeSyncResultValid(secondResult))
                            return success;
                    }
                    LogService.Log("EwsSyncService", $"Server uri is {connectionInfo.ServerUri} but GetRootFolderIds failed ({ex.Message}), fallbacking to classic autodiscover");
                }
            }

            LogService.Log("EwsSyncService", "Server uri is empty, performing auto discover");

            var engine = new AutoDiscoverEngine();
            var autoDiscoverResult = await engine.AutoDiscoverAsync(connectionInfo.Email, connectionInfo.Username, connectionInfo.Password, connectionInfo.Version);

            if (autoDiscoverResult != null)
            {
                if (autoDiscoverResult.ServerUri != null)
                {
                    LogService.Log("EwsSyncService", "Now using server uri: " + autoDiscoverResult.ServerUri);
                    connectionInfo.ServerUri = autoDiscoverResult.ServerUri;
                }

                if (!string.IsNullOrWhiteSpace(autoDiscoverResult.RedirectEmail))
                {
                    LogService.Log("EwsSyncService", "Now using email redirect: " + autoDiscoverResult.RedirectEmail);
                    connectionInfo.Email = autoDiscoverResult.RedirectEmail;
                }
                return success;
            }

            LogService.Log("EwsSyncService", "Auto discovery failed");

            return new ExchangeSyncResult
            {
                AuthorizationResult = new ExchangeAuthorizationResult
                {
                    AuthorizationStatus = ExchangeAuthorizationStatus.AutodiscoveryServiceNotFound,
                    ErrorMessage = "Check credentials are valid and/or try to specify manually a valid server address"
                }
            };
        }

        private async Task<List<EwsItemIdentifier>> ApplyRemoteChanges(ExchangeChangeSet changeSet, EwsSyncServer server, List<ExchangeMapId> mapId, EwsFolderIdentifier folderIdentifier, EwsItemType ewsItemType)
        {
            // enumerate remote content
            var remoteIdentifiers = await server.EnumerateFolderContentAsync(folderIdentifier, ewsItemType);

            var editedItems = new List<EwsItemIdentifier>();
            var newItems = new List<EwsItemIdentifier>();

            // check for remote identifiers that have a new change key (ie. updated elements in Exchange)
            foreach (var task in this.workbook.Tasks.Where(t => t.SyncId != null))
            {
                // check if we can find this task that exists in the workbook in the remote identifiers
                var remoteId = remoteIdentifiers.FirstOrDefault(i => i.Id == task.SyncId.GetId());

                // we find this task in the remote identifiers, check if the change key has changed since last sync
                if (remoteId != null && remoteId.ChangeKey != task.SyncId.GetEwsItemIdentifier().ChangeKey)
                {
                    // local item found on server with a different ChangeKey because it was updated => update
                    editedItems.Add(new EwsItemIdentifier(remoteId.Id, remoteId.ChangeKey));
                }
            }

            // check for remote identifiers that we don't have in the workbook and that were not added during this sync operation
            foreach (var identifier in remoteIdentifiers.Where(i => mapId.All(id => id.ExchangeId != null && id.ExchangeId.GetId() != i.Id)))
            {
                var task = this.workbook.Tasks.Where(t => t.SyncId != null).FirstOrDefault(t => t.SyncId.GetId() == identifier.Id);
                if (task == null)
                {
                    // item found on server but not locally => new item
                    newItems.Add(identifier);
                }
            }

            var items = new List<EwsItemIdentifier>();
            items.AddRange(editedItems);
            items.AddRange(newItems);

            var ewsTasks = await server.DownloadFolderContentAsync(items, ewsItemType);
            foreach (var ewsTask in ewsTasks)
            {
                if (editedItems.Any(i => i.Id == ewsTask.Id))
                    changeSet.ModifiedTasks.Add(ewsTask.BuildExchangeTask());
                else if (newItems.Any(i => i.Id == ewsTask.Id))
                    changeSet.AddedTasks.Add(ewsTask.BuildExchangeTask());
                else
                    throw new NotSupportedException();
            }

            return remoteIdentifiers;
        }

        private async Task PushLocalChanges(ExchangeChangeSet changeSet, EwsSyncServer server, List<ExchangeMapId> mapId, EwsFolderIdentifier folder)
        {
            // send local add content
            if (changeSet.AddedTasks.Count > 0)
            {
                var ewsTasks = changeSet.AddedTasks.Select(exchangeTask => exchangeTask.BuildEwsTask()).ToList();
                var createItemResult = await server.CreateItemAsync(ewsTasks, folder);
                if (changeSet.AddedTasks.Count == createItemResult.Identifiers.Count)
                {
                    for (int i = 0; i < createItemResult.Identifiers.Count; i++)
                    {
                        var identifier = createItemResult.Identifiers[i];
                        if (identifier.IsValid)
                            mapId.Add(new ExchangeMapId { LocalId = changeSet.AddedTasks[i].LocalId, ExchangeId = identifier.GetFullId() });
                        else
                            LogService.Log("EwsSyncService", "Error while creating task: " + changeSet.AddedTasks[i].Subject + " error: " + identifier.ErrorMessage);
                    }
                }
                else
                {
                    LogService.Log("EwsSyncService", string.Format("Request to create {0} items but result only has {1} items", changeSet.AddedTasks.Count, createItemResult.Identifiers.Count));
                }
            }

            // send local modification content
            if (changeSet.ModifiedTasks.Count > 0)
            {
                var ewsTasks = changeSet.ModifiedTasks.Select(exchangeTask => exchangeTask.BuildEwsTask()).ToList();
                var updateItemResult = await server.UpdateItemsAsync(ewsTasks);

                // todo ews: we don't update change key here... not optimal because then we're going to ask 
                // the server to enumerate items, we will find different change keys for update items and so
                // we're going to download them again from the server
            }

            // send local delete content
            if (changeSet.DeletedTasks.Count > 0)
            {
                var ewsIdentifiers = new List<EwsItemIdentifier>();
                foreach (var deletedAsset in changeSet.DeletedTasks)
                    ewsIdentifiers.Add(deletedAsset.Id.GetEwsItemIdentifier());

                await server.DeleteItemsAsync(ewsIdentifiers);
            }
        }

        public async Task<ExchangeSyncResult> ExecuteFirstSyncAsync(ExchangeConnectionInfo connectionInfo, ExchangeChangeSet changeSet)
        {
            return await this.ExecuteSyncAsync(connectionInfo, changeSet, null, null);
        }

        public void CancelCurrentOperation()
        {
        }

        public void ResetCache()
        {
        }

        private static bool IsExchangeSyncResultValid(ExchangeSyncResult exchangeSyncResult)
        {
            return exchangeSyncResult != null && exchangeSyncResult.AuthorizationResult != null && exchangeSyncResult.AuthorizationResult.AuthorizationStatus == ExchangeAuthorizationStatus.OK;            
        }
    }
}

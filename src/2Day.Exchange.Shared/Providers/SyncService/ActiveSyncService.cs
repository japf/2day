using System;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Exchange.ActiveSync;
using Chartreuse.Today.Exchange.ActiveSync.Commands;
using Chartreuse.Today.Exchange.Model;
using Chartreuse.Today.Exchange.Resources;

namespace Chartreuse.Today.Exchange.Providers.SyncService
{
    public class ActiveSyncService : IExchangeSyncService
    {
        public const string StatusOk = "1";
        private const string InitialSyncKey = "0";

        private string taskFolderId;
        private string taskFolderName;

        public const int DefaultTaskFolderType = 7;

        public string TaskFolderId
        {
            get { return this.taskFolderId; }
        }

        public string TaskFolderName
        {
            get { return this.taskFolderName; }
        }

        public async Task<ExchangeAuthorizationResult> LoginAsync(ExchangeConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException("connectionInfo");

            ActiveSyncServer server;

            // reset cached task folder id
            this.taskFolderId = null;

            if (connectionInfo.ServerUri == null)
            {
                LogService.Log("ActiveSyncService", "Server is empty, trying to auto discover...");
                server = CreateExchangeServer(connectionInfo);

                string discoveredUri = await server.GetAutoDiscoveredServer();
                if (string.IsNullOrEmpty(discoveredUri))
                {
                    LogService.Log("ActiveSyncService", "Didn't found a server for the email address");
                    // Wrong credentials passed to the server
                    return new ExchangeAuthorizationResult
                    {
                        AuthorizationStatus = ExchangeAuthorizationStatus.UserCredentialsInvalid,
                        IsOperationSuccess = false,
                        ErrorMessage = ExchangeResources.ExchangeActiveSync_InvalidCredentialsMessage
                    };
                }
                connectionInfo.ServerUri = SafeUri.Get(discoveredUri);
                LogService.LogFormat("ActiveSyncService", "Server autodiscovered at '{0}'",discoveredUri);
            }

            // Try to get folders information to know if user is correct
            server = CreateExchangeServer(connectionInfo);
            FolderSyncCommandResult result;

            try
            {
                result = await server.FolderSync("0");
                connectionInfo.PolicyKey = server.PolicyKey;
            }
            catch (Exception ex)
            {
                string message = string.Empty + ex.Message;
                if (ex.InnerException != null && ex.Message != null)
                    message += ", " + ex.InnerException.Message;

                return new ExchangeAuthorizationResult
                {
                        AuthorizationStatus = ExchangeAuthorizationStatus.UserCredentialsInvalid,
                        IsOperationSuccess = false,
                        ErrorMessage = message
                    };
            }

            if (result.Status != StatusOk)
            {
                return new ExchangeAuthorizationResult
                {
                    AuthorizationStatus = ExchangeAuthorizationStatus.OK,
                    IsOperationSuccess = false,
                    ErrorMessage = ActiveSyncErrorHelper.GetFolderSyncErrorMessage(result.Status)
                };
            }

            ExchangeFolder taskFolder = null;
            if (string.IsNullOrWhiteSpace(this.taskFolderId))
            {
                taskFolder = result.AddedFolders.FirstOrDefault(f => f.FolderType == DefaultTaskFolderType);
                if (taskFolder == null)
                {
                    LogService.Log("ActiveSyncService", "No default task folder found");

                    return new ExchangeAuthorizationResult
                    {
                        AuthorizationStatus = ExchangeAuthorizationStatus.OK,
                        IsOperationSuccess = false,
                        ErrorMessage = ExchangeResources.ExchangeActiveSync_NoTasksFolder
                    };
                }
            }
            else
            {
                taskFolder = result.AddedFolders.FirstOrDefault(f => f.ServerId == this.taskFolderId);
                if (taskFolder == null)
                {
                    taskFolder = result.AddedFolders.FirstOrDefault(f => f.FolderType == DefaultTaskFolderType);
                    string taskFolderName = taskFolder != null ? taskFolder.DisplayName : "unkown";
                    string message = $"Could not retrieve task folder with id {this.taskFolderId} (default: {taskFolderName}, all: {result.AddedFolders.Select(f => f.DisplayName).AggregateString()})";

                    LogService.LogFormat("ActiveSyncService", message);
                    
                    return new ExchangeAuthorizationResult
                    {
                        AuthorizationStatus = ExchangeAuthorizationStatus.OK,
                        IsOperationSuccess = false,
                        ErrorMessage = message
                    };
                }
                else
                {
                    LogService.LogFormat("ActiveSyncService", "Using task folder: {0}", taskFolder.ServerId);
                }
            }
            
            this.taskFolderId = taskFolder.ServerId;
            this.taskFolderName = taskFolder.DisplayName;

            // all is Ok, update server Uri with the found Uri
            return new ExchangeAuthorizationResult
            {
                AuthorizationStatus = ExchangeAuthorizationStatus.OK,
                IsOperationSuccess = true,
                ServerUri = connectionInfo.ServerUri
            };

        }

        public async Task<ExchangeSyncResult> ExecuteSyncAsync(ExchangeConnectionInfo connectionInfo, ExchangeChangeSet changeSet, string syncState, string folderId)
        {
            return await this.ExecuteSyncAsync(connectionInfo, changeSet, syncState, folderId, false);
        }

        private async Task<ExchangeSyncResult> ExecuteSyncAsync(ExchangeConnectionInfo connectionInfo, ExchangeChangeSet changeSet, string syncState, string folderId, bool isFirstSync)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException("connectionInfo");
            if (changeSet == null)
                throw new ArgumentNullException("changeSet");
            if (string.IsNullOrEmpty(syncState))
                throw new ArgumentNullException("syncState");

            if (string.IsNullOrEmpty(folderId) || connectionInfo.ServerUri == null || string.IsNullOrWhiteSpace(connectionInfo.ServerUri.ToString()))
            {
                ExchangeSyncResult loginResult = await this.EnsureLoginAsync(folderId, connectionInfo);
                if (loginResult != null)
                    return loginResult;
            }
            else
            {
                this.taskFolderId = folderId;                
            }

            ActiveSyncServer server = CreateExchangeServer(connectionInfo);

            ExchangeSyncResult returnValue = new ExchangeSyncResult
            {
                SyncState = syncState, 
                ChangeSet = new ExchangeChangeSet()
            };

            bool mustSync = true;

            while (mustSync)
            {
                SyncCommandResult result = await server.Sync(returnValue.SyncState, this.taskFolderId, changeSet);
                if (result.Status != StatusOk)
                {
                    returnValue.AuthorizationResult = this.GetFailedAuthResult("Sync", result);
                    mustSync = false;
                }
                else
                {
                    returnValue.AuthorizationResult = new ExchangeAuthorizationResult
                    {
                            AuthorizationStatus = ExchangeAuthorizationStatus.OK,
                            IsOperationSuccess = true,
                            ServerUri = connectionInfo.ServerUri
                        };

                    if (result.SyncKey != null)
                        returnValue.SyncState = result.SyncKey;

                    connectionInfo.PolicyKey = server.PolicyKey;

                    // If we don't have any syncstate (nothing has changed) we return the old one
                    returnValue.OperationResult.IsOperationSuccess = true;
                    
                    returnValue.ChangeSet.AddedTasks.AddRange(result.AddedTasks);
                    returnValue.ChangeSet.ModifiedTasks.AddRange(result.ModifiedTasks);
                    returnValue.ChangeSet.DeletedTasks.AddRange(result.DeletedTasks);
                    returnValue.TaskAddedCount += result.ServerAddedTasks;
                    returnValue.TaskEditedCount += result.ServerModifiedTasks;
                    
                    foreach (var map in result.ClientServerMapIds)
                        returnValue.AddMap(map.Key, map.Value);

                    returnValue.TaskDeletedCount += changeSet.DeletedTasks.Count;
                    mustSync = result.MoreAvailable;
                    
                    changeSet = new ExchangeChangeSet(); // changeSet has been pushed to server, reset it !
                }
            }
            return returnValue;
        }
        
        public async Task<ExchangeSyncResult> ExecuteFirstSyncAsync(ExchangeConnectionInfo connectionInfo, ExchangeChangeSet changeSet)
        {
            ExchangeSyncResult loginResult = await this.EnsureLoginAsync(this.taskFolderId, connectionInfo);
            if (loginResult != null)
                return loginResult;

            ActiveSyncServer server = CreateExchangeServer(connectionInfo);
           
            // Start sync with sync state = 0
            // In this case we only obtain a non ero syncKey to use with future queries
            SyncCommandResult syncCommandResult = await server.Sync(InitialSyncKey, this.taskFolderId, new ExchangeChangeSet());

            connectionInfo.PolicyKey = server.PolicyKey;

            if (syncCommandResult.Status != StatusOk)
            {
                var result = new ExchangeSyncResult
                {
                    AuthorizationResult = this.GetFailedAuthResult("ExecuteFirstSync", syncCommandResult)
                };

                return result;
            }

            // As it is first sync, we have just picked a new SyncId
            // We have to re-sync with this new SyncId
            return await this.ExecuteSyncAsync(connectionInfo, changeSet, syncCommandResult.SyncKey, this.taskFolderId, true);
        }

        private async Task<ExchangeSyncResult> EnsureLoginAsync(string folderId, ExchangeConnectionInfo connectionInfo)
        {
            if (string.IsNullOrEmpty(folderId) || connectionInfo.ServerUri == null)
            {
                var authorizationResult = await this.LoginAsync(connectionInfo);

                if (authorizationResult.AuthorizationStatus != ExchangeAuthorizationStatus.OK || !authorizationResult.IsOperationSuccess || this.taskFolderId == null)
                    return new ExchangeSyncResult { AuthorizationResult = authorizationResult };
            }

            return null;
        }

        private ExchangeAuthorizationResult GetFailedAuthResult(string command, SyncCommandResult syncResult)
        {
            return new ExchangeAuthorizationResult
            {
                AuthorizationStatus = ExchangeAuthorizationStatus.OK,
                IsOperationSuccess = false,
                Status = syncResult.Status,
                ErrorMessage = $"command: {command} status: {ActiveSyncErrorHelper.GetSyncErrorMessage(syncResult.Status)}"
            };
        }

        public void CancelCurrentOperation()
        {
            // we don't manage cancellation
        }

        public void ResetCache()
        {
        }

        private static ActiveSyncServer CreateExchangeServer(ExchangeConnectionInfo connectionInfo)
        {
            return new ActiveSyncServer(
                connectionInfo.ServerUri == null ? string.Empty : connectionInfo.ServerUri.OriginalString,
                connectionInfo.Email,
                connectionInfo.Password,
                connectionInfo.DeviceId,
                connectionInfo.PolicyKey);
        }
    }
}

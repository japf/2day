using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Chartreuse.Today.Exchange.Resources;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange.Providers
{
    public abstract class ExchangeSynchronizationProviderBase<TSyncService> : SynchronizationProviderBase where TSyncService : IExchangeSyncService
    {
        public const string CacheSyncState = "Exchange_SyncState";
        public const string CacheTaskFolderId = "Exchange_TaskFolderId";

        private readonly TSyncService syncService;

        private bool isCanceled;

        private string SyncState
        {
            get { return this.GetCachedString(CacheSyncState); }
            set { this.Metadata.ProviderDatas[CacheSyncState] = value; }
        }

        private string FolderId
        {
            get { return this.GetCachedString(CacheTaskFolderId); }
            set { this.Metadata.ProviderDatas[CacheTaskFolderId] = value; }
        }

        protected IExchangeSyncService SyncService
        {
            get { return this.syncService; }
        }

        public override bool CanDeleteAccount
        {
            get { return false; }
        }

        public TSyncService ExchangeService
        {
            get { return this.syncService; }
        }

        protected ExchangeSynchronizationProviderBase(ISynchronizationManager synchronizationManager, TSyncService syncService, ICryptoService crypto)
            : base(synchronizationManager, crypto)
        {
            if (syncService == null)
                throw new ArgumentNullException(nameof(syncService));

            this.syncService = syncService;
        }

        public override void SetStartDate(DateTime? due, DateTime? start, Action<DateTime?> setDue)
        {
            // when start date is set, due date is set to the same value if it's null
            if (!due.HasValue)
                setDue(start);
            else if (start.HasValue && due.Value < start.Value)
                setDue(start);
        }

        public override void SetDueDate(DateTime? due, DateTime? start, Action<DateTime?> setStart)
        {
            // when due date is removed, start date is removed too
            if (!due.HasValue)
                setStart(null);
            // when due date is set and start is after due date, update start date
            else if (start.HasValue && due.Value < start.Value)
                setStart(due);
        }

        protected abstract ExchangeInfoBuild BuildExchangeConnectionInfo();

        protected abstract void ClearSettings();

        protected abstract void UpdateSettingsAfterSync(ExchangeConnectionInfo connectionInfo, string serverUri);

        public override async Task<bool> CheckLoginAsync()
        {
            ExchangeInfoBuild check = this.BuildExchangeConnectionInfo();
            if (check.IsValid)
            {
                try
                {
                    Uri originalServerUri = check.ConnectionInfo.ServerUri;
                    var authResult = await this.SyncService.LoginAsync(check.ConnectionInfo);

                    if (authResult.IsOperationSuccess && authResult.AuthorizationStatus == ExchangeAuthorizationStatus.OK)
                    {
                        // update the server uri as it might have changed
                        this.UpdateSettingsAfterSync(check.ConnectionInfo, authResult.ServerUri.ToString());
                        return true;
                    }
                    else
                    {
                        // if if the auth result is not a succes, we might update server uri
                        if (originalServerUri == null && check.ConnectionInfo.ServerUri != null)
                            this.UpdateSettingsAfterSync(check.ConnectionInfo, authResult.ServerUri.ToString());

                        // if the status is "autodisover not found" but we have a server uri, that means credentials are invalid
                        var authorizationStatus = authResult.AuthorizationStatus;
                        if (authorizationStatus == ExchangeAuthorizationStatus.AutodiscoveryServiceNotFound && check.ConnectionInfo.ServerUri != null)
                            authorizationStatus = ExchangeAuthorizationStatus.UserCredentialsInvalid;

                        string message = authorizationStatus.ToReadableString();
                        if (!string.IsNullOrWhiteSpace(authResult.ErrorMessage))
                            message += string.Format(",{0}", authResult.ErrorMessage);
                        
                        this.Manager.TrackEvent("Login failed " + this.Service, message);
                        this.OnSynchronizationFailed(string.Format(ExchangeResources.Exchange_SyncErrorFormat, message));
                    
                        return false;
                    }
                }
                catch (Exception e)
                {
                    this.Manager.TrackEvent("Login failed " + this.Service, e.Message);
                    this.OnSynchronizationFailed(string.Format(ExchangeResources.Exchange_ExceptionFormat, e.Message));

                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override async Task<bool> SyncAsync()
        {
            this.isCanceled = false;

            var exchangeInfo = this.BuildExchangeConnectionInfo();
            if (!exchangeInfo.IsValid)
                return false;

            this.Changes.Reset();

            try
            {
                if (string.IsNullOrEmpty(this.SyncState))
                    await this.FirstSync(exchangeInfo);
                else
                    await this.NormalSync(exchangeInfo);

                this.UpdateSettingsAfterSync(exchangeInfo.ConnectionInfo, exchangeInfo.ConnectionInfo.ServerUri.ToString());
            }
            catch (CommunicationObjectAbortedException)
            {
                // this can occurs if the user cancel the operation
                // convert to an OperationCanceledException which is caught by the SynchronizationManager
                throw new OperationCanceledException();
            }

            return true;
        }
       
        private async Task FirstSync(ExchangeInfoBuild exchangeInfo)
        {
            this.OnSynchronizationProgressChanged(StringResources.SyncProgress_SyncInProgress);

            var changeSet = new ExchangeChangeSet();

            // start by sending an empty change set to get all existing task on the service
            LogService.LogFormat("ExchangeSync", "Performing first sync, step 1, sending empty changeset");
            var result = await this.SyncService.ExecuteFirstSyncAsync(exchangeInfo.ConnectionInfo, changeSet);

            var completedTasks = result.ChangeSet.AddedTasks.Where(t => t.Completed.HasValue).ToList();
            if (completedTasks.Count > 100)
            {
                // takes only 100 completed tasks, prevent bringing more in 2Day to prevent weird issue like having 3k tasks in the app
                var tasks = new List<ExchangeTask>();
                tasks.AddRange(result.ChangeSet.AddedTasks.Where(t => !t.Completed.HasValue));          // add all non completed tasks
                tasks.AddRange(result.ChangeSet.AddedTasks.Where(t => t.Completed.HasValue).Take(100)); // add first 100 completed tasks

                result.ChangeSet.AddedTasks.Clear();
                result.ChangeSet.AddedTasks.AddRange(tasks);
            }

            this.ProcessSyncResult(changeSet, result, sendCompletedNotification: false);
            this.OnSynchronizationProgressChanged(StringResources.SyncProgress_UpdatingTasks);

            // send a changeset for all tasks that does not have a sync id
            changeSet.AddedTasks.AddRange(this.Workbook.Tasks.Where(t => !t.IsCompleted && string.IsNullOrEmpty(t.SyncId)).Select(t => t.ToExchangeTask(setCategory: true, properties: TaskProperties.All)));
            LogService.LogFormat("ExchangeSync", "Performing first sync, step 2, sending changeset with {0} added tasks", changeSet.AddedTasks.Count);

            if (changeSet.AddedTasks.Count > 0)
            {
                result = await this.SyncService.ExecuteFirstSyncAsync(exchangeInfo.ConnectionInfo, changeSet);
                this.ProcessSyncResult(changeSet, result);
            }
            else
            {
                this.OnSynchronizationCompleted("Exchange");                
            }

            this.FolderId = this.SyncService.TaskFolderId;
        }

        private async Task NormalSync(ExchangeInfoBuild exchangeInfo)
        {
            this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingTasks);

            var changeSet = this.BuildChangeSetFromMetadata();

            var result = await this.SyncService.ExecuteSyncAsync(exchangeInfo.ConnectionInfo, changeSet, this.SyncState, this.FolderId);

            this.OnSynchronizationProgressChanged(StringResources.SyncProgress_UpdatingTasks);

            this.ProcessSyncResult(changeSet, result);
        }

        private ExchangeChangeSet BuildChangeSetFromMetadata()
        {
            var changeSet = new ExchangeChangeSet();

            foreach (var id in this.Metadata.AddedTasks)
            {
                var task = this.Workbook.Tasks.FirstOrDefault(t => t.Id == id);
                if (task != null)
                {
                    changeSet.AddedTasks.Add(this.CreateExchangeTask(task, TaskProperties.All));
                }
            }

            foreach (var deletedEntry in this.Metadata.DeletedTasks)
            {
                if (!string.IsNullOrEmpty(deletedEntry.SyncId))
                    changeSet.DeletedTasks.Add(new ServerDeletedAsset(deletedEntry.SyncId));
            }

            foreach (var editedEntry in this.Metadata.EditedTasks)
            {
                var task = this.Workbook.Tasks.FirstOrDefault(t => t.Id == editedEntry.Key);
                if (task != null)
                {
                    // the task is marked edited, make sure it has a sync id
                    if (!string.IsNullOrEmpty(task.SyncId))
                    {
                        changeSet.ModifiedTasks.Add(this.CreateExchangeTask(task, editedEntry.Value));
                    }
                    else
                    {
                        // otherwise, process it has an "added" task
                        changeSet.AddedTasks.Add(this.CreateExchangeTask(task, TaskProperties.All));
                    }

                }
            }

            LogService.LogFormat("ExchangeSync", "Performing sync, sending changeset with {0} added tasks {1} deleted tasks and {2} edited tasks",
                changeSet.AddedTasks.Count, changeSet.DeletedTasks.Count, changeSet.ModifiedTasks.Count);

                return changeSet;
        }

        private ExchangeTask CreateExchangeTask(ITask task, TaskProperties taskProperties)
        {
            return task.ToExchangeTask(setCategory: !this.IsInDefaultFolder(task), properties: taskProperties);
        }

        private void ProcessSyncResult(ExchangeChangeSet sourceChangeSet, ExchangeSyncResult result, bool sendCompletedNotification = true)
        {
            if (this.isCanceled)
                throw new OperationCanceledException();

            if (result.AuthorizationResult.IsOperationSuccess)
            {
                this.Metadata.Reset();

                var changeSet = result.ChangeSet;

                this.Changes.WebAdd = result.TaskAddedCount;
                this.Changes.WebEdit = result.TaskEditedCount;
                this.Changes.WebDelete = result.TaskDeletedCount;

                // update task sent in the sourceChangeSet
                foreach (var exchangeTask in sourceChangeSet.AddedTasks)
                {
                    // find the task in the workbook
                    var task = this.Workbook.Tasks.FirstOrDefault(t => t.Id == exchangeTask.LocalId);
                    if (task != null)
                    {
                        var map = result.MapId.FirstOrDefault(m => m.LocalId == task.Id);
                        if (map != null)
                        {
                            this.PrepareTaskUpdate(task);

                            task.SyncId = map.ExchangeId;
                        }
                    }
                }

                LogService.LogFormat(
                    "ExchangeSync", "Processing result sent by server changeset with {0} added tasks {1} deleted tasks and {2} edited tasks",
                    changeSet.AddedTasks.Count, changeSet.DeletedTasks.Count, changeSet.ModifiedTasks.Count);

                foreach (var exchangeTask in changeSet.AddedTasks.Where(t => !string.IsNullOrEmpty(t.Subject)))
                {
                    // find the target folder by looking using its name
                    var folder = this.GetFolderForExchangeTask(exchangeTask);

                    var candidate = folder.Tasks.FirstOrDefault(t => t.SyncId == exchangeTask.Id);
                    if (candidate == null)
                    {
                        // check that we don't have already the "same" task
                        candidate = this.FindTask(folder.Tasks, exchangeTask.Subject, exchangeTask.Due);
                        if (candidate == null)
                        {
                            // create the task and add it to its folder
                            var task = exchangeTask.ToTask(this.Workbook);
                            this.AttachTaskToFolder(task, folder);

                            this.Changes.LocalAdd++;
                        }
                        else if (string.IsNullOrEmpty(candidate.SyncId))
                        {
                            candidate.SyncId = exchangeTask.Id;
                        }
                    }
                }

                foreach (var deletedAsset in changeSet.DeletedTasks)
                {
                    var task = this.Workbook.Tasks.FirstOrDefault(t => t.SyncId == deletedAsset.Id);
                    if (task != null)
                    {
                        this.DeleteTask(task);
                        this.Changes.LocalDelete++;
                    }
                }

                foreach (var exchangeTask in changeSet.ModifiedTasks)
                {
                    var task = this.Workbook.Tasks.FirstOrDefault(t => this.HaveSameId(t, exchangeTask));
                    if (task != null)
                    {
                        this.PrepareTaskUpdate(task);
                        task.UpdateFromExchange(exchangeTask);

                        var requiredFolder = this.GetFolderForExchangeTask(exchangeTask);
                        if (task.Folder != requiredFolder)
                            task.Folder = requiredFolder;

                        this.Changes.LocalEdit++;
                    }
                    else
                    {
                        LogService.LogFormat("ExchangeSync", "Task {0} has been modified on the server but cannot be found in the workbook", exchangeTask.Subject);
                    }
                }

                if (result.OperationResult.IsOperationSuccess)
                {
                    this.SyncState = result.SyncState;

                    if (sendCompletedNotification)
                        this.OnSynchronizationCompleted("Exchange");
                }
                else
                {
                    this.OnSynchronizationFailed(string.Format(ExchangeResources.Exchange_SyncErrorFormat, result.OperationResult.ErrorMessage));
                }
            }
            else
            {
                if (result.AuthorizationResult.Status != null && result.AuthorizationResult.Status.Equals("3", StringComparison.OrdinalIgnoreCase))
                {
                    this.OnSynchronizationFailed(StringResources.Exchange_InvalidSyncKeyWorkaround);
                }
                else
                {
                    this.OnSynchronizationFailed(string.Format(ExchangeResources.Exchange_AuthProblemFormat,result.AuthorizationResult.AuthorizationStatus, result.AuthorizationResult.ErrorMessage));
                }
            }
        }

        protected virtual bool HaveSameId(ITask task, ExchangeTask exchangeTask)
        {
            return task.SyncId == exchangeTask.Id;
        }

        private IFolder GetFolderForExchangeTask(ExchangeTask exchangeTask)
        {
            IFolder folder = null;

            if (!string.IsNullOrEmpty(exchangeTask.Category))
            {
                // check a folder match the name
                folder = this.Workbook.Folders.FirstOrDefault(f => f.Name.Equals(exchangeTask.Category, StringComparison.OrdinalIgnoreCase));
                if (folder == null)
                {
                    // no folder found and the task in exchange has a category
                    // create a folder with the name of the category
                    folder = this.CreateFolder(exchangeTask.Category);
                }
            }
            else
            {
                // the task has no folder, use the default one
                folder = this.GetDefaultFolder();
            }

            return folder;
        }

        public override void Reset(bool clearSettings)
        {
            this.SyncState = null;

            if (clearSettings)
            {
                if (this.syncService != null)
                    this.syncService.ResetCache();

                this.ClearSettings();
            }
        }

        public override void Cancel()
        {
            this.SyncService.CancelCurrentOperation();
            this.isCanceled = true;
        }

        protected struct ExchangeInfoBuild
        {
            public bool IsValid { get; set; }
            public ExchangeConnectionInfo ConnectionInfo { get; set; }
        }
    }
}

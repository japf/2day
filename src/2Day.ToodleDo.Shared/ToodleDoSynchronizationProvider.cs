using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Security;
using Chartreuse.Today.ToodleDo.Shared;
using Chartreuse.Today.ToodleDo.Shared.Resources;

namespace Chartreuse.Today.ToodleDo
{
    public class ToodleDoSynchronizationProvider : SynchronizationProviderBase
    {
        #region private fields

        public const string ServerUrl = "http://api.toodledo.com/2";
        public const string Server = "http://www.toodledo.com";

        public const string AppId = "2dowp7";
        public const string AppToken = "api4dd10c860b878";

        private const string CacheToken = "ToodleDo_Token";
        private const string CacheKey = "ToodleDo_Key";
        private const string CacheUserId = "ToodleDo_UserId";
        private const string CacheFolderEditTimeStamp = "ToodleDo_LastFolderEdit";
        private const string CacheContextEditTimeStamp = "ToodleDo_LastContextEdit";
        private const string CacheTaskEditTimeStamp = "ToodleDo_LastTasksEdit";
        private const string CacheTaskDeleteTimeStamp = "ToodleDo_LastTasksDelete";
        private const string CacheTokenTimeStamp = "ToodleDo_TokenTimestamp";
        private const double TokenValidityHours = 4.0;

        private readonly ToodleDoService service;

        private string email;
        private byte[] password;
        private ToodleDoAccount account;

        private string token
        {
            get { return this.GetCachedString(CacheToken); }
            set { this.Metadata.ProviderDatas[CacheToken] = value; }
        }

        private string key
        {
            get { return this.GetCachedString(CacheKey); }
            set { this.Metadata.ProviderDatas[CacheKey] = value; }
        }

        private string userId
        {
            get { return this.GetCachedString(CacheUserId); }
            set { this.Metadata.ProviderDatas[CacheUserId] = value; }
        }

        private int folderEditTimestamp
        {
            get { return this.GetCachedInt(CacheFolderEditTimeStamp); }
            set { this.Metadata.ProviderDatas[CacheFolderEditTimeStamp] = value.ToString(CultureInfo.InvariantCulture); }
        }

        private int contextEditTimestamp
        {
            get { return this.GetCachedInt(CacheContextEditTimeStamp); }
            set { this.Metadata.ProviderDatas[CacheContextEditTimeStamp] = value.ToString(CultureInfo.InvariantCulture); }
        }

        private int taskEditTimestamp
        {
            get { return this.GetCachedInt(CacheTaskEditTimeStamp); }
            set { this.Metadata.ProviderDatas[CacheTaskEditTimeStamp] = value.ToString(CultureInfo.InvariantCulture); }
        }

        private int taskDeleteTimestamp
        {
            get { return this.GetCachedInt(CacheTaskDeleteTimeStamp); }
            set { this.Metadata.ProviderDatas[CacheTaskDeleteTimeStamp] = value.ToString(CultureInfo.InvariantCulture); }
        }

        private long tokenTimestamp
        {
            get { return this.GetCachedLong(CacheTokenTimeStamp); }
            set { this.Metadata.ProviderDatas[CacheTokenTimeStamp] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public ToodleDoService ToodleDoService
        {
            get { return this.service; }
        }

        internal static bool FetchNonCompletedAtFirstSync
        {
            get; set;
        }

        #endregion

        public override string DefaultFolderName
        {
            get { return "ToodleDo"; }
        }

        public override string LoginInfo
        {
            get
            {
                return this.email;
            }
        }

        public override string ServerInfo
        {
            get
            {
                return Server;
            }
        }

        public override string Name
        {
            get { return ToodleDoResources.ToodleDo_ProviderName; }
        }

        public override string Headline
        {
            get { return ToodleDoResources.ToodleDo_Headline; }
        }

        public override string Description
        {
            get { return ToodleDoResources.ToodleDo_Description; }
        }

        public override SynchronizationService Service
        {
            get { return SynchronizationService.ToodleDo; }
        }

        public override SyncFeatures SupportedFeatures
        {
            get
            {
                return SyncFeatures.Title |
                       SyncFeatures.DueDate |
                       SyncFeatures.Priority |
                       SyncFeatures.Folder |
                       //SyncFeatures.Reminders         |
                       //SyncFeatures.Colors         |
                       //SyncFeatures.Icons          |
                       SyncFeatures.Recurrence |
                       SyncFeatures.Notes |
                       SyncFeatures.Context |
                       SyncFeatures.StartDate;
            }
        }

        public override bool CanDeleteAccount
        {
            get { return false; }
        }

        static ToodleDoSynchronizationProvider()
        {
            FetchNonCompletedAtFirstSync = true;
        }

        public ToodleDoSynchronizationProvider(ISynchronizationManager synchronizationManager, ICryptoService cryptoService)
            : base(synchronizationManager, cryptoService)
        {
            this.service = new ToodleDoService(() => this.key, AppId);
            this.service.OnWebException += (s, e) => this.OnSynchronizationFailed(string.Format(ToodleDoResources.ToodleDo_SyncErrorFormat, e.Item.Message));

            this.email = this.Workbook.Settings.GetValue<string>(ToodleDoSettings.ToodleDoLogin);
            this.password = this.Workbook.Settings.GetValue<byte[]>(ToodleDoSettings.ToodleDoPassword);
        }

        public override async Task<bool> SyncAsync()
        {
            this.OnSynchronizationProgressChanged(ToodleDoResources.ToodleDo_LoginInProgress);

            this.userId = await this.GetUserId();

            // returns null means sync failed because of empty email or password
            if (string.IsNullOrEmpty(this.userId))
                return false;

            // check if we a token which is still valid
            DateTime tokendate = new DateTime(this.tokenTimestamp);
            var timespan = DateTime.Now - tokendate;
            bool hasTokenExpired = timespan.TotalHours >= TokenValidityHours;

            if (string.IsNullOrEmpty(this.token) || hasTokenExpired)
            {
                await this.UpdateSyncTokenAsync();
            }

            int tryCount = 0;

            retry:

            var accountResponse = await this.service.GetAccountInfo();
            if (accountResponse.HasError)
            {
                if (accountResponse.Error.Equals("invalid key", StringComparison.OrdinalIgnoreCase) && tryCount == 0)
                {
                    // try again 
                    tryCount++;

                    await this.UpdateSyncTokenAsync();

                    // yes, I'm using a goto :D
                    goto retry;
                }

                this.OnSynchronizationFailed(accountResponse.Error);
            }
            else
            {
                this.account = accountResponse.Data;

                this.Changes.Reset();
                
                await this.StartSynchronization();

                this.OnSynchronizationCompleted("ToodleDo");
            }

            return true;
        }

        private async Task UpdateSyncTokenAsync()
        {
            string sig = MD.Encrypt(this.userId + AppToken);
            var response = await this.service.GetToken(this.userId, sig);

            if (response.HasError)
            {
                this.OnSynchronizationFailed(response.Error);
            }
            else
            {
                this.tokenTimestamp = DateTime.Now.Ticks;
                this.token = response.Data;

                this.key = MD.Encrypt(MD.Encrypt(this.CryptoService.Decrypt(this.password)) + AppToken + this.token);
            }
        }

        public override async Task<bool> CheckLoginAsync()
        {
            try
            {
                var id = await this.GetUserId();
                return !string.IsNullOrEmpty(id);
            }
            catch (Exception ex)
            {
                this.Manager.TrackEvent("Login failed " + this.Service, ex.Message);
                return false;
            }
        }

        public override void Reset(bool clearSettings)
        {
            if (clearSettings)
            {
                this.token = null;

                this.Workbook.Settings.SetValue<string>(ToodleDoSettings.ToodleDoLogin, null);
                this.Workbook.Settings.SetValue<byte[]>(ToodleDoSettings.ToodleDoPassword, null);
            }
        }

        public override void Cancel()
        {
            this.service.Cancel();
        }

        protected override void OnSettingsKeyChanged(string key)
        {
            // when a settings changes, check if we must invalidate the current session
            if (key == ToodleDoSettings.ToodleDoLogin)
            {
                this.email = this.Workbook.Settings.GetValue<string>(ToodleDoSettings.ToodleDoLogin);
                this.InvalidateCurrentSession();
            }
            else if (key == ToodleDoSettings.ToodleDoPassword)
            {
                this.password = this.Workbook.Settings.GetValue<byte[]>(ToodleDoSettings.ToodleDoPassword);

                this.InvalidateCurrentSession();
            }
        }

        private async Task<string> GetUserId()
        {
            if (string.IsNullOrEmpty(this.email))
            {
                this.OnSynchronizationFailed(ToodleDoResources.ToodleDo_EmptyUsername);
                return null;
            }

            if (this.password == null || this.password.Length == 0)
            {
                this.OnSynchronizationFailed(ToodleDoResources.ToodleDo_EmptyPassword);
                return null;
            }

            string decryptedPassword = this.CryptoService.Decrypt(this.password);
            if (string.IsNullOrEmpty(decryptedPassword))
            {
                // password exist but cannot be decrypted (because device changed for example)
                // remove this password and signal is it empty
                this.Workbook.Settings.SetValue<byte[]>(ToodleDoSettings.ToodleDoPassword, null);
                this.OnSynchronizationFailed(ToodleDoResources.ToodleDo_EmptyPassword);
                return null;
            }

            if (string.IsNullOrEmpty(this.userId))
            {
                string sig = MD.Encrypt(this.email + AppToken);
                var response = await this.service.GetUserId(sig, this.email, this.CryptoService.Decrypt(this.password));

                if (response.HasError)
                    this.OnSynchronizationFailed(response.Error);
                else
                    return response.Data;
            }

            return this.userId;
        }

        private async Task StartSynchronization()
        {
            if (this.Metadata.LastSync == DateTime.MinValue)
            {
                // first synchronization
                await this.FirstFolderSync();
                await this.FirstContextSync();
                await this.FirstTasksSync();
            }
            else
            {
                await this.FolderSync();
                await this.ContextSync();
                await this.TaskSync();
            }
            // If there was any update during sync,
            // get the last timestamp from ToodleDo to avoid re-syncing tasks next time
            if (this.Changes.WebAdd > 0 || this.Changes.WebEdit > 0 || this.Changes.WebDelete > 0)
            {
                // Get account info
                ToodleDoResponse<ToodleDoAccount> response = await this.service.GetAccountInfo();
                if (response.HasError)
                {
                    // Nothing to do, no error has to be thrown as sync ias already done
                    // Some tasks will be uselessly sync'ed next time
                }
                else
                {
                    var account = response.Data;
                    this.taskEditTimestamp = account.TaskEditTimestamp;
                    this.taskDeleteTimestamp = account.TaskDeleteTimestamp;
                    this.folderEditTimestamp = account.FolderEditTimestamp;
                    this.contextEditTimestamp = account.ContextEditTimestamp;
                }
            }
        }

        #region folders

        private async Task FirstFolderSync()
        {
            this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingFolders);

            var folderResult = await this.service.GetFolders();

            if (folderResult.HasError)
            {
                this.OnSynchronizationFailed(string.Format(ToodleDoResources.ToodleDo_SyncErrorFormat, folderResult.Error));
            }
            else
            {
                var toodleFolders = folderResult.Data;

                // check if we must push local folders in ToodleDo);
                foreach (var folder in this.Workbook.Folders)
                {
                    var toodleFolder = toodleFolders.FirstOrDefault(f => f.Id == folder.SyncId);
                    if (toodleFolder == null)
                    {
                        // check if a folder with the same name exists in ToodleDo
                        toodleFolder = this.FindFolder(toodleFolders, folder);
                        if (toodleFolder != null)
                        {
                            // a folder with the same name exists in ToodleDo, update the sync id of the local folder 
                            folder.SyncId = toodleFolder.Id;
                        }
                        else
                        {
                            this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingFolderFormat, folder.Name));

                            // this local folder does not exists in ToodleDo, create it
                            string id = await this.service.AddFolder(folder.Name);
                            folder.SyncId = id;
                            this.Changes.WebAdd++;
                        }
                    }
                }

                this.EnsureWorkbookHasFolder(toodleFolders);
            }
        }

        private async Task FolderSync()
        {
            // add new local folders in ToodleDo
            // use ToList() to have a copy of the list as each statement in the loop will change the collection
            foreach (var id in this.Metadata.AddedFolders.ToList())
            {
                IFolder folder = this.Workbook[id];
                if (folder != null)
                {
                    this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingFolderFormat, folder.Name));

                    string syncId = await this.service.AddFolder(folder.Name);
                    folder.SyncId = syncId;

                    this.Changes.WebAdd++;
                }

                this.Metadata.AddedFolders.Remove(id);
            }

            // remove deleted local folders from ToodleDo
            this.UpdateDeletedFolders();
            foreach (var deletedEntry in this.Metadata.DeletedFolders.ToList())
            {
                if (!string.IsNullOrEmpty(deletedEntry.SyncId))
                {
                    this.OnSynchronizationProgressChanged(StringResources.SyncProgress_DeletingFolder);
                    await this.service.DeleteFolder(deletedEntry.SyncId);

                    this.Changes.WebDelete++;
                }

                this.Metadata.DeletedFolders.Remove(deletedEntry);
            }

            // is LastFolderEdit newer than the last sync
            if (this.account.FolderEditTimestamp > this.folderEditTimestamp)
            {
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingFolders);

                // there are probably changes in ToodleDo we must perform in the workbook...
                // start by fetching folders 
                var folderResult = await this.service.GetFolders();
                if (folderResult.HasError)
                {
                    this.OnSynchronizationFailed(string.Format(ToodleDoResources.ToodleDo_SyncErrorFormat, folderResult.Error));
                }
                else
                {
                    var toodleFolders = folderResult.Data;

                    // does the server have folders we don't have ?
                    this.EnsureWorkbookHasFolder(toodleFolders);

                    // does the server misses folders that we have);
                    foreach (var folder in this.Workbook.Folders.ToList())
                    {
                        var toodleFolder = toodleFolders.FirstOrDefault(f => f.Id == folder.SyncId);
                        int id = -1;
                        bool hasId = int.TryParse(folder.SyncId, out id);

                        if (toodleFolder == null && hasId && id > 0 && !string.Equals(this.DefaultFolderName, folder.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (folder.Tasks.Any())
                            {
                                // move all the tasks to the default folder
                                IFolder defaultFolder = this.GetDefaultFolder();
                                foreach (var task in folder.Tasks.ToList())
                                    task.Folder = defaultFolder;
                            }

                            // this folder is no longer in ToodleDo, delete it from the workbook
                            this.DeleteFolder(folder.Name);

                            this.Changes.LocalDelete++;
                        }
                    }

                    foreach (var folder in this.Workbook.Folders)
                    {
                        // does a folder exist in both place
                        var toodleFolder = toodleFolders.FirstOrDefault(f => f.Id == folder.SyncId);

                        // if the name is not the same locally and in ToodleDo
                        if (!this.Metadata.EditedFolders.Contains(folder.Id) && toodleFolder != null
                            && !folder.Name.Equals(toodleFolder.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            // ToodleDo has the latest value
                            folder.Name = toodleFolder.Name;
                        }
                    }
                }
            }

            // do we need to edit folders
            foreach (var folderId in this.Metadata.EditedFolders.ToList())
            {
                var folder = this.Workbook[folderId];
                if (folder != null)
                {
                    this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_UpdatingFolderFormat, folder.Name));

                    await this.service.UpdateFolder(folder.SyncId, folder.Name);

                    this.Changes.WebEdit++;
                }

                this.Metadata.EditedFolders.Remove(folderId);
            }
        }

        private void EnsureWorkbookHasFolder(IEnumerable<ToodleDoFolder> toodleFolders)
        {
            foreach (var toodleFolder in toodleFolders)
            {
                var folder = this.Workbook.Folders.FirstOrDefault(f => f.SyncId == toodleFolder.Id);
                if (folder == null)
                {
                    // no folder has the requested SyncId
                    // check if a folder has the same name
                    folder = this.FindFolder(this.Workbook.Folders, toodleFolder);
                    if (folder != null)
                    {
                        // symply update the SyncId
                        folder.SyncId = toodleFolder.Id;
                    }
                    else
                    {
                        this.CreateFolder(toodleFolder.Name, toodleFolder.Id);

                        this.Changes.LocalAdd++;
                    }
                }
            }
        }

        #endregion

        #region contexts

        private async Task FirstContextSync()
        {
            this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingContexts);

            var contextResult = await this.service.GetContexts();

            if (contextResult.HasError)
            {
                this.OnSynchronizationFailed(string.Format(ToodleDoResources.ToodleDo_SyncErrorFormat, contextResult.Error));
            }
            else
            {
                var toodleDoContexts = contextResult.Data;

                // check if we must push local contexts in ToodleDo);
                foreach (var context in this.Workbook.Contexts)
                {
                    var toodleDoContext = toodleDoContexts.FirstOrDefault(f => f.Id == context.SyncId);
                    if (toodleDoContext == null)
                    {
                        // check if a context with the same name exists in ToodleDo
                        toodleDoContext = this.FindContext(toodleDoContexts, context);
                        if (toodleDoContext != null)
                        {
                            // a context with the same name exists in ToodleDo, update the sync id of the local folder 
                            context.SyncId = toodleDoContext.Id;
                        }
                        else
                        {
                            this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingContextFormat, context.Name));

                            // this local context does not exists in ToodleDo, create it
                            string id = await this.service.AddContext(context.Name);
                            context.SyncId = id;
                            this.Changes.WebAdd = this.Changes.WebAdd;
                        }
                    }
                }

                this.EnsureWorkbookHasContext(toodleDoContexts);
            }
        }

        private async Task ContextSync()
        {
            // add new local contexts in ToodleDo
            // use ToList() to have a copy of the list as each statement in the loop will change the collection
            foreach (var id in this.Metadata.AddedContexts.ToList())
            {
                IContext context = this.Workbook.Contexts.FirstOrDefault(c => c.Id == id);

                // we should never have a folder in the PendingAdded list that does not exist
                // but who knows...
                if (context != null)
                {
                    this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingContextFormat, context.Name));

                    string syncId = await this.service.AddContext(context.Name);
                    context.SyncId = syncId;

                    this.Changes.WebAdd++;
                }

                this.Metadata.AddedContexts.Remove(id);
            }

            // remove deleted local contexts from ToodleDo
            this.UpdateDeletedContexts();
            foreach (var deletedEntry in this.Metadata.DeletedContexts.ToList())
            {
                if (!string.IsNullOrEmpty(deletedEntry.SyncId))
                {
                    this.OnSynchronizationProgressChanged(StringResources.SyncProgress_DeletingContext);
                    await this.service.DeleteContext(deletedEntry.SyncId);

                    this.Changes.WebDelete++;
                }

                this.Metadata.DeletedContexts.Remove(deletedEntry);
            }

            // is LastFolderEdit newer than the last sync
            if (this.account.ContextEditTimestamp > this.contextEditTimestamp)
            {
                // we don't provide name here for string format so we just remove it to prevent displaying " {0}"
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_UpdatingContextFormat.Replace(" {0}", string.Empty));

                // there are probably changes in ToodleDo we must perform in the workbook...
                // start by fetching contexts 
                var contextResult = await this.service.GetContexts();
                if (contextResult.HasError)
                {
                    this.OnSynchronizationFailed(string.Format(ToodleDoResources.ToodleDo_SyncErrorFormat, contextResult.Error));
                }
                else
                {
                    var toodleDoContexts = contextResult.Data;

                    // does the server have contexts we don't have ?
                    this.EnsureWorkbookHasContext(toodleDoContexts);

                    // does the server misses contexts that we have);
                    foreach (var context in this.Workbook.Contexts.ToList())
                    {
                        var toodleDoContext = toodleDoContexts.FirstOrDefault(f => f.Id == context.SyncId);
                        int id;
                        bool hasId = int.TryParse(context.SyncId, out id);

                        if (toodleDoContext == null && hasId && id > 0)
                        {
                            // this context is no longer in ToodleDo, delete it from the workbook
                            this.DeleteContext(context.Name);

                            this.Changes.LocalDelete++;
                        }
                    }

                    foreach (var context in this.Workbook.Contexts)
                    {
                        // does a context exist in both place
                        var toodleDoContext = toodleDoContexts.FirstOrDefault(f => f.Id == context.SyncId);

                        // if the name is not the same locally and in ToodleDo
                        if (!this.Metadata.EditedContexts.Contains(context.Id) && toodleDoContext != null
                            && !context.Name.Equals(toodleDoContext.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            // ToodleDo has the latest value
                            context.Name = toodleDoContext.Name;
                        }
                    }
                }
            }

            // do we need to edit contexts
            foreach (var contextId in this.Metadata.EditedContexts.ToList())
            {
                IContext context = this.Workbook.Contexts.FirstOrDefault(c => c.Id == contextId);
                if (context != null)
                {
                    this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_UpdatingContextFormat, context.Name));

                    await this.service.UpdateContext(context.SyncId, context.Name);

                    this.Changes.WebEdit++;
                    this.Metadata.EditedContexts.Remove(contextId);
                }
            }
        }

        private void EnsureWorkbookHasContext(IEnumerable<ToodleDoContext> toodleDoContexts)
        {
            foreach (var toodleDoContext in toodleDoContexts)
            {
                var context = this.Workbook.Contexts.FirstOrDefault(f => f.SyncId == toodleDoContext.Id);
                if (context == null)
                {
                    // no folder has the requested SyncId
                    // check if a folder has the same name
                    context = this.FindContext(this.Workbook.Contexts, toodleDoContext);
                    if (context != null)
                    {
                        // symply update the SyncId
                        context.SyncId = toodleDoContext.Id;
                    }
                    else
                    {
                        this.CreateContext(toodleDoContext.Name, toodleDoContext.Id);

                        this.Changes.LocalAdd++;
                    }
                }
            }
        }

        #endregion

        #region tasks

        private async Task FirstTasksSync()
        {
            this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingTasks);

            // default of FetchNonCompletedAtFirstSync is true so that we don't fetch huge list of completed tasks
            // for users that keep all their completed tasks in ToodleDo
            // this option is configurable throught an internal property for testing purpose
            // in sync integration tests, we need to set this option to false
            var toodleTasks = await this.service.GetTasks(FetchNonCompletedAtFirstSync);
            var tasks = SortWithSubtasksAfter(this.Workbook.Tasks);
            foreach (var task in tasks)
            {
                var toodleTask = toodleTasks.FirstOrDefault(t => t.Id == task.SyncId);
                if (toodleTask == null)
                {
                    // check if a task with the same name exists in ToodleDo
                    toodleTask = this.FindTask(toodleTasks, task);
                    if (toodleTask != null)
                    {
                        this.PrepareTaskUpdate(task);

                        // a task with the same name exists in ToodleDo, update the sync id of the local task 
                        task.SyncId = toodleTask.Id;
                    }
                    else
                    {
                        this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingTaskFormat, task.Title));

                        // this local task does not exists in ToodleDo, create it
                        var id = await this.service.AddTask(new ToodleDoTask(task));
                        if (!string.IsNullOrEmpty(id))
                        {
                            this.PrepareTaskUpdate(task);

                            task.SyncId = id;
                            this.Changes.WebAdd++;
                        }
                    }
                }
            }

            this.EnsureWorkbookHasTasks(toodleTasks, true);
        }

        private async Task TaskSync()
        {
            // add new local tasks in ToodleDo
            int count = this.Metadata.AddedTasks.Count;
            var addedTasks = new List<ITask>(count);
            foreach (var addedTaskId in this.Metadata.AddedTasks.ToList())
            {
                var task = this.Workbook.Tasks.FirstOrDefault(t => t.Id == addedTaskId);
                if (task != null)
                {
                    addedTasks.Add(task);
                }
                this.Metadata.AddedTasks.Remove(addedTaskId);
            }

            foreach (var task in addedTasks)
            {
                this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingTaskFormat, task.Title));

                string taskId = await this.service.AddTask(new ToodleDoTask(task));
                if (!string.IsNullOrEmpty(taskId))
                {
                    this.PrepareTaskUpdate(task);

                    task.SyncId = taskId;
                    this.Changes.WebAdd++;
                }
            }

            // remove deleted local tasks from ToodleDo
            if (this.Metadata.DeletedTasks.Count > 0)
            {
                var message = StringResources.SyncProgress_DeletingTaskFormat.Replace(" {0}...", string.Empty);
                this.OnSynchronizationProgressChanged(message);

                var tasks = this.Metadata.DeletedTasks
                    .Where(d => !string.IsNullOrEmpty(d.SyncId))
                    .Select(deletedEntry => deletedEntry.SyncId)
                    .ToList();

                if (tasks.Count > 0)
                {
                    await this.service.DeleteTasks(tasks);

                    this.Changes.WebDelete += tasks.Count;
                    this.Metadata.DeletedTasks.Clear();
                }
            }

            // if TaskEditTimestamp newer than the last sync
            // it means we have edited task in ToodleDo since the last sync
            if (this.account.TaskEditTimestamp > this.taskEditTimestamp)
            {
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingTasks);

                // there are probably changed in ToodleDo we must do locally... start by fetching tasks
                // ask the server to give us all the task that have changed since the last sync
                var toodleTasks = await this.service.GetTasks(false, this.taskEditTimestamp);

                // does the server have tasks we don't have ?
                this.EnsureWorkbookHasTasks(toodleTasks, true);

                // does tasks exist in both place, resolve conflicts and update
                await this.UpdateWorkbookTasks(toodleTasks);
            }

            // is TaskDeleteTimestamp newer than the last sync
            if (this.account.TaskDeleteTimestamp > this.taskDeleteTimestamp)
            {
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_UpdatingTasks);

                var deletedTasks = await this.service.GetDeletedTasks(this.taskDeleteTimestamp);

                foreach (string syncId in deletedTasks)
                {
                    var task = this.Workbook.Tasks.FirstOrDefault(t => t.SyncId == syncId);
                    if (task != null)
                    {
                        this.DeleteTask(task);
                        this.Changes.LocalDelete++;
                    }
                }
            }

            // do we need to edit tasks
            foreach (var kvp in this.Metadata.EditedTasks.ToList())
            {
                ITask task = this.Workbook.Tasks.FirstOrDefault(t => t.Id == kvp.Key);

                if (task != null)
                {
                    TaskProperties changes = kvp.Value;

                    this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_UpdatingTaskFormat, task.Title));

                    var result = await this.service.UpdateTask(task.SyncId, new ToodleDoTask(task), changes);
                    if (result)
                    {
                        this.Changes.WebEdit++;
                        this.Metadata.EditedTasks.Remove(kvp.Key);
                    }
                }
            }

            this.RemoveDefaultFolderIfNeeded();
        }

        private void EnsureWorkbookHasTasks(List<ToodleDoTask> toodleTasks, bool takeDuplicate = false)
        {
            foreach (var toodleTask in toodleTasks)
            {
                // check if a task has the right sync id
                var task = this.Workbook.Tasks.FirstOrDefault(t => t.SyncId == toodleTask.Id);
                if (task == null)
                {
                    // check if an existing task has the same title and could be use instead of creating a new one
                    // find its associated folder
                    IFolder folder;
                    int folderId = -1;

                    if (int.TryParse(toodleTask.FolderId, out folderId) && folderId > 0)
                    {
                        // the task is attached to a folder in ToodleDo
                        folder = this.Workbook.Folders.FirstOrDefault(f => f.SyncId == toodleTask.FolderId);
                        if (folder == null)
                            folder = this.GetDefaultFolder();
                    }
                    else
                    {
                        // the task has no folder in ToodleDo, use the default folder
                        folder = this.GetDefaultFolder();
                    }

                    var candidate = this.FindTask(folder.Tasks, toodleTask);
                    if (candidate != null && !takeDuplicate)
                    {
                        // a task with the same title already exist in the workbook
                    }
                    else
                    {
                        var newTask = this.GetTaskFromToodleDo(toodleTask);
                        
                        // no need to do anything with the variable because settings the Folder property
                        // register the task to the folder...
                        this.AttachTaskToFolder(newTask, folder);

                        var taskContext = this.Workbook.Contexts.FirstOrDefault(c => c.SyncId == toodleTask.ContextId);
                        if (taskContext != null)
                            newTask.Context = taskContext;

                        this.Changes.LocalAdd++;
                    }
                }
            }
        }

        private async Task UpdateWorkbookTasks(List<ToodleDoTask> toodleDoTasks)
        {
            foreach (var toodleTask in toodleDoTasks)
            {
                var task = this.Workbook.Tasks.FirstOrDefault(t => t.SyncId == toodleTask.Id);
                if (task != null)
                {
                    if (IsEquivalentTo(toodleTask, task))
                    {
                        LogService.LogFormat("ToodleDoSync", "ToodleDo task {0} is equivalent to workbook task {1}, ignore changes", toodleTask.Title, task.Title);
                        continue;
                    }

                    LogService.LogFormat("ToodleDoSync", "Task {0} is different in the workbook and in ToodleDo (workbook: {1} toodledo: {2})", task.Title, task.Modified, toodleTask.Modified);

                    bool useToodleInformation = false;

                    if (!this.Metadata.EditedTasks.ContainsKey(task.Id))
                    {
                        // if the task is not in the pending edited tasks in the metadata, it has been modified
                        // only in ToodlEdo, so we are sure to take this version
                        useToodleInformation = true;
                    }
                    else
                    {
                        // the task has been modified in ToodlEdo and in 2Day
                        // compare modification dates to use the appropriate version
                        useToodleInformation = (task.Modified < toodleTask.Modified);
                    }

                    if (useToodleInformation)
                    {
                        LogService.LogFormat("ToodleDoSync", "Updating local task from ToodleDo");

                        this.PrepareTaskUpdate(task);

                        // do not call GetDefaultFolder because it creates it if needed, and the method might not always need it
                        toodleTask.UpdateTask(task, this.Workbook, () => this.GetDefaultFolder());

                        this.Changes.LocalEdit++;
                    }
                    else
                    {
                        LogService.LogFormat("ToodleDoSync", "Updating ToodleDo task from workbook");
                        var result = await this.service.UpdateTask(toodleTask.Id, new ToodleDoTask(task), TaskProperties.All);
                        if (result)
                        {
                            this.Changes.WebEdit++;
                        }
                    }
                    // Task has just been synchronized
                    this.Metadata.EditedTasks.Remove(task.Id);
                }
                else
                {
                    LogService.LogFormat("ToodleDoSync", "Error while updating workbook tasks, could not find task with ToodleDo ID: {0} and title: {1}", toodleTask.Id, toodleTask.Title);
                }
            }
        }

        private static bool IsEquivalentTo(ToodleDoTask toodleDoTask, ITask task)
        {
            bool match = toodleDoTask.Title.Equals(task.Title, StringComparison.CurrentCultureIgnoreCase)
                   && (
                        (toodleDoTask.Note == null && task.Note == null) ||
                        (toodleDoTask.Note != null && task.Note != null && toodleDoTask.Note.Equals(task.Note, StringComparison.CurrentCultureIgnoreCase))
                       )
                   && toodleDoTask.EnumPriority == task.Priority
                   && toodleDoTask.Due == task.Due
                   && toodleDoTask.Start == task.Start
                   && toodleDoTask.Tags == task.Tags
                   && toodleDoTask.Completed == task.Completed
                   && toodleDoTask.FolderId == task.Folder.SyncId
                   && ((task.Context == null && (toodleDoTask.ContextId == string.Empty || toodleDoTask.ContextId == "0")) || (task.Context != null && toodleDoTask.ContextId == task.Context.SyncId))
                   && toodleDoTask.RepeatFrom == (task.UseFixedDate ? 0 : 1)
                   && ToodleDoRecurrencyHelpers.Get2DayRecurrency(toodleDoTask.Repeat).Equals(task.CustomFrequency);

            if (!match)
                return false;
            
            if (toodleDoTask.ParentId == null && task.ParentId == null)
                return true;

            if (toodleDoTask.ParentId != null && task.ParentId == null)
                return false;

            if (toodleDoTask.ParentId == null && task.ParentId != null)
                return false;

            // check parent
            if (toodleDoTask.ParentId != null && task.ParentId != null)
            {
                var parentTask = task.Folder.Tasks.FirstOrDefault(t => t.Id == task.ParentId);
                return parentTask != null && toodleDoTask.ParentId == parentTask.SyncId;
            }

            return true;
        }

        #endregion

        private ITask GetTaskFromToodleDo(ToodleDoTask source)
        {
            var task = this.Workbook.CreateTask();

            task.Title = source.Title;
            task.Note = source.Note;
            task.Tags = source.Tags;
            task.Added = source.Added;
            task.Priority = source.EnumPriority;
            task.Due = source.Due;
            task.Start = source.Start;
            task.Completed = source.Completed;
            task.SyncId = source.Id;
            task.Modified = source.Modified;
            task.UseFixedDate = (source.RepeatFrom == 0);
            task.CustomFrequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency(source.Repeat);

            if (!string.IsNullOrWhiteSpace(source.ParentId))
            {
                var parent = this.Workbook.Tasks.FirstOrDefault(t => t.SyncId == source.ParentId);
                if (parent != null)
                    parent.AddChild(task);
            }

            return task;
        }

        private void InvalidateCurrentSession()
        {
            this.userId = null;
            this.token = null;
            this.key = null;
        }
    }
}

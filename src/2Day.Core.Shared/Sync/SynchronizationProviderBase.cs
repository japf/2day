using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Logging;

namespace Chartreuse.Today.Core.Shared.Sync
{
    public abstract class SynchronizationProviderBase : ISynchronizationProvider
    {
        private readonly ISynchronizationManager synchronizationManager;
        private readonly ICryptoService cryptoService;
        private readonly IWorkbook workbook;
        private readonly SynchronizationChanges changes;

        private const string CacheDefaultFolderId = "Sync_DefaultFolderId";

        protected IWorkbook Workbook
        {
            get { return this.workbook; }
        }

        protected SynchronizationChanges Changes
        {
            get { return this.changes; }
        }

        protected ISynchronizationManager Manager
        {
            get { return this.synchronizationManager; }
        }

        protected ISynchronizationMetadata Metadata
        {
            get { return this.synchronizationManager.Metadata; }
        }

        protected ICryptoService CryptoService
        {
            get { return this.cryptoService; }
        }

        public abstract string DefaultFolderName
        {
            get;
        }

        public abstract string LoginInfo
        {
            get;
        }

        public abstract string ServerInfo
        {
            get;
        }

        public virtual string FolderInfo
        {
            get { return null; }
        }

        public abstract string Name
        {
            get;
        }

        public abstract string Headline
        {
            get;
        }

        public abstract string Description
        {
            get;
        }

        public abstract bool CanDeleteAccount
        {
            get;
        }

        public abstract SynchronizationService Service
        {
            get;
        }

        public abstract SyncFeatures SupportedFeatures
        {
            get;
        }

        public event EventHandler<EventArgs<string>> OperationCompleted;
        public event EventHandler<EventArgs<string>> OperationFailed;
        public event EventHandler<EventArgs<string>> OperationProgressChanged;

        protected SynchronizationProviderBase(ISynchronizationManager synchronizationManager, ICryptoService cryptoService)
        {
            if (synchronizationManager == null)
                throw new ArgumentNullException("synchronizationManager");
            if (cryptoService == null)
                throw new ArgumentNullException("cryptoService");

            this.synchronizationManager = synchronizationManager;
            this.cryptoService = cryptoService;
            this.workbook = synchronizationManager.Workbook;
            this.changes = new SynchronizationChanges();

            this.Workbook.Settings.KeyChanged += this.OnSettingsKeyChanged;
        }

        public virtual Task PrepareAsync()
        {
            return Task.FromResult(0);
        }

        public abstract Task<bool> SyncAsync();

        public virtual Task<string> DeleteAccountAync()
        {
            return new Task<string>(() => "not supported");
        }

        public abstract Task<bool> CheckLoginAsync();

        public abstract void Reset(bool clearSettings);

        public abstract void Cancel();

        public virtual void SetDueDate(DateTime? due, DateTime? start, Action<DateTime?> setStart)
        {
        }

        public virtual void SetStartDate(DateTime? due, DateTime? start, Action<DateTime?> setDue)
        {
        }

        protected IFolder CreateFolder(string name, string syncId = null)
        {
            this.Metadata.IgnoreFolder(name);
            return this.Workbook.AddFolder(name, syncId);
        }

        protected IContext CreateContext(string name, string syncId = null)
        {
            this.Metadata.IgnoreContext(name);
            return this.Workbook.AddContext(name, syncId);
        }

        protected ISmartView CreateSmartView(string name, string rules, string syncId = null)
        {
            this.Metadata.IgnoreSmartView(name);
            var smartView = this.Workbook.AddSmartView(name, rules);
            smartView.SyncId = syncId;

            return smartView;
        }

        /// <summary>
        /// Update the <see cref="ISynchronizationMetadata"/>.DeletedFolders list to remove all entry
        /// that have a null, empty or whitespace sync id
        /// or have a sync id that currently exists in the list of folders of the workbook
        /// </summary>
        protected void UpdateDeletedFolders()
        {
            // update the deleted folder list to remove deleted folder that were just added
            // use case for this is
            // * delete a existing folder
            // * create a folder with the exame same name
            // => DeletedFolders count is 1, AddedFolders count is 1
            // => we start by adding the folder, but it already exists in the remote service so the previous id is keept
            // => we do NOT want to remove this folder that is in the DeletedFolders list in that case
            foreach (var deletedEntry in this.Metadata.DeletedFolders.ToList())
            {
                if (string.IsNullOrEmpty(deletedEntry.SyncId) || this.Workbook.Folders.Any(f => f.SyncId != null && f.SyncId.Equals(deletedEntry.SyncId, StringComparison.OrdinalIgnoreCase)))
                    this.Metadata.DeletedFolders.Remove(deletedEntry);
            }
        }

        /// <summary>
        /// Update the <see cref="ISynchronizationMetadata"/>.DeletedContexts list to remove all entry
        /// that have a null, empty or whitespace sync id
        /// or have a sync id that currently exists in the list of contexts of the workbook
        /// </summary>
        protected void UpdateDeletedContexts()
        {
            // see comment below
            foreach (var deletedEntry in this.Metadata.DeletedContexts.ToList())
            {
                if (string.IsNullOrEmpty(deletedEntry.SyncId) || this.Workbook.Contexts.Any(f => f.SyncId != null && f.SyncId.Equals(deletedEntry.SyncId, StringComparison.OrdinalIgnoreCase)))
                    this.Metadata.DeletedContexts.Remove(deletedEntry);
            }
        }

        /// <summary>
        /// Update the <see cref="ISynchronizationMetadata"/>.DeletedSmartViews list to remove all entry
        /// that have a null, empty or whitespace sync id
        /// or have a sync id that currently exists in the list of smart views of the workbook
        /// </summary>
        protected void UpdateDeletedSmartViews()
        {
            // see comment below
            foreach (var deletedEntry in this.Metadata.DeletedSmartViews.ToList())
            {
                if (string.IsNullOrEmpty(deletedEntry.SyncId) || this.Workbook.SmartViews.Any(f => f.SyncId != null && f.SyncId.Equals(deletedEntry.SyncId, StringComparison.OrdinalIgnoreCase)))
                    this.Metadata.DeletedSmartViews.Remove(deletedEntry);
            }
        }

        protected void AttachTaskToFolder(ITask task, IFolder folder)
        {
            this.Metadata.IgnoreTask(task);

            task.Folder = folder;
        }

        protected void PrepareTaskUpdate(ITask task)
        {
            this.Metadata.IgnoreTask(task);
        }

        protected void DeleteTask(ITask task)
        {
            if (task.IsBeingEdited)
            {
                // the task is currently being edited by the user, we cannot delete it
                // mark the task as "added" so that during next sync it will be pushed to the server
                this.Metadata.TaskAdded(task);
            }
            else
            {
                this.Metadata.IgnoreTask(task);
                task.Delete();    
            }
        }

        protected void DeleteFolder(string folder)
        {
            this.Metadata.IgnoreFolder(folder);

            this.Workbook.RemoveFolder(folder);
        }

        protected void DeleteContext(string context)
        {
            this.Metadata.IgnoreContext(context);

            this.Workbook.RemoveContext(context);
        }

        protected void DeleteSmartView(string smartview)
        {
            this.Metadata.IgnoreSmartView(smartview);

            this.Workbook.RemoveSmartView(smartview);
        }

        protected IFolder GetDefaultFolder(bool createIfNeeded = true)
        {
            IFolder folder = this.Workbook.Folders.FirstOrDefault(f => f.Name.Equals(this.DefaultFolderName, StringComparison.CurrentCultureIgnoreCase));

            if (folder == null && createIfNeeded)
            {
                folder = this.CreateFolder(this.DefaultFolderName);
                this.Metadata.ProviderDatas[CacheDefaultFolderId] = folder.Id.ToString();

                this.workbook.SetupRemoteSyncFolder(folder);
            }

            return folder;
        }

        protected void RemoveDefaultFolderIfNeeded()
        {
            // make sure the default folder is not empty, otherwise, delete it
            var defaultFolder = this.GetDefaultFolder(createIfNeeded: false);
            if (defaultFolder != null && defaultFolder.TaskCount == 0)
                this.Workbook.RemoveFolder(defaultFolder.Name);
        }

        protected TSyncFolder FindFolder<TSyncFolder>(IEnumerable<TSyncFolder> syncFolders, IFolder folder) where TSyncFolder : ISyncItem
        {
            return syncFolders.FirstOrDefault(f => f.Name.Trim().Equals(folder.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        protected IFolder FindFolder(IEnumerable<IFolder> folders, ISyncItem syncFolder)
        {
            return folders.FirstOrDefault(f => f.Name.Trim().Equals(syncFolder.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        protected TSyncContext FindContext<TSyncContext>(IEnumerable<TSyncContext> syncContexts, IContext context) where TSyncContext : ISyncItem
        {
            return syncContexts.FirstOrDefault(c => c.Name.Trim().Equals(context.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        protected IContext FindContext(IEnumerable<IContext> contexts, ISyncItem syncContext)
        {
            return contexts.FirstOrDefault(c => c.Name.Trim().Equals(syncContext.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        protected TSyncTask FindTask<TSyncTask>(IEnumerable<TSyncTask> syncTasks, ITask task) where TSyncTask : ISyncTask
        {
            List<TSyncTask> candidates = new List<TSyncTask>();
            foreach (var syncTask in syncTasks)
            {
                bool nameMatches = task.Title.Trim().Equals(syncTask.Name.Trim(), StringComparison.OrdinalIgnoreCase);
                bool dueMatches;
                if (syncTask.Due != null)
                    dueMatches = task.Due.HasValue && task.Due.Value.Date.ToString("d") == syncTask.Due.Value.Date.ToString("d");
                else
                    dueMatches = !task.Due.HasValue;

                if (nameMatches && dueMatches)
                    candidates.Add(syncTask);
            }

            return candidates.FirstOrDefault(t => this.Workbook.Tasks.All(ta => ta.SyncId != t.Id));
        }

        protected ITask FindTask(IEnumerable<ITask> tasks, ISyncTask syncTask)
        {
            List<ITask> candidates = new List<ITask>();
            foreach (var task in tasks)
            {
                bool nameMatches = task.Title.Trim().Equals(syncTask.Name.Trim(), StringComparison.OrdinalIgnoreCase);

                bool dueMatches;
                if (syncTask.Due != null)
                    dueMatches = task.Due.HasValue && task.Due.Value.Date.ToString("d") == syncTask.Due.Value.Date.ToString("d");
                else
                    dueMatches = !task.Due.HasValue;

                if (nameMatches && dueMatches)
                    candidates.Add(task);
            }

            return candidates.FirstOrDefault(t => string.IsNullOrEmpty(t.SyncId));
        }

        protected ITask FindTask(IEnumerable<ITask> tasks, string name, DateTime? due)
        {
            List<ITask> candidates = new List<ITask>();
            foreach (var task in tasks)
            {
                bool nameMatches = task.Title.Trim().Equals(name.Trim(), StringComparison.OrdinalIgnoreCase);

                bool dueMatches;
                if (due != null)
                    dueMatches = task.Due.HasValue && task.Due.Value.Date.ToString("d") == due.Value.Date.ToString("d");
                else
                    dueMatches = !task.Due.HasValue;

                if (nameMatches && dueMatches)
                    candidates.Add(task);
            }

            return candidates.FirstOrDefault(t => string.IsNullOrEmpty(t.SyncId));
        }

        protected bool IsInDefaultFolder(ITask task)
        {
            if (task.Folder == null)
                return false; // should never happens

            string folderId = task.Folder.Id.ToString();

            return 
                task.Folder.Name.Equals(this.DefaultFolderName, StringComparison.OrdinalIgnoreCase) &&
                this.Metadata.ProviderDatas.ContainsKey(CacheDefaultFolderId) && this.Metadata.ProviderDatas[CacheDefaultFolderId] == folderId;
        }

        protected void OnSynchronizationCompleted(string service)
        {
            string message = string.Format(
                "{0} Local +{1} e{2} -{3} Remote +{4} e{5} -{6}",
                service, this.Changes.LocalAdd, this.Changes.LocalEdit, this.Changes.LocalDelete, this.Changes.WebAdd, this.Changes.WebEdit, this.Changes.WebDelete);
            
            LogService.LogFormat("SyncProvider", "Sync completed ({0})", message);

            this.OperationCompleted.Raise(this, new EventArgs<string>(message));
        }

        protected void OnSynchronizationFailed(string message)
        {
            LogService.LogFormat("SyncProvider", "Sync failed ({0})", message);

            this.OperationFailed.Raise(this, new EventArgs<string>(message));
        }

        protected void OnSynchronizationProgressChanged(string message)
        {
            LogService.LogFormat("SyncProvider", "Sync progress changed: {0}", message);

            this.OperationProgressChanged.Raise(this, new EventArgs<string>(message));
        }

        private void OnSettingsKeyChanged(object sender, SettingsKeyChanged e)
        {
            this.OnSettingsKeyChanged(e.Key);
        }

        protected virtual void OnSettingsKeyChanged(string settingsKey)
        {
        }

        protected string GetCachedString(string id)
        {
            string s;
            this.Metadata.ProviderDatas.TryGetValue(id, out s);
            return s;
        }

        protected int GetCachedInt(string id)
        {
            string s;
            this.Metadata.ProviderDatas.TryGetValue(id, out s);
            if (!string.IsNullOrEmpty(s))
            {
                int t;
                if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out t))
                    return t;
            }

            return 0;
        }

        protected long GetCachedLong(string id)
        {
            string s;
            this.Metadata.ProviderDatas.TryGetValue(id, out s);
            if (!string.IsNullOrEmpty(s))
            {
                long t;
                if (long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out t))
                    return t;
            }

            return 0;
        }

        protected static List<ITask> SortWithSubtasksAfter(IList<ITask> source)
        {
            var result = new List<ITask>(source.Count);
            result.AddRange(source.Where(t => t.ParentId == null));
            result.AddRange(source.Where(t => t.ParentId != null));

            return result;
        }
    }
}

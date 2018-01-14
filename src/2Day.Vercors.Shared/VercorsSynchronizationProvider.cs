using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Vercors.Shared.Model;
using Chartreuse.Today.Vercors.Shared.Resources;

namespace Chartreuse.Today.Vercors.Shared
{
    public class VercorsSynchronizationProvider : SynchronizationProviderBase
    {
        #region private fields

        private const string FolderName = "2Day";
        private readonly IVercorsService service;
        private VercorsAccount account;

        private const string CacheFolderEditTimeStamp = "Vercors_LastFolderEdit";
        private const string CacheContextEditTimeStamp = "Vercors_LastContextEdit";
        private const string CacheSmartViewEditTimeStamp = "Vercors_LastSmartViewEdit";
        private const string CacheTaskEditTimeStamp = "Vercors_LastTasksEdit";
        private const string CacheTaskDeleteTimeStamp = "Vercors_LastTasksDelete";

        private long folderEditTimestamp
        {
            get { return this.GetCachedLong(CacheFolderEditTimeStamp); }
            set { this.Metadata.ProviderDatas[CacheFolderEditTimeStamp] = value.ToString(CultureInfo.InvariantCulture); }
        }

        private long contextEditTimestamp
        {
            get { return this.GetCachedLong(CacheContextEditTimeStamp); }
            set { this.Metadata.ProviderDatas[CacheContextEditTimeStamp] = value.ToString(CultureInfo.InvariantCulture); }
        }

        private long smartViewEditTimestamp
        {
            get { return this.GetCachedLong(CacheSmartViewEditTimeStamp); }
            set { this.Metadata.ProviderDatas[CacheSmartViewEditTimeStamp] = value.ToString(CultureInfo.InvariantCulture); }
        }

        private long taskEditTimestamp
        {
            get { return this.GetCachedLong(CacheTaskEditTimeStamp); }
            set { this.Metadata.ProviderDatas[CacheTaskEditTimeStamp] = value.ToString(CultureInfo.InvariantCulture); }
        }

        private long taskDeleteTimestamp
        {
            get { return this.GetCachedLong(CacheTaskDeleteTimeStamp); }
            set { this.Metadata.ProviderDatas[CacheTaskDeleteTimeStamp] = value.ToString(CultureInfo.InvariantCulture); }
        }

        #endregion

        public override string DefaultFolderName
        {
            get { return FolderName; }
        }

        public override string LoginInfo
        {
            get
            {
                if (!string.IsNullOrEmpty(this.service.LoginInfo))
                    return this.service.LoginInfo;
                else
                    return string.Empty;
            }
        }

        public override string ServerInfo
        {
            get
            {
                return Constants.VercorsServiceUri;
            }
        }

        public override string Name
        {
            get { return VercorsResources.Vercors_ProviderName; }
        }

        public override string Headline
        {
            get { return VercorsResources.Vercors_Headline; }
        }

        public override string Description
        {
            get { return VercorsResources.Vercors_Description; }
        }

        public override SynchronizationService Service
        {
            get { return SynchronizationService.Vercors; }
        }

        public override SyncFeatures SupportedFeatures
        {
            get
            {
                return SyncFeatures.Title |
                       SyncFeatures.DueDate |
                       SyncFeatures.Priority |
                       SyncFeatures.Folder |
                       SyncFeatures.Reminders |
                       SyncFeatures.Colors |
                       SyncFeatures.Icons  |
                       SyncFeatures.Recurrence |
                       SyncFeatures.Notes |
                       SyncFeatures.Context |
                       SyncFeatures.Progress |
                       SyncFeatures.StartDate |
                       SyncFeatures.SmartView;
            }
        }

        public override bool CanDeleteAccount
        {
            get { return true; }
        }

        public IVercorsService VercorsService
        {
            get { return this.service; }
        }

        public VercorsSynchronizationProvider(ISynchronizationManager synchronizationManager, ICryptoService cryptoService, IVercorsService vercorsService) 
            : base(synchronizationManager, cryptoService)
        {
            if (vercorsService == null)
                throw new ArgumentNullException("vercorsService");

            this.service = vercorsService;
        }

        public override async Task PrepareAsync()
        {
            try
            {
                await this.service.LoginAsync(true);
            }
            catch (Exception ex)
            {
                TryTrackEvent(string.Format("LoginAsync exception: {0}", ex));
            }
        }

        public override Task<string> DeleteAccountAync()
        {
            return this.service.DeleteAccount();
        }

        public override async Task<bool> SyncAsync()
        {
            this.OnSynchronizationProgressChanged(StringResources.SyncProgress_SyncInProgress);

            bool result = true;
            Exception exception = null;
            if (string.IsNullOrEmpty(this.service.LoginInfo) || this.service.LoginInfo == Constants.DefaultLoginInfo)
            {
                try
                {
                    result = await this.service.LoginAsync(this.Manager.IsBackground);
                }
                catch (Exception e)
                {
                    exception = e;
                    result = false;
                }
            }

            if (!result)
            {
                string message = VercorsResources.Vercors_UnableToLogin;
                if (exception != null)
                    message += string.Format(" ({0})", exception);

                this.OnSynchronizationFailed(message);
                return false;
            }

            this.account = await this.service.GetUserAccount();

            this.Changes.Reset();

            if (this.account == null)
                this.account = await this.service.AddUserAccount(new VercorsAccount());

            if (this.Metadata.LastSync == DateTime.MinValue)
            {            
                await this.FirstFolderSync();
                await this.FirstContextSync();
                await this.FirstSmartViewSync();
                await this.FirstTasksSync();
            }
            else
            {
                await this.FolderSync();
                await this.ContextSync();
                await this.SmartViewSync();
                await this.TasksSync();
            }

            // If there was any update during sync, 
            // get the last timestamp from Vercors to avoid re-syncing tasks next time
            if (this.Changes.WebAdd > 0 || this.Changes.WebEdit > 0 || this.Changes.WebDelete > 0)
            {
                this.account = await this.service.GetUserAccount();
                if (this.account != null)
                {
                    this.taskEditTimestamp    = this.account.TaskEditTimestamp;
                    this.taskDeleteTimestamp  = this.account.TaskDeleteTimestamp;
                    this.folderEditTimestamp  = this.account.FolderEditTimestamp;
                    this.contextEditTimestamp = this.account.ContextEditTimestamp;
                }
            }

            this.OnSynchronizationCompleted(this.Name);

            return true;
        }

        private static void TryTrackEvent(string message)
        {
            if (Ioc.HasType<ITrackingManager>())
                Ioc.Resolve<ITrackingManager>().Event(TrackingSource.SyncMessage, "Vercors", message);
        }

        #region folders

        private async Task FirstFolderSync()
        {
            this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingFolders);

            var vercorsFolders = await this.service.GetFolders();

            // check if we must push local folders in Vercors
            foreach (var folder in this.Workbook.Folders)
            {
                var vercorsFolder = vercorsFolders.FirstOrDefault(f => f.Id == folder.SyncId);
                if (vercorsFolder == null)
                {
                    // check if a folder with the same name exists in Vercors
                    vercorsFolder = this.FindFolder(vercorsFolders, folder);
                    if (vercorsFolder != null)
                    {
                        // a folder with the same name exists in Vercors, update the sync id of the local folder 
                        folder.SyncId = vercorsFolder.Id;
                    }
                    else
                    {
                        this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingFolderFormat, folder.Name));

                        // this local folder does not exists in Vercors, create it
                        vercorsFolder = await this.service.AddFolder(new VercorsFolder(folder));
                        folder.SyncId = vercorsFolder.Id;
                        this.Changes.WebAdd++;
                    }
                }
            }

            this.EnsureWorkbookHasFolder(vercorsFolders);
        }

        private async Task FolderSync()
        {
            // add new local folders in Vercors
            // use ToList() to have a copy of the list as each statement in the loop will change the collection
            foreach (var id in this.Metadata.AddedFolders.ToList())
            {
                IFolder folder = this.Workbook[id];
                if (folder != null)
                {
                    this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingFolderFormat, folder.Name));

                    VercorsFolder vercorsFolder = await this.service.AddFolder(new VercorsFolder(folder));
                    if (vercorsFolder != null)
                    {
                        folder.SyncId = vercorsFolder.Id;
                        this.Changes.WebAdd++;
                    }
                }

                this.Metadata.AddedFolders.Remove(id);
            }

            // remove deleted local folders from Vercors
            this.UpdateDeletedFolders();
            foreach (var deletedEntry in this.Metadata.DeletedFolders.ToList())
            {
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_DeletingFolder);
                await this.service.DeleteFolder(new VercorsFolder { ItemId = int.Parse(deletedEntry.SyncId) });

                this.Changes.WebDelete++;
                this.Metadata.DeletedFolders.Remove(deletedEntry);
            }

            // is LastFolderEdit newer than the last sync
            if (this.account.FolderEditTimestamp > this.folderEditTimestamp)
            {
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingFolders);

                // there are probably changes in Vercors we must perform in the workbook...
                // start by fetching folders 
                var vercorsFolders = await this.service.GetFolders();

                // does the server have folders we don't have ?
                this.EnsureWorkbookHasFolder(vercorsFolders);

                // does the server misses folders that we have);
                foreach (var folder in this.Workbook.Folders.ToList())
                {
                    var vercorsFolder = vercorsFolders.FirstOrDefault(f => f.Id == folder.SyncId);
                    int id = -1;
                    bool hasId = int.TryParse(folder.SyncId, out id);

                    if (vercorsFolder == null && hasId && id > 0 && !string.Equals(this.DefaultFolderName, folder.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        // move all the tasks to the default folder
                        if (folder.Tasks.Any())
                        {
                            var defaultFolder = this.GetDefaultFolder();
                            foreach (var task in folder.Tasks.ToList())
                                task.Folder = defaultFolder;
                        }

                        // this folder is no longer in Vercors, delete it from the workbook
                        this.DeleteFolder(folder.Name);

                        this.Changes.LocalDelete++;
                    }
                }

                foreach (var folder in this.Workbook.Folders)
                {
                    // does a folder exist in both place
                    var vercorsFolder = vercorsFolders.FirstOrDefault(f => f.Id == folder.SyncId);

                    // if the name is not the same locally and in Vercors
                    if (!this.Metadata.EditedFolders.Contains(folder.Id) && vercorsFolder != null
                        && !folder.Name.Equals(vercorsFolder.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        // Vercors has the latest value
                        vercorsFolder.UpdateTarget(folder);
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

                    await this.service.UpdateFolder(new VercorsFolder(folder));

                    this.Changes.WebEdit++;
                }

                this.Metadata.EditedFolders.Remove(folderId);
            }
        }

        private void EnsureWorkbookHasFolder(IEnumerable<VercorsFolder> vercorsFolders)
        {
            foreach (var vercorsFolder in vercorsFolders)
            {
                var folder = this.Workbook.Folders.FirstOrDefault(f => f.SyncId == vercorsFolder.Id);
                if (folder == null)
                {
                    // no folder has the requested SyncId
                    // check if a folder has the same name
                    folder = this.FindFolder(this.Workbook.Folders, vercorsFolder);
                    if (folder != null)
                    {
                        vercorsFolder.UpdateTarget(folder);
                    }
                    else
                    {
                        folder = this.CreateFolder(vercorsFolder.Name, vercorsFolder.Id);
                        vercorsFolder.UpdateTarget(folder);

                        if (folder.Color == null)
                        {
                            // use a default color (should never happens)
                            folder.Color = ColorChooser.Default;
                        }
                        else
                        {
                            // make sure the color uses the new possible color
                            string newColor = ColorChooser.GetNewColor(folder.Color);
                            if (!folder.Color.Equals(newColor, StringComparison.OrdinalIgnoreCase))
                            {
                                folder.Color = newColor;

                                // flag this folder as edited
                                if (!this.Metadata.EditedFolders.Contains(folder.Id) && !this.Metadata.AddedFolders.Contains(folder.Id))
                                    this.Metadata.EditedFolders.Add(folder.Id);
                            }
                        }

                        if (folder.IconId == 0)
                            folder.IconId = 1;

                        this.Changes.LocalAdd++;
                    }
                }
                else
                {
                    // we have the folder, update it
                    vercorsFolder.UpdateTarget(folder);
                }
            }
        }

        #endregion

        #region contexts

        private async Task FirstContextSync()
        {
            this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingContexts);

            var vercorsContexts = await this.service.GetContexts();

            // check if we must push local contexts in Vercors;
            foreach (var context in this.Workbook.Contexts)
            {
                var vercorsContext = vercorsContexts.FirstOrDefault(f => f.Id == context.SyncId);
                if (vercorsContext == null)
                {
                    // check if a context with the same name exists in Vercors
                    vercorsContext = this.FindContext(vercorsContexts, context);
                    if (vercorsContext != null)
                    {
                        // a context with the same name exists in Vercors, update the sync id of the local folder 
                        context.SyncId = vercorsContext.Id;
                    }
                    else
                    {
                        this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingContextFormat, context.Name));

                        // this local context does not exists in Vercors, create it
                        vercorsContext = await this.service.AddContext(new VercorsContext(context));
                        context.SyncId = vercorsContext.Id;
                        this.Changes.WebAdd++;
                    }
                }
            }

            this.EnsureWorkbookHasContext(vercorsContexts);
        }

        private async Task ContextSync()
        {
            // add new local contexts in Vercors
            // use ToList() to have a copy of the list as each statement in the loop will change the collection
            foreach (var id in this.Metadata.AddedContexts.ToList())
            {
                IContext context = this.Workbook.Contexts.FirstOrDefault(c => c.Id == id);

                // we should never have a folder in the PendingAdded list that does not exist
                // but who knows...
                if (context != null)
                {
                    this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingContextFormat, context.Name));

                    VercorsContext vercorsContext = await this.service.AddContext(new VercorsContext(context));
                    if (vercorsContext != null)
                    {
                        context.SyncId = vercorsContext.Id;
                        this.Changes.WebAdd++;
                    }
                }

                this.Metadata.AddedContexts.Remove(id);
            }

            // remove deleted local contexts from Vercors
            this.UpdateDeletedContexts();            
            foreach (var deletedEntry in this.Metadata.DeletedContexts.ToList())
            {
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_DeletingContext);
                await this.service.DeleteContext(new VercorsContext { ItemId = int.Parse(deletedEntry.SyncId) });

                this.Changes.WebDelete++;
                this.Metadata.DeletedContexts.Remove(deletedEntry);
            }

            // is LastFolderEdit newer than the last sync
            if (this.account.ContextEditTimestamp > this.contextEditTimestamp)
            {
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingContexts);

                // there are probably changes in Vercors we must perform in the workbook...
                // start by fetching contexts 
                var vercorsContexts = await this.service.GetContexts();

                // does the server have contexts we don't have ?
                this.EnsureWorkbookHasContext(vercorsContexts);

                // does the server misses contexts that we have);
                foreach (var context in this.Workbook.Contexts.ToList())
                {
                    var vercorsContext = vercorsContexts.FirstOrDefault(f => f.Id == context.SyncId);
                    int id = -1;
                    bool hasId = int.TryParse(context.SyncId, out id);

                    if (vercorsContext == null && hasId && id > 0)
                    {
                        // this context is no longer in Vercors, delete it from the workbook
                        this.DeleteContext(context.Name);

                        this.Changes.LocalDelete++;
                    }
                }

                foreach (var context in this.Workbook.Contexts)
                {
                    // does a context exist in both place
                    var vercorsContext = vercorsContexts.FirstOrDefault(f => f.Id == context.SyncId);

                    // if the name is not the same locally and in Vercors
                    if (!this.Metadata.EditedContexts.Contains(context.Id) && vercorsContext != null
                        && !context.Name.Equals(vercorsContext.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        // Vercors has the latest value
                        vercorsContext.UpdateTarget(context);
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

                    await this.service.UpdateContext(new VercorsContext(context));

                    this.Changes.WebEdit++;
                    this.Metadata.EditedContexts.Remove(contextId);
                }
            }
        }

        private void EnsureWorkbookHasContext(IEnumerable<VercorsContext> vercorsContexts)
        {
            foreach (var vercorsContext in vercorsContexts)
            {
                var context = this.Workbook.Contexts.FirstOrDefault(f => f.SyncId == vercorsContext.Id);
                if (context == null)
                {
                    // no folder has the requested SyncId
                    // check if a folder has the same name
                    context = this.FindContext(this.Workbook.Contexts, vercorsContext);
                    if (context != null)
                    {
                        // symply update the SyncId
                        context.SyncId = vercorsContext.Id;
                    }
                    else
                    {
                        this.CreateContext(vercorsContext.Name, vercorsContext.Id);

                        this.Changes.LocalAdd++;
                    }
                }
            }
        }

        #endregion

        #region contexts

        private async Task FirstSmartViewSync()
        {
            this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingSmartViews);

            var vercorsSmartViews = await this.service.GetSmartViews();

            // check if we must push local smartViews in Vercors;
            foreach (var smartView in this.Workbook.SmartViews)
            {
                var vercorsSmartView = vercorsSmartViews.FirstOrDefault(f => f.Id == smartView.SyncId);
                if (vercorsSmartView == null)
                {
                    // check if a smartView with the same name exists in Vercors
                    vercorsSmartView = vercorsSmartViews.FirstOrDefault(f => f.Name.Equals(smartView.Name, StringComparison.OrdinalIgnoreCase));
                    if (vercorsSmartView != null)
                    {
                        // a smartView with the same name exists in Vercors, update the sync id of the local folder 
                        smartView.SyncId = vercorsSmartView.Id;
                    }
                    else
                    {
                        this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingSmartViewFormat, smartView.Name));

                        // this local smartView does not exists in Vercors, create it
                        vercorsSmartView = await this.service.AddSmartView(new VercorsSmartView(smartView));
                        smartView.SyncId = vercorsSmartView.Id;
                        this.Changes.WebAdd++;
                    }
                }
            }

            this.EnsureWorkbookHasSmartView(vercorsSmartViews);
        }

        private async Task SmartViewSync()
        {
            // add new local smartViews in Vercors
            // use ToList() to have a copy of the list as each statement in the loop will change the collection
            foreach (var id in this.Metadata.AddedSmartViews.ToList())
            {
                ISmartView smartView = this.Workbook.SmartViews.FirstOrDefault(c => c.Id == id);

                // we should never have a folder in the PendingAdded list that does not exist
                // but who knows...
                if (smartView != null)
                {
                    this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingSmartViewFormat, smartView.Name));

                    VercorsSmartView vercorsSmartView = await this.service.AddSmartView(new VercorsSmartView(smartView));
                    if (vercorsSmartView != null)
                    {
                        smartView.SyncId = vercorsSmartView.Id;
                        this.Changes.WebAdd++;
                    }
                }

                this.Metadata.AddedSmartViews.Remove(id);
            }

            // remove deleted local smartViews from Vercors
            this.UpdateDeletedSmartViews();
            foreach (var deletedEntry in this.Metadata.DeletedSmartViews.ToList())
            {
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_DeletingSmartView);
                await this.service.DeleteSmartView(new VercorsSmartView { ItemId = deletedEntry.SyncId });

                this.Changes.WebDelete++;
                this.Metadata.DeletedSmartViews.Remove(deletedEntry);
            }

            // is LastFolderEdit newer than the last sync
            if (this.account.SmartViewEditTimestamp > this.smartViewEditTimestamp)
            {
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_GettingSmartViews);

                // there are probably changes in Vercors we must perform in the workbook...
                // start by fetching smartViews 
                var vercorsSmartViews = await this.service.GetSmartViews();

                // does the server have smartViews we don't have ?
                this.EnsureWorkbookHasSmartView(vercorsSmartViews);

                // does the server misses smartViews that we have);
                foreach (var smartView in this.Workbook.SmartViews.ToList())
                {
                    var vercorsSmartView = vercorsSmartViews.FirstOrDefault(f => f.Id == smartView.SyncId);
                    bool hasId = !string.IsNullOrWhiteSpace(smartView.SyncId);

                    if (vercorsSmartView == null && hasId)
                    {
                        // this smartView is no longer in Vercors, delete it from the workbook
                        this.DeleteSmartView(smartView.Name);

                        this.Changes.LocalDelete++;
                    }
                }

                foreach (var smartView in this.Workbook.SmartViews)
                {
                    // does a smartView exist in both place
                    var vercorsSmartView = vercorsSmartViews.FirstOrDefault(f => f.Id == smartView.SyncId);

                    // if the name is not the same locally and in Vercors
                    if (!this.Metadata.EditedSmartViews.Contains(smartView.Id) && vercorsSmartView != null
                        && !smartView.Name.Equals(vercorsSmartView.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        // Vercors has the latest value
                        try
                        {
                            vercorsSmartView.UpdateTarget(smartView);
                        }
                        catch (Exception ex)
                        {
                            LogService.Log("VercorsSync", $"Error while updating smart view: {ex}");
                            TrackingManagerHelper.Exception(ex, $"Error while updating smart view: {ex}");
                        }
                    }
                }

                this.smartViewEditTimestamp = this.account.SmartViewEditTimestamp;
            }

            // do we need to edit smartViews
            foreach (var smartViewId in this.Metadata.EditedSmartViews.ToList())
            {
                ISmartView smartView = this.Workbook.SmartViews.FirstOrDefault(c => c.Id == smartViewId);
                if (smartView != null)
                {
                    this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_UpdatingSmartViewFormat, smartView.Name));

                    await this.service.UpdateSmartView(new VercorsSmartView(smartView));

                    this.Changes.WebEdit++;
                    this.Metadata.EditedSmartViews.Remove(smartViewId);
                }
            }
        }

        private void EnsureWorkbookHasSmartView(IEnumerable<VercorsSmartView> vercorsSmartViews)
        {
            foreach (var vercorsSmartView in vercorsSmartViews)
            {
                var smartView = this.Workbook.SmartViews.FirstOrDefault(f => f.SyncId == vercorsSmartView.Id);
                if (smartView == null)
                {
                    // no folder has the requested SyncId
                    // check if a folder has the same name
                    smartView = this.Workbook.SmartViews.FirstOrDefault(f => f.Name.Equals(vercorsSmartView.Name, StringComparison.OrdinalIgnoreCase));
                    if (smartView != null)
                    {
                        // symply update the SyncId
                        smartView.SyncId = vercorsSmartView.Id;
                    }
                    else
                    {
                        try
                        {
                            this.CreateSmartView(vercorsSmartView.Name, vercorsSmartView.Rules, vercorsSmartView.Id);
                        }
                        catch (Exception ex)
                        {
                            LogService.Log("VercorsSync", $"Error while adding smart view: {ex}");
                            TrackingManagerHelper.Exception(ex, $"Error while adding smart view: {ex}");
                        }

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

            var vercorsTasks = await this.service.GetTasks();
            int count = this.Workbook.Tasks.Count;
            int index = 0;
            var tasks = SortWithSubtasksAfter(this.Workbook.Tasks);
            foreach (var task in tasks)
            {
                var vercorsTask = vercorsTasks.FirstOrDefault(t => t.Id == task.SyncId);
                if (vercorsTask == null)
                {
                    // check if a task with the same name exists in Vercors
                    vercorsTask = this.FindTask(vercorsTasks, task);
                    if (vercorsTask != null)
                    {
                        this.PrepareTaskUpdate(task);

                        // a task with the same name exists in Vercors, update the sync id of the local task 
                        task.SyncId = vercorsTask.Id;
                    }
                    else
                    {
                        this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingTaskFormat, FormatPercentage(index, count)));

                        // this local task does not exists in Vercors, create it
                        vercorsTask = await this.service.AddTask(new VercorsTask(this.Workbook, task));
                        if (vercorsTask != null)
                        {
                            this.PrepareTaskUpdate(task);

                            task.SyncId = vercorsTask.Id;
                            this.Changes.WebAdd++;
                        }
                    }
                }

                index++;
            }

            this.EnsureWorkbookHasTasks(vercorsTasks, true);
        }

        private async Task TasksSync()
        {
            // add new local tasks in Vercors
            int count = this.Metadata.AddedTasks.Count;
            int index = 0;

            // get the list of ITask from the list of their ids
            var addedTasks = new List<ITask>(count);
            foreach (var addedTaskId in this.Metadata.AddedTasks.ToList())
            {
                var task = this.Workbook.Tasks.FirstOrDefault(t => t.Id == addedTaskId);
                if (task != null)
                {
                    addedTasks.Add(task);
                }
                this.Metadata.AddedTasks.Remove(addedTaskId);
                index++;
            }
            // sort to have subtasks after
            addedTasks = SortWithSubtasksAfter(addedTasks);

            foreach (var task in addedTasks)
            {
                this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_AddingTaskFormat, FormatPercentage(index, count)));

                var vercorsTask = await this.service.AddTask(new VercorsTask(this.Workbook, task));
                if (vercorsTask != null)
                {
                    this.PrepareTaskUpdate(task);

                    task.SyncId = vercorsTask.Id;
                    this.Changes.WebAdd++;
                }
            }

            // remove deleted local tasks from Vercors
            if (this.Metadata.DeletedTasks.Count > 0)
            {
                var message = StringResources.SyncProgress_DeletingTaskFormat.Replace(" {0}...", string.Empty);
                this.OnSynchronizationProgressChanged(message);

                var tasks = this.Metadata.DeletedTasks
                    .Where(d => !string.IsNullOrEmpty(d.SyncId))
                    .Select(deletedEntry => new VercorsTask {ItemId = int.Parse(deletedEntry.SyncId)})
                    .ToList();

                if (tasks.Count > 0)
                {
                    var result = await this.service.DeleteTasks(tasks);
                    if (result != null)
                    {
                        foreach (var item in result)
                        {
                            var deleted = this.Metadata.DeletedTasks.FirstOrDefault(t => t.SyncId == item.Id);
                            if (!string.IsNullOrEmpty(deleted.SyncId))
                            {
                                this.Changes.WebDelete++;
                                this.Metadata.DeletedTasks.Remove(deleted);
                            }
                        }
                    }
                }
            }
            
            // if TaskEditTimestamp newer than the last sync
            // it means we have edited task in Vercors since the last sync
            if (this.account.TaskEditTimestamp > this.taskEditTimestamp)
            {
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_UpdatingTasks);

                // there are probably changed in Vercors we must do locally... start by fetching tasks
                // ask the server to give us all the task that have changed since the last sync
                var vercorsTasks = await this.service.GetTasks(this.taskEditTimestamp);

                // does the server have tasks we don't have ?
                this.EnsureWorkbookHasTasks(vercorsTasks, true);

                // does tasks exist in both place, resolve conflicts and update
                await this.UpdateWorkbookTasks(vercorsTasks);
            }

            // is TaskDeleteTimestamp newer than the last sync
            if (this.account.TaskDeleteTimestamp > this.taskDeleteTimestamp)
            {
                this.OnSynchronizationProgressChanged(StringResources.SyncProgress_UpdatingTasks);

                var deletedTasks = await this.service.GetDeletedTasks(this.taskDeleteTimestamp);

                foreach (var deletedTask in deletedTasks)
                {
                    var task = this.Workbook.Tasks.FirstOrDefault(t => t.SyncId == deletedTask.DeletedId.ToString());
                    if (task != null)
                    {
                        this.DeleteTask(task);
                        this.Changes.LocalDelete++;
                    }
                }
            }

            // do we need to edit tasks
            count = this.Metadata.EditedTasks.Count;
            index = 0;
            foreach (var kvp in this.Metadata.EditedTasks.ToList())
            {
                ITask task = this.Workbook.Tasks.FirstOrDefault(t => t.Id == kvp.Key);

                if (task != null)
                {
                    this.OnSynchronizationProgressChanged(string.Format(StringResources.SyncProgress_UpdatingTaskFormat, FormatPercentage(index, count)));

                    var vercorsTask = await this.service.UpdateTask(new VercorsTask(this.Workbook, task));
                    if (vercorsTask != null)
                    {
                        this.Changes.WebEdit++;
                        this.Metadata.EditedTasks.Remove(kvp.Key);
                    }
                }
                index++;
            }

            this.RemoveDefaultFolderIfNeeded();
        }

        private void EnsureWorkbookHasTasks(List<VercorsTask> vercorsTasks, bool takeDuplicate = false)
        {
            foreach (var vercorsTask in vercorsTasks)
            {
                // check if a task has the right sync id
                var task = this.Workbook.Tasks.FirstOrDefault(t => t.SyncId == vercorsTask.Id);
                if (task == null)
                {
                    // check if an existing task has the same title and could be use instead of creating a new one
                    // find its associated folder
                    IFolder folder;
      
                    if (vercorsTask.FolderId > 0)
                    {
                        // the task is attached to a folder in Vercors
                        folder = this.Workbook.Folders.FirstOrDefault(f => f.SyncId == vercorsTask.FolderId.ToString());
                        if (folder == null)
                            folder = this.GetDefaultFolder();
                    }
                    else
                    {
                        // the task has no folder in Vercors, use the default folder
                        folder = this.GetDefaultFolder();
                    }

                    var candidate = this.FindTask(folder.Tasks, vercorsTask);                    
                    if (candidate != null && !takeDuplicate)
                    {
                        // a task with the same title already exist in the workbook
                    }
                    else
                    {
                        var newTask = this.Workbook.CreateTask();

                        // we are going to add a new task
                        // flag it so that it gets ignored from the changes metadata list
                        this.Metadata.IgnoreTask(newTask);
                        
                        vercorsTask.UpdateTarget(newTask, this.Workbook, () => this.GetDefaultFolder());

                        var taskContext = this.Workbook.Contexts.FirstOrDefault(c => c.SyncId == vercorsTask.ContextId.ToString());
                        if (taskContext != null)
                            newTask.Context = taskContext;

                        this.Changes.LocalAdd++;
                    }
                }
            }
        }

        private async Task UpdateWorkbookTasks(List<VercorsTask> vercorsTasks)
        {
            foreach (var vercorsTask in vercorsTasks)
            {
                var task = this.Workbook.Tasks.FirstOrDefault(t => t.SyncId == vercorsTask.Id);
                if (task != null)
                {
                    //if (vercorsTask.IsEquivalentTo(task))
                    //{
                    //    continue;
                    //}

                    bool useVercorsInformation = false;

                    if (!this.Metadata.EditedTasks.ContainsKey(task.Id))
                    {
                        // if the task is not in the pending edited tasks in the metadata, it has been modified
                        // only in Vercors, so we are sure to take this version
                        useVercorsInformation = true;
                    }
                    else
                    {
                        // the task has been modified in Vercors and in 2Day
                        // compare modification dates to use the appropriate version
                        useVercorsInformation = (task.Modified < vercorsTask.Modified);
                    }

                    if (useVercorsInformation)
                    {
                        this.PrepareTaskUpdate(task);

                        vercorsTask.UpdateTarget(task, this.Workbook, () => this.GetDefaultFolder());

                        this.Changes.LocalEdit++;
                    }
                    else
                    {
                        var updatedVercorsTask = await this.service.UpdateTask(new VercorsTask(this.Workbook, task));
                        if (updatedVercorsTask != null)
                        {
                            this.Changes.WebEdit++;
                        }
                    }
                    // Task has just been synchronized
                    this.Metadata.EditedTasks.Remove(task.Id);
                }
            }
        }

        #endregion
        
        private static string FormatPercentage(int step, int total)
        {
            if (total == 1)
                return "50%";

            double value = Math.Min(100.0, (double)(step*100)/total);
            string output = value.ToString("##");
            if (string.IsNullOrEmpty(output))
                return "0%";
            else
                return output + "%";
        }

        public override async Task<bool> CheckLoginAsync()
        {
            try
            {
                return await this.service.LoginAsync(this.Manager.IsBackground);
            }
            catch (Exception ex)
            {
                this.Manager.TrackEvent("Login failed " + this.Service, ex.Message);

                this.OnSynchronizationFailed(ex.Message);
                return false;
            }
        }

        public override void Reset(bool clearSettings)
        {
            this.service.Logout();
        }

        public override void Cancel()
        {
            this.service.Cancel();
        }
    }
}

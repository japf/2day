using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Shared.Sync
{
    public abstract class AdvancedSyncronizationManagerBase : IAdvancedSyncronizationManager
    {
        private readonly IWorkbook workbook;
        private readonly ISynchronizationManager synchronizationManager;
        private readonly ITrackingManager trackingManager;

        protected IWorkbook Workbook
        {
            get { return this.workbook; }
        }

        protected ISynchronizationManager SynchronizationManager
        {
            get { return this.synchronizationManager; }
        }
        
        protected ITrackingManager TrackingManager
        {
            get { return this.trackingManager; }
        }

        protected AdvancedSyncronizationManagerBase(IWorkbook workbook, ISynchronizationManager synchronizationManager, ITrackingManager trackingManager)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (synchronizationManager == null)
                throw new ArgumentNullException(nameof(synchronizationManager));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));

            this.workbook = workbook;
            this.synchronizationManager = synchronizationManager;
            this.trackingManager = trackingManager;
        }

        public async Task<bool> AdvancedSync()
        {
            if (this.synchronizationManager.ActiveProvider == null)
                return false;

            var provider = this.synchronizationManager.ActiveProvider;

            // try to keep color and icon if current provider do not support syncing them
            bool colorSupported = (provider.SupportedFeatures & SyncFeatures.Colors) == SyncFeatures.Colors;
            bool iconSupported = (provider.SupportedFeatures & SyncFeatures.Icons) == SyncFeatures.Icons;
            bool contextSupported = (provider.SupportedFeatures & SyncFeatures.Context) == SyncFeatures.Context;
            bool smartViewSupported = (provider.SupportedFeatures & SyncFeatures.SmartView) == SyncFeatures.SmartView;

            // key is the folder name, value is the item kept (color code or icon id)
            var colorMap = new Dictionary<string, string>();
            var iconMap = new Dictionary<string, int>();
            var orderMap = new Dictionary<string, int>();
            var smartviewMap = new Dictionary<string, FolderBaseEntry>();
            var contextMap = new Dictionary<string, FolderBaseEntry>();

            foreach (var folder in this.workbook.Folders)
            {
                if (!colorSupported && !colorMap.ContainsKey(folder.Name))
                    colorMap.Add(folder.Name, folder.Color);

                if (!iconSupported && !iconMap.ContainsKey(folder.Name))
                    iconMap.Add(folder.Name, folder.IconId);
                if (!iconSupported && !orderMap.ContainsKey(folder.Name))
                    orderMap.Add(folder.Name, folder.Order);
            }

            if (!contextSupported)
            {
                foreach (var task in this.Workbook.Tasks)
                {
                    if (task.Context != null)
                    {
                        string key = this.GetTaskKey(task);
                        if (!contextMap.ContainsKey(key))
                            contextMap.Add(key, new FolderBaseEntry(task.Context));
                    }
                }
            }

            if (!smartViewSupported)
            {
                foreach (var smartView in this.Workbook.SmartViews)
                {
                    if (!smartviewMap.ContainsKey(smartView.Name))
                        smartviewMap.Add(smartView.Name, new FolderBaseEntry(smartView, smartView.Rules));
                }
            }

            bool result = await this.AdvancedSyncCore();

            foreach (var kvp in colorMap)
            {
                var folder = this.workbook.Folders.FirstOrDefault(f => f.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                if (folder != null)
                    folder.Color = kvp.Value;
            }
            foreach (var kvp in iconMap)
            {
                var folder = this.workbook.Folders.FirstOrDefault(f => f.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                if (folder != null)
                    folder.IconId = kvp.Value;
            }
            foreach (var kvp in orderMap)
            {
                var folder = this.workbook.Folders.FirstOrDefault(f => f.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                if (folder != null)
                    folder.Order = kvp.Value;
            }

            if (orderMap.Count > 0)
                this.workbook.ApplyFolderOrder(this.workbook.Folders.OrderBy(f => f.Order).ToList());

            foreach (var kvp in contextMap)
            {
                string key = kvp.Key;
                FolderBaseEntry folderEntry = kvp.Value;

                ITask task = this.Workbook.Tasks.FirstOrDefault(t => this.GetTaskKey(t) == key);
                if (task != null)
                {
                    IContext context = this.workbook.Contexts.FirstOrDefault(c => c.Name.Equals(folderEntry.Name, StringComparison.OrdinalIgnoreCase));
                    if (context == null)
                    {
                        context = this.workbook.AddContext(folderEntry.Name);
                        context.GroupAscending = folderEntry.GroupAscending;
                        context.TaskGroup = folderEntry.Group;
                    }

                    if (context != null)
                        task.Context = context;
                }
            }

            foreach (var kvp in smartviewMap)
            {
                FolderBaseEntry folderEntry = kvp.Value;
                ISmartView smartview = this.workbook.SmartViews.FirstOrDefault(t => t.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                if (smartview == null && !string.IsNullOrEmpty(folderEntry.Parameter))
                {
                    smartview = this.workbook.AddSmartView(kvp.Key, folderEntry.Parameter);
                    smartview.GroupAscending = folderEntry.GroupAscending;
                    smartview.TaskGroup = folderEntry.Group;
                }
            }

            return result;
        }

        private string GetTaskKey(ITask task)
        {
            // use several fields of the task to build a kind of unique key
            string key = task.Title + task.Folder.Name;
            if (task.Due.HasValue)
                key += task.Due.Value.Date.ToString("yyyy MMMM dd");
            if (task.Note != null && task.Note.Length > 5)
                key += task.Note.Substring(0, 5);

            return key;
        }

        protected abstract Task<bool> AdvancedSyncCore();

        private class FolderBaseEntry
        {
            public string Name { get; private set; }
            public string Parameter { get; private set; }
            public bool GroupAscending { get; private set; }
            public TaskGroup Group { get; private set; }

            public FolderBaseEntry(IAbstractFolder folder, string parameter = null)
            {
                this.Name = folder.Name;
                this.GroupAscending = folder.GroupAscending;
                this.Group = folder.TaskGroup;
                this.Parameter = parameter;
            }
        }
    }
}
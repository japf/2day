using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Shared.Sync
{
    // CAUTION: because this class is serialized to a XML file, do NOT rename any of its properties
    // because it will break compatibility with previous versions
    [DataContract(Namespace = "http://www.2day-app.com/schemas")]
    public class SynchronizationMetadata : ISynchronizationMetadata
    {
        public const string Filename = "sync.xml";

        private List<ITask> ignoredTasks;
        private List<string> ignoredFolders;
        private List<string> ignoredContexts;
        private List<string> ignoredSmartViews;

        [DataMember]
        public DateTime LastSync { get; set; }

        [DataMember]
        public SynchronizationService ActiveProvider { get; set; }

        public bool HasChanges
        {
            get
            {
                return this.AddedTasks.Count > 0 || this.AddedFolders.Count > 0 || this.AddedContexts.Count > 0 || this.AddedSmartViews.Count > 0 ||
                       this.DeletedTasks.Count > 0 || this.DeletedFolders.Count > 0 || this.DeletedContexts.Count > 0 || this.DeletedSmartViews.Count > 0 ||
                       this.EditedTasks.Count > 0 || this.EditedFolders.Count > 0 || this.EditedContexts.Count > 0 || this.EditedSmartViews.Count > 0;
            }
        }

        [DataMember]
        public List<string> Backups { get; set; }

        [DataMember]
        public List<int> AddedFolders { get; set; }

        [DataMember]
        public List<int> EditedFolders { get; set; }

        [DataMember]
        public List<DeletedEntry> DeletedFolders { get; set; }

        [DataMember]
        public List<int> AddedContexts { get; set; }

        [DataMember]
        public List<int> EditedContexts { get; set; }

        [DataMember]
        public List<DeletedEntry> DeletedContexts { get; set; }

        [DataMember]
        public List<int> AddedSmartViews { get; set; }

        [DataMember]
        public List<int> EditedSmartViews { get; set; }

        [DataMember]
        public List<DeletedEntry> DeletedSmartViews { get; set; }

        [DataMember]
        public List<int> AddedTasks { get; set; }

        [DataMember]
        public Dictionary<int, TaskProperties> EditedTasks { get; set; }

        [DataMember]
        public List<DeletedEntry> DeletedTasks { get; set; }

        [DataMember]
        public Dictionary<string, string> ProviderDatas { get; set; }

        [DataMember]
        public List<int> AfterSyncEditedTasks { get; set; }

        [DataMember]
        public List<string> AfterSyncEditedFolders { get; set; }
        
        [DataMember]
        public List<string> AfterSyncEditedContexts { get; set; }

        [DataMember]
        public List<string> AfterSyncEditedSmartViews { get; set; }

        public SynchronizationMetadata()
        {
            this.DeletedFolders = new List<DeletedEntry>();
            this.AddedFolders = new List<int>();
            this.EditedFolders = new List<int>();

            this.DeletedContexts = new List<DeletedEntry>();
            this.AddedContexts = new List<int>();
            this.EditedContexts = new List<int>();

            this.DeletedSmartViews = new List<DeletedEntry>();
            this.AddedSmartViews = new List<int>();
            this.EditedSmartViews = new List<int>();

            this.DeletedTasks = new List<DeletedEntry>();
            this.AddedTasks = new List<int>();
            this.EditedTasks = new Dictionary<int, TaskProperties>();

            this.ProviderDatas = new Dictionary<string, string>();
            this.Backups = new List<string>();

            this.AfterSyncEditedContexts = new List<string>();
            this.AfterSyncEditedFolders = new List<string>();
            this.AfterSyncEditedTasks = new List<int>();

            this.ignoredFolders = new List<string>();
            this.ignoredContexts = new List<string>();
            this.ignoredSmartViews = new List<string>();
            this.ignoredTasks = new List<ITask>();
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            this.ignoredTasks = new List<ITask>();
            this.ignoredFolders = new List<string>();
            this.ignoredContexts = new List<string>();
            this.ignoredSmartViews = new List<string>();

            // the following properties have been added in 2Day 2.0
            // and will be missing for user upgrading to 2.0
            if (this.AddedContexts == null)
                this.AddedContexts = new List<int>();
            if (this.EditedContexts == null)
                this.EditedContexts = new List<int>();
            if (this.DeletedContexts == null)
                this.DeletedContexts  = new List<DeletedEntry>();

            // added for SmartView release
            if (this.AddedSmartViews == null)
                this.AddedSmartViews = new List<int>();
            if (this.EditedSmartViews == null)
                this.EditedSmartViews = new List<int>();
            if (this.DeletedSmartViews == null)
                this.DeletedSmartViews = new List<DeletedEntry>();

            if (this.AfterSyncEditedContexts == null)
                this.AfterSyncEditedContexts = new List<string>();
            if (this.AfterSyncEditedSmartViews == null)
                this.AfterSyncEditedSmartViews = new List<string>();
            if (this.AfterSyncEditedFolders == null)
                this.AfterSyncEditedFolders = new List<string>();
            if (this.AfterSyncEditedTasks == null)
                this.AfterSyncEditedTasks = new List<int>();
        }

        public void Reset()
        {
            this.DeletedFolders.Clear();
            this.AddedFolders.Clear();
            this.EditedFolders.Clear();

            this.DeletedContexts.Clear();
            this.AddedContexts.Clear();
            this.EditedContexts.Clear();

            this.DeletedSmartViews.Clear();
            this.AddedSmartViews.Clear();
            this.EditedSmartViews.Clear();

            this.DeletedTasks.Clear();
            this.AddedTasks.Clear();
            this.EditedTasks.Clear();

            this.ignoredFolders.Clear();
            this.ignoredContexts.Clear();
            this.ignoredSmartViews.Clear();
            this.ignoredTasks.Clear();

            this.LastSync = DateTime.MinValue;
        }

        public void IgnoreTask(ITask task)
        {
            if (!this.ignoredTasks.Contains(task))
                this.ignoredTasks.Add(task);
        }

        public void IgnoreFolder(string folder)
        {
            if (!this.ignoredFolders.Contains(folder))
                this.ignoredFolders.Add(folder);
        }

        public void IgnoreContext(string context)
        {
            if (!this.ignoredContexts.Contains(context))
                this.ignoredContexts.Add(context);
        }

        public void IgnoreSmartView(string smartview)
        {
            if (!this.ignoredSmartViews.Contains(smartview))
                this.ignoredSmartViews.Add(smartview);
        }
        
        public void FolderAdded(IFolder folder)
        {
            if (this.ShouldIgnoreFolder(folder.Name))
                return;

            if (!this.AddedFolders.Contains(folder.Id))
                this.AddedFolders.Add(folder.Id);
        }

        public void FolderEdited(IFolder folder)
        {
            if (this.ShouldIgnoreFolder(folder.Name))
                return;

            if (!this.EditedFolders.Contains(folder.Id) && !this.AddedFolders.Contains(folder.Id))
                this.EditedFolders.Add(folder.Id);
        }

        public void FolderRemoved(IFolder folder)
        {
            if (this.ShouldIgnoreFolder(folder.Name))
                return;

            if (this.AddedFolders.Contains(folder.Id))
            {
                // folder has been added then deleted
                this.AddedFolders.Remove(folder.Id);
                this.EditedFolders.Remove(folder.Id);
            }
            else
            {
                // folder has been deleted, make sure it's not already in the edited list
                this.EditedFolders.Remove(folder.Id);

                this.DeletedFolders.Add(new DeletedEntry(folder.Id, folder.SyncId));
            }
        }

        public void ContextAdded(IContext context)
        {
            if (this.ShouldIgnoreContext(context.Name))
                return;

            if (!this.AddedContexts.Contains(context.Id))
                this.AddedContexts.Add(context.Id);
        }

        public void ContextEdited(IContext context)
        {
            if (this.ShouldIgnoreContext(context.Name))
                return;

            if (!this.EditedContexts.Contains(context.Id) && !this.AddedContexts.Contains(context.Id))
                this.EditedContexts.Add(context.Id);
        }

        public void ContextRemoved(IContext context)
        {
            if (this.ShouldIgnoreContext(context.Name))
                return;

            if (this.AddedContexts.Contains(context.Id))
            {
                // context has been added then deleted
                this.AddedContexts.Remove(context.Id);
                this.EditedContexts.Remove(context.Id);
            }
            else
            {
                this.DeletedContexts.Add(new DeletedEntry(context.Id, context.SyncId));
            }
        }

        public void SmartViewAdded(ISmartView smartview)
        {
            if (this.ShouldIgnoreSmartView(smartview.Name))
                return;

            if (!this.AddedSmartViews.Contains(smartview.Id))
                this.AddedSmartViews.Add(smartview.Id);
        }

        public void SmartViewEdited(ISmartView smartview)
        {
            if (this.ShouldIgnoreSmartView(smartview.Name))
                return;

            if (!this.EditedSmartViews.Contains(smartview.Id) && !this.AddedSmartViews.Contains(smartview.Id))
                this.EditedSmartViews.Add(smartview.Id);
        }

        public void SmartViewRemoved(ISmartView smartview)
        {
            if (this.ShouldIgnoreSmartView(smartview.Name))
                return;

            if (this.AddedSmartViews.Contains(smartview.Id))
            {
                // smartview has been added then deleted
                this.AddedSmartViews.Remove(smartview.Id);
                this.EditedSmartViews.Remove(smartview.Id);
            }
            else
            {
                this.DeletedSmartViews.Add(new DeletedEntry(smartview.Id, smartview.SyncId));
            }
        }

        public void TaskAdded(ITask task)
        {
            if (this.ShouldIgnoreTask(task))
                return;

            var deletedEntry = this.DeletedTasks.FirstOrDefault(d => d.SyncId == task.SyncId);
            if (deletedEntry.SyncId != null)
            {
                // task has been flagged as removed, and now as added
                // let's consider instead it's a folder change
                this.DeletedTasks.Remove(deletedEntry);
                this.TaskEdited(task, deletedEntry.Properties | TaskProperties.Folder);
            }
            else if (!this.AddedTasks.Contains(task.Id))
            {
                this.AddedTasks.Add(task.Id);
            }
        }

        private void TaskEdited(ITask task, TaskProperties properties)
        {
            if (this.ShouldIgnoreTask(task))
                return;

            // if the task has been added, nothing more to track
            if (this.AddedTasks.Contains(task.Id))
                return;

            // if the task is not yet in the EditedTasks dictionary, add it without properties for the moment
            if (!this.EditedTasks.ContainsKey(task.Id))
                this.EditedTasks.Add(task.Id, TaskProperties.None);

            // setup properties
            TaskProperties oldEntry = this.EditedTasks[task.Id];
            TaskProperties newEntry = (oldEntry | properties);

            this.EditedTasks[task.Id] = newEntry;
        }

        public void TaskEdited(ITask task, string propertyName)
        {
            this.TaskEdited(task, TaskPropertiesUtil.StringToTaskProperty(propertyName));
        }

        public void TaskRemoved(ITask task)
        {
            if (this.ShouldIgnoreTask(task))
                return;

            if (this.AddedTasks.Contains(task.Id))
            {
                // task has been added then deleted
                this.AddedTasks.Remove(task.Id);
                this.EditedTasks.Remove(task.Id);
            }
            else
            {
                TaskProperties properties = TaskProperties.None;
                if (this.EditedTasks.ContainsKey(task.Id))
                {
                    properties |= this.EditedTasks[task.Id];
                    this.EditedTasks.Remove(task.Id);
                }

                this.DeletedTasks.Add(new DeletedEntry(task.Id, task.SyncId, properties));
            }
        }

        private bool ShouldIgnoreTask(ITask task)
        {
            return this.ignoredTasks.Contains(task);
        }

        private bool ShouldIgnoreFolder(string folder)
        {
            return this.ignoredFolders.Contains(folder);
        }

        private bool ShouldIgnoreContext(string context)
        {
            return this.ignoredContexts.Contains(context);
        }

        private bool ShouldIgnoreSmartView(string smartview)
        {
            return this.ignoredSmartViews.Contains(smartview);
        }

        public void ClearIgnoreList()
        {
            if (this.ignoredTasks == null || this.ignoredFolders == null || this.ignoredContexts == null || this.ignoredSmartViews == null)
            {
                this.ignoredTasks = new List<ITask>();
                this.ignoredFolders = new List<string>();
                this.ignoredContexts = new List<string>();
                this.ignoredSmartViews = new List<string>();
            }
            else
            {
                this.AfterSyncEditedTasks = this.ignoredTasks.Select(t => t.Id).ToList();
                this.AfterSyncEditedFolders = this.ignoredFolders.ToList();
                this.AfterSyncEditedContexts = this.ignoredContexts.ToList();
                this.AfterSyncEditedSmartViews = this.ignoredSmartViews.ToList();

                this.ignoredTasks.Clear();
                this.ignoredFolders.Clear();
                this.ignoredContexts.Clear();
                this.ignoredSmartViews.Clear();
            }
        }
    }
}

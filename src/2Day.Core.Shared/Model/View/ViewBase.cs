using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Model.Impl;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public abstract class ViewBase : ModelEntityBase, IView
    {
        protected readonly IView owner;
        private readonly List<ITask> tasks;
        private readonly List<IFolder> trackedFolders;

        private readonly IWorkbook workbook;
        private string name;

        private bool includeNoDate;
        private Predicate<ITask> taskPredicate;
        
        public int Id
        {
            get { return this.owner.Id; }
            set { this.owner.Id = value; }

        }

        public string Name
        {
            get { return this.name; }
            set 
            {
                if (this.owner.Name != value)
                {
                    this.owner.Name = value;
                    this.name = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }

        public string Color
        {
            get { return ResourcesLocator.ViewColor; }
        }

        public int IconId
        {
            get
            {
                return this.owner.IconId;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public IEnumerable<ITask> Tasks
        {
            get { return this.tasks; }
        }

        public int TaskCount
        {
            get { return this.tasks.Count; }
        }

        public int Order
        {
            get { return this.owner.Order; }
            set
            {
                if (this.owner.Order != value)
                {
                    this.owner.Order = value;
                    this.RaisePropertyChanged("Order");
                }
            }
        }

        public TaskGroup TaskGroup
        {
            get
            {
                return this.owner.TaskGroup;
            }
            set
            {
                if (this.owner.TaskGroup != value)
                {
                    this.owner.TaskGroup = value;
                    this.RaisePropertyChanged("TaskGroup");
                }
            }
        }

        public bool GroupAscending
        {
            get { return this.owner.GroupAscending; }
            set
            {
                if (this.owner.GroupAscending != value)
                {
                    this.owner.GroupAscending = value;
                    this.RaisePropertyChanged("GroupAscending");
                }
            }
        }

        public ITask this[int taskId]
        {
            get { return this.tasks.FirstOrDefault(t => t.Id == taskId); }
        }

        public virtual string EmptyHeader
        {
            get { return string.Empty; }
        }

        public virtual string EmptyHint
        {
            get { return string.Empty; }
        }

        public virtual bool CanReceiveTasks
        {
            get { return false; }
        }

        protected bool IncludeNoDate
        {
            get { return this.includeNoDate; }
        }

        public virtual bool HasCustomCommand
        {
            get { return false; }
        }

        public event EventHandler GroupingChanged;
        public event EventHandler<EventArgs<ITask>> TaskAdded;
        public event EventHandler<EventArgs<ITask>> TaskRemoved;
        public event EventHandler<EventArgs<IFolder>> ChildFolderChanged;

        protected ViewBase(IWorkbook workbook, IView owner, string name = null)
        {
            if (workbook == null)
                throw new ArgumentNullException("workbook");
            if (owner == null)
                throw new ArgumentNullException("owner");

            this.owner = owner;
            this.owner.PropertyChanged += (s, e) => this.RaisePropertyChanged(e.PropertyName);

            this.tasks = new List<ITask>();
            this.trackedFolders = new List<IFolder>();
            this.workbook = workbook;

            this.workbook.FolderAdded += this.OnFolderAdded;
            this.workbook.FolderRemoved += this.OnFolderRemoved;
            this.workbook.Settings.KeyChanged += this.OnSettingsKeyChanged;

            if (string.IsNullOrEmpty(name) && owner is ISystemView)
            {
                try
                {
                    // update Name because during initialization, Name column contains the name
                    // of the resource to fetch for localization
                    string key = "SystemView_Title" + ((ISystemView)this.owner).ViewKind;
                    string localizedName = StringResources.ResourceManager.GetString(key);
                    if (!string.IsNullOrEmpty(localizedName))
                        this.name = localizedName;
                    else
                        this.name = this.owner.Name;
                }
                catch (Exception)
                {
                }
            }
            else
            {
                this.name = name;
            }
        }

        public virtual System.Threading.Tasks.Task ExecuteCustomCommandAsync()
        {
            return System.Threading.Tasks.Task.FromResult(0);
        }

        protected void Ready()
        {
            // create the predicate which will filter tasks
            this.UpdateFilterPredicate();

            foreach (var folder in this.workbook.Folders)
                this.StartTrackingFolder(folder);
        }

        protected abstract Predicate<ITask> BuildTaskPredicateCore();

        public virtual void MoveTasks(IEnumerable<ITask> tasks)
        {
            throw new NotSupportedException();
        }

        private void OnSettingsKeyChanged(object sender, SettingsKeyChanged e)
        {
            if (e.Key == CoreSettings.IncludeNoDateInViews)
            {
                this.UpdateFilterPredicate();
                this.Rebuild();
            }
        }

        public void DetachWorkbook()
        {
            if (this.workbook == null)
                throw new NotSupportedException("No workbook attached");

            foreach (var folder in this.workbook.Folders)
                this.StopTrackingFolder(folder);

            this.workbook.FolderAdded -= this.OnFolderAdded;
            this.workbook.FolderRemoved -= this.OnFolderRemoved;
            this.workbook.Settings.KeyChanged -= this.OnSettingsKeyChanged;
        }

        public virtual void Rebuild()
        {
            foreach (var task in this.tasks.ToList())
                this.StopTrackingTask(task);

            this.tasks.Clear();

            foreach (var folder in this.workbook.Folders)
                this.StartTrackingFolder(folder);
        }

        public void UpdateGroupingMode(TaskGroup taskGroup, bool ascending)
        {
            if (this.TaskGroup != taskGroup || this.GroupAscending != ascending)
            {
                this.TaskGroup = taskGroup;
                this.GroupAscending = ascending;

                if (this.GroupingChanged != null)
                    this.GroupingChanged(this, EventArgs.Empty);
            }
        }

        protected void UpdateFilterPredicate()
        {
            this.includeNoDate = this.workbook.Settings.GetValue<bool>(CoreSettings.IncludeNoDateInViews);
            this.taskPredicate = this.BuildTaskPredicateCore();
        }

        private void StartTrackingFolder(IFolder folder)
        {
            if (!this.trackedFolders.Contains(folder))
            {
                this.trackedFolders.Add(folder);

                folder.TaskAdded += this.OnFolderTaskAdded;
                folder.TaskRemoved += this.OnFolderTaskRemoved;
                folder.PropertyChanged += this.OnFolderPropertyChanged;
            }

            foreach (var task in folder.Tasks)
                this.StartTrackingTask(task);

            this.RaisePropertyChanged("TaskCount");
        }

        private void StopTrackingFolder(IFolder folder)
        {
            this.trackedFolders.Remove(folder);

            folder.TaskAdded -= this.OnFolderTaskAdded;
            folder.TaskRemoved -= this.OnFolderTaskRemoved;
            folder.PropertyChanged -= this.OnFolderPropertyChanged;

            foreach (var task in folder.Tasks)
                this.StopTrackingTask(task);

            this.RaisePropertyChanged("TaskCount");
        }

        protected virtual bool ShouldTrackFolder(IFolder folder)
        {
            return true;
        }

        private void InvalidateTaskTracking(IFolder folder)
        {
            if (!this.ShouldTrackFolder(folder))
            {
                // the filter of the folder does not match, remove all its tasks
                foreach (var task in folder.Tasks)
                    this.StopTrackingTask(task);
            }
            else
            {
                // fiter match, track tasks (will do nothing if we're already tracking them)
                foreach (var task in folder.Tasks)
                    this.StartTrackingTask(task);
            }

            this.RaisePropertyChanged("TaskCount");
        }

        private void StartTrackingTask(ITask task)
        {
            // do nothing if we're already tracking this task
            if (this.tasks.Contains(task))
                return;

            task.PropertyChanged += this.OnTaskPropertyChanged;

            if (this.taskPredicate(task) && this.ShouldTrackFolder(task.Folder))
            {
                this.tasks.Add(task);
                this.TaskAdded.Raise(this, new EventArgs<ITask>(task));
            }
        }

        private void StopTrackingTask(ITask task)
        {
            task.PropertyChanged -= this.OnTaskPropertyChanged;

            this.tasks.Remove(task);
            this.TaskRemoved.Raise(this, new EventArgs<ITask>(task));
        }

        protected void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var task = (ITask)sender;
            if (this.tasks.Contains(task) && !this.taskPredicate(task))
            {
                // remove this task
                this.tasks.Remove(task);
                this.TaskRemoved.Raise(this, new EventArgs<ITask>(task));
            }
            else if (!this.tasks.Contains(task) && this.taskPredicate(task) && this.ShouldTrackFolder(task.Folder))
            {
                // add this task
                this.tasks.Add(task);
                this.TaskAdded.Raise(this, new EventArgs<ITask>(task));
            }

            this.RaisePropertyChanged("TaskCount");
        }

        private void OnFolderTaskRemoved(object sender, EventArgs<ITask> e)
        {
            if (this.tasks.Contains(e.Item))
            {
                this.StopTrackingTask(e.Item);

                this.RaisePropertyChanged("TaskCount");
            }
        }

        private void OnFolderTaskAdded(object sender, EventArgs<ITask> e)
        {
            if (!this.tasks.Contains(e.Item))
            {
                this.StartTrackingTask(e.Item);

                this.RaisePropertyChanged("TaskCount");
            }
        }

        private void OnFolderRemoved(object sender, EventArgs<IFolder> e)
        {
            this.StopTrackingFolder(e.Item);
        }

        private void OnFolderAdded(object sender, EventArgs<IFolder> e)
        {
            this.StartTrackingFolder(e.Item);
        }

        private void OnFolderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowInViews")
            {
                // invalidate tracking of all tasks of this folder
                this.InvalidateTaskTracking((IFolder)sender);
            }
            else if (e.PropertyName == "Name")
            {
                if (this.ChildFolderChanged != null)
                    this.ChildFolderChanged(sender, new EventArgs<IFolder>((IFolder)sender));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using SQLite.Net.Attributes;

namespace Chartreuse.Today.Core.Shared.Model.Impl
{
    [DebuggerDisplay("Name: {Name}, Tasks: {TaskCount}")]
    public class Folder : ModelEntityBase, IFolder
    {
        #region fields

        private readonly List<ITask> tasks;

        private bool groupAscending;
        private TaskGroup taskGroup;
        private int order;
        private string syncId;
        private bool? showInViews;
        private string name;
        private string color;
        private int iconId;
        private DateTime modified;

        #endregion

        #region properties

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public IEnumerable<ITask> Tasks
        {
            get { return this.tasks; }
        }
        
        public int TaskCount
        {
            get { return this.tasks.Count; }
        }

        public ITask this[int taskId]
        {
            get { return this.tasks.FirstOrDefault(t => t.Id == taskId); }
        }

        public bool CanReceiveTasks
        {
            get { return true; }
        }

        public string EmptyHeader
        {
            get { return StringResources.Folder_EmptyHeader; }
        }

        public string EmptyHint
        {
            get { return StringResources.Folder_EmptyHint; }
        }

        public DateTime Modified
        {
            get
            {
                return this.modified;
            }
            set
            {
                if (this.modified != value)
                {
                    this.modified = value;
                    this.RaisePropertyChanged("Modified");
                }
            }
        }

        public int Order
        {
            get { return this.order; }
            set
            {
                if (this.order != value)
                {
                    this.order = value;
                    this.RaisePropertyChanged("Order");

                    this.Modified = DateTime.Now;
                }
            }
        }

        public TaskGroup TaskGroup
        {
            get { return this.taskGroup; }
            set
            {
                if (this.taskGroup != value)
                {
                    this.taskGroup = value;
                    this.RaisePropertyChanged("TaskGroup");
                }
            }
        }

        public bool GroupAscending
        {
            get { return this.groupAscending; }
            set
            {
                if (this.groupAscending != value)
                {
                    this.groupAscending = value;
                    this.RaisePropertyChanged("GroupAscending");
                }
            }
        }

        public string SyncId
        {
            get { return this.syncId; }
            set
            {
                if (this.syncId != value)
                {
                    this.syncId = value;
                    this.RaisePropertyChanged("SyncId");
                }
            }
        }

        public bool? ShowInViews
        {
            get { return this.showInViews; }
            set
            {
                if (this.showInViews != value)
                {
                    this.showInViews = value;
                    this.RaisePropertyChanged("ShowInViews");
                }
            }
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.RaisePropertyChanged("Name");

                    this.Modified = DateTime.Now;
                }
            }
        }
        
        public string Color
        {
            get { return this.color; }
            set
            {
                if (this.color != value)
                {
                    this.color = value;
                    this.RaisePropertyChanged("Color");

                    this.Modified = DateTime.Now;
                }
            }
        }

        public int IconId
        {
            get { return this.iconId; }
            set
            {
                if (this.iconId != value)
                {
                    this.iconId = value;
                    this.RaisePropertyChanged("IconId");

                    this.Modified = DateTime.Now;
                }
            }
        }

        public bool HasCustomCommand
        {
            get { return false; }
        }

        #endregion

        #region events

        public event EventHandler GroupingChanged;
        public event EventHandler<EventArgs<ITask>> TaskAdded;
        public event EventHandler<EventArgs<ITask>> TaskRemoved;
        public event EventHandler<EventArgs<IFolder>> ChildFolderChanged
        {
            add { }
            remove { }
        }

        #endregion

        public Folder()
        {
            this.tasks = new List<ITask>();

            this.groupAscending = true;
            this.taskGroup = TaskGroup.DueDate;
            this.ShowInViews = true;
        }

        public System.Threading.Tasks.Task ExecuteCustomCommandAsync()
        {
            return System.Threading.Tasks.Task.FromResult(0);
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

        public void RegisterTask(ITask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            if (this.tasks.Contains((Task)task))
                return;

            task.Folder = this;

            this.tasks.Add((Task)task);
            this.TaskAdded.Raise(this, new EventArgs<ITask>(task));

            this.RaisePropertyChanged("TaskCount");
        }

        public void UnregisterTask(ITask task, bool signalChange = true)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            if (!this.tasks.Contains((Task)task))
                return;

            this.tasks.Remove((Task)task);
            this.TaskRemoved.Raise(this, new EventArgs<ITask>(task));

            this.RaisePropertyChanged("TaskCount");
        }

        public bool RemoveTask(ITask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            if (this.tasks.All(t => t.Id != task.Id))
                return false;

            task.Context = null;

            this.TaskRemoved.Raise(this, new EventArgs<ITask>(task));
            this.tasks.Remove((Task) task);

            this.RaisePropertyChanged("TaskCount");

            return true;
        }

        public virtual void MoveTasks(IEnumerable<ITask> tasks)
        {
            foreach (var task in tasks)
                task.Folder = this;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Chartreuse.Today.Core.Shared.Icons;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using SQLite.Net.Attributes;

namespace Chartreuse.Today.Core.Shared.Model.Impl
{
    [DebuggerDisplay("Name: {Name}, Tasks: {TaskCount}")]
    [Table("Contexts")]
    public class Context : ModelEntityBase, IContext
    {
        #region fields

        private readonly List<ITask> items;

        private string name;
        private TaskGroup taskGroup;
        private bool groupAscending;
        private bool isEnabled;
        private int order;
        private string syncId;

        #endregion

        #region properties

        [Ignore]
        public int IconId
        {
            get { return FontIconHelper.IconIdContext; }
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Ignore]
        public string Color
        {
            get { return ResourcesLocator.ContextColor; }
        }

        public string EmptyHeader
        {
            get { return StringResources.Context_EmptyHeader; }
        }

        public string EmptyHint
        {
            get { return StringResources.Context_EmptyHint; }
        }

        public bool CanReceiveTasks
        {
            get { return true; }
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
        
        public int Order
        {
            get { return this.order; }
            set
            {
                if (this.order != value)
                {
                    this.order = value;
                    this.RaisePropertyChanged("Order");
                }
            }
        }

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                if (this.isEnabled != value)
                {
                    this.isEnabled = value;
                    this.RaisePropertyChanged("IsEnabled");
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
        public IEnumerable<ITask> Tasks
        {
            get { return this.items; }
        }

        public int TaskCount
        {
            get { return this.items.Count; }
        }

        public ITask this[int taskId]
        {
            get { return this.items.SingleOrDefault(t => t.Id == taskId); }
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
        public event EventHandler<EventArgs<IFolder>> ChildFolderChanged { add { } remove {} }

        #endregion

        public Context()
        {
            this.items = new List<ITask>();

            this.groupAscending = true;
            this.taskGroup = TaskGroup.DueDate;
        }

        public System.Threading.Tasks.Task ExecuteCustomCommandAsync()
        {
            return System.Threading.Tasks.Task.FromResult(0);
        }

        public void RegisterTask(ITask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            if (this.items.Contains(task))
                return;

            task.Context = this;

            this.items.Add(task);
            this.TaskAdded.Raise(this, new EventArgs<ITask>(task));

            this.RaisePropertyChanged("TaskCount");
        }

        public void UnregisterTask(ITask task, bool signalChange = true)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            if (!this.items.Contains(task))
                return;

            this.items.Remove(task);
            this.TaskRemoved.Raise(this, new EventArgs<ITask>(task));

            this.RaisePropertyChanged("TaskCount");
        }

        public void UpdateGroupingMode(TaskGroup taskGroup, bool @ascending)
        {
            if (this.TaskGroup != taskGroup || this.GroupAscending != ascending)
            {
                this.TaskGroup = taskGroup;
                this.GroupAscending = ascending;

                if (this.GroupingChanged != null)
                    this.GroupingChanged(this, EventArgs.Empty);
            }
        }

        public void MoveTasks(IEnumerable<ITask> tasks)
        {
            foreach (var task in tasks)
                task.Context = this;
        }
    }
}

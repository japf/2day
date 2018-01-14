using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SQLite.Net.Attributes;

namespace Chartreuse.Today.Core.Shared.Model.Impl
{
    [DebuggerDisplay("Name: {Name}")]
    [Table("SmartView")]
    public class SmartView : ModelEntityBase, ISmartView
    {
        // note: use property change notifications for Order, TaskGroup and GroupAscending 
        // because they are likely to change and win database context listen for those
        // changes
        private readonly List<ITask> tasks;
        private bool isEnabled;
        private TaskGroup taskGroup;
        private bool groupAscending;
        private int order;
        private string rules;
        private string syncId;
        private string name;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

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

        public string Color { get; set; }
        
        public int IconId { get; set; }

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

        public string Rules
        {
            get { return this.rules; }
            set
            {
                if (this.rules != value)
                {
                    this.rules = value;
                    this.RaisePropertyChanged("Rules");
                }
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
        
        public ITask this[int taskId]
        {
            get { return this.tasks.FirstOrDefault(t => t.Id == taskId); }
        }

        public string EmptyHeader
        {
            get; private set;
        }

        public string EmptyHint
        {
            get; private set;
        }

        public bool CanReceiveTasks
        {
            get; private set;
        }

        public event EventHandler GroupingChanged
        {
            add { }
            remove { }
        }

        public event EventHandler<EventArgs<ITask>> TaskAdded
        {
            add { }
            remove { }
        }

        public event EventHandler<EventArgs<ITask>> TaskRemoved
        {
            add { }
            remove { }
        }

        public event EventHandler<EventArgs<IFolder>> ChildFolderChanged
        {
            add { }
            remove { }
        }

        public bool HasCustomCommand
        {
            get { return false; }
        }

        public bool ShowCompletedTasks
        {
            get { return false; }
        }

        public SmartView()
        {
            this.tasks = new List<ITask>();
        }

        public System.Threading.Tasks.Task ExecuteCustomCommandAsync()
        {
            return System.Threading.Tasks.Task.FromResult(0);
        }

        public void UpdateGroupingMode(TaskGroup taskGroup, bool @ascending)
        {
        }

        public void MoveTasks(IEnumerable<ITask> tasks)
        {
            throw new NotImplementedException();
        }

        public ViewKind ViewKind { get; set; }

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

        public void DetachWorkbook()
        {
            throw new NotImplementedException();
        }

        public void Rebuild()
        {
        }
    }
}

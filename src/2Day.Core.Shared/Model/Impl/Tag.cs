using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Chartreuse.Today.Core.Shared.Resources;
using SQLite.Net.Attributes;

namespace Chartreuse.Today.Core.Shared.Model.Impl
{
    [Table("Tags")]
    [DebuggerDisplay("Name: {Name}")]
    public class Tag : ModelEntityBase, ITag
    {
        private int order;
        private bool isEnabled;
        private TaskGroup taskGroup;
        private bool groupAscending;
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

        public string Color
        {
            get { return ResourcesLocator.TagViewColor; }
        }

        public int IconId { get; set; }

        public IEnumerable<ITask> Tasks
        {
            get { return Enumerable.Empty<ITask>(); }
        }

        public int TaskCount
        {
            get { return 0; }
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
            get
            {
                return this.isEnabled;
            }
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
            get
            {
                return this.taskGroup;
            }
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

        public ITask this[int taskId]
        {
            get { throw new NotImplementedException(); }
        }

        [Ignore]
        public string EmptyHeader
        {
            get;
            set;
        }

        [Ignore]
        public string EmptyHint
        {
            get;
            set;
        }

        public IView Owner
        {
            get { return null; }
        }

        public bool HasCustomCommand
        {
            get { return false; }
        }

        public bool CanReceiveTasks { get { return false; } }

        public event EventHandler GroupingChanged
        {
            add { throw new NotSupportedException(); }
            remove { }
        }

        public event EventHandler<EventArgs<ITask>> TaskAdded
        {
            add { throw new NotSupportedException(); }
            remove { }
        }

        public event EventHandler<EventArgs<ITask>> TaskRemoved
        {
            add { throw new NotSupportedException(); }
            remove { }
        }

        public event EventHandler<EventArgs<IFolder>> ChildFolderChanged
        {
            add { }
            remove { }
        }

        public void Rebuild()
        {
        }

        public void MoveTasks(IEnumerable<ITask> tasks)
        {
        }

        public void UpdateGroupingMode(TaskGroup newTaskGroup, bool newAscending)
        {
        }

        public System.Threading.Tasks.Task ExecuteCustomCommandAsync()
        {
            return System.Threading.Tasks.Task.FromResult(0);
        }
    }
}


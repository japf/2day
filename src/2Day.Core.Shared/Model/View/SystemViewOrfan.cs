using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class SystemViewOrfan : ISystemView
    {
        private bool isEnabled;

        public SystemViewOrfan(int iconId = 0)
        {
            this.IconId = iconId;
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { }
            remove { }
        }

        public int Id { get; set; }
        public string Color { get; private set; }
        public IEnumerable<ITask> Tasks { get; private set; }
        public int TaskCount { get; private set; }

        public ITask this[int taskId]
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasCustomCommand
        {
            get { return false; }
        }

        public string EmptyHeader { get; private set; }
        public string EmptyHint { get; private set; }
        public int IconId { get; private set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public TaskGroup TaskGroup { get; set; }
        public bool GroupAscending { get; set; }
        public void UpdateGroupingMode(TaskGroup taskGroup, bool @ascending)
        {
            throw new NotImplementedException();
        }

        public bool CanReceiveTasks { get; private set; }

        public System.Threading.Tasks.Task ExecuteCustomCommandAsync()
        {
            return System.Threading.Tasks.Task.FromResult(0);
        }

        public void MoveTasks(IEnumerable<ITask> tasks)
        {
            throw new NotImplementedException();
        }

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
            add { throw new NotSupportedException(); }
            remove { }
        }

        public void Rebuild()
        {
            throw new NotImplementedException();
        }

        public ViewKind ViewKind
        {
            get { return ViewKind.Today; }
        }

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set { this.isEnabled = value; }
        }

        public void DetachWorkbook()
        {
            throw new NotImplementedException();
        }
    }
}
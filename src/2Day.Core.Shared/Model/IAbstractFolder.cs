using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Model
{
    public interface IAbstractFolder : IModelEntity
    {
        int Id { get; set; }
        
        string Color { get; }
        
        IEnumerable<ITask> Tasks { get; }
        int TaskCount { get; }
        
        ITask this[int taskId] { get; }

        string EmptyHeader { get; }
        string EmptyHint { get; }

        bool HasCustomCommand { get; }
        Task ExecuteCustomCommandAsync();

        int IconId { get; }
        string Name { get; set; }
        int Order { get; set; }
        TaskGroup TaskGroup { get; set; }
        bool GroupAscending { get; set; }

        void UpdateGroupingMode(TaskGroup newTaskGroup, bool newAscending);

        bool CanReceiveTasks { get; }
        void MoveTasks(IEnumerable<ITask> tasks);

        event EventHandler GroupingChanged;
        event EventHandler<EventArgs<ITask>> TaskAdded;
        event EventHandler<EventArgs<ITask>> TaskRemoved;
        event EventHandler<EventArgs<IFolder>> ChildFolderChanged;
    }
}
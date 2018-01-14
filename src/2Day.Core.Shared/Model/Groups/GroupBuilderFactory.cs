using System;
using Chartreuse.Today.Core.Shared.Model.Groups.Factory;

namespace Chartreuse.Today.Core.Shared.Model.Groups
{
    public static class GroupBuilderFactory
    {
        public static GroupBuilder<ITask> GetGroupBuilder(IAbstractFolder folder, ISettings settings)
        {
            TaskGroup taskGroup = folder.TaskGroup;

            switch (taskGroup)
            {
                case TaskGroup.DueDate:
                    return new DueDateGroupBuilderFactory(folder, settings).CreateGroupBuilder();
                case TaskGroup.Status:
                    return new StatusGroupBuilderFactory(folder, settings).CreateGroupBuilder();
                case TaskGroup.Priority:
                    return new PriorityGroupBuilderFactory(folder, settings).CreateGroupBuilder();
                case TaskGroup.Folder:
                    return new FolderGroupBuilderFactory(folder, settings).CreateGroupBuilder();
                case TaskGroup.Action:
                    return new ActionGroupBuilderFactory(folder, settings).CreateGroupBuilder();
                case TaskGroup.Progress:
                    return new ProgressGroupBuilderFactory(folder, settings).CreateGroupBuilder();
                case TaskGroup.Context:
                    return new ContextGroupBuilderFactory(folder, settings).CreateGroupBuilder();
                case TaskGroup.StartDate:
                    return new StartDateGroupBuilderFactory(folder, settings).CreateGroupBuilder();
                case TaskGroup.Completed:
                    return new CompletedDateGroupBuilderFactory(folder, settings).CreateGroupBuilder();
                case TaskGroup.Modified:
                    return new ModifiedGroupBuilderFactory(folder, settings).CreateGroupBuilder();
                default:
                    throw new ArgumentOutOfRangeException(nameof(folder));
            }
        }
    }
}
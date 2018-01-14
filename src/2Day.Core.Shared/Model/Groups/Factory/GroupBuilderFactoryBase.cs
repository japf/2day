using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Collection;

namespace Chartreuse.Today.Core.Shared.Model.Groups.Factory
{
    public abstract class GroupBuilderFactoryBase
    {
        private readonly IAbstractFolder folder;
        private readonly ISettings settings;
        private readonly bool groupCompletedTasks;

        protected GroupBuilderFactoryBase(IAbstractFolder folder, ISettings settings, bool groupCompletedTasks)
        {
            this.folder = folder;
            this.settings = settings;

            this.groupCompletedTasks = groupCompletedTasks && settings.GetValue<CompletedTaskMode>(CoreSettings.CompletedTasksMode) == CompletedTaskMode.Group;
        }

        public string GroupBy(ITask task)
        {
            if (this.groupCompletedTasks && task.IsCompleted)
                return StringResources.ConverterStatus_Completed;
            else
                return this.GroupByCore(task);
        }

        protected abstract string GroupByCore(ITask task);

        protected int CompareGroup(Group<ITask> x, Group<ITask> y)
        {
            if (x.Count == 0 && y.Count > 0)
                return -1;
            else if (x.Count > 0 && y.Count == 0)
                return 1;
            else if (x.Count == 0 && y.Count == 0)
                return 0;
            else
            {
                var t1 = x[0];
                var t2 = y[0];

                if (this.groupCompletedTasks)
                {
                    if (t1.IsCompleted && !t2.IsCompleted)
                        return 1;
                    else if (!t1.IsCompleted && t2.IsCompleted)
                        return -1;
                    else
                        return this.CompareCore(t1, t2);
                }
                else
                {
                    return this.CompareCore(t1, t2);
                }
            }
        }

        protected abstract int CompareCore(ITask t1, ITask t2);

        protected abstract object OrderByCore(ITask task);

        public GroupBuilder<ITask> CreateGroupBuilder()
        {
            return new GroupBuilder<ITask>(
                this.OrderByCore,
                this.GroupBy,
                new GenericComparer<Group<ITask>>(this.CompareGroup),
                new TaskComparer(this.settings),
                this.folder.GroupAscending);
        }
    }
}
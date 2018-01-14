using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Groups.Factory
{
    public class StatusGroupBuilderFactory : GroupBuilderFactoryBase
    {
        public StatusGroupBuilderFactory(IAbstractFolder folder, ISettings settings)
            : base(folder, settings, false)
        {
        }

        protected override string GroupByCore(ITask task)
        {
            if (task.IsCompleted)
                return StringResources.ConverterStatus_Completed;
            else
                return StringResources.ConverterStatus_Todo;
        }

        protected override int CompareCore(ITask t1, ITask t2)
        {
            if (t1.IsCompleted && !t2.IsCompleted)
                return 1;
            else if (!t1.IsCompleted && t2.IsCompleted)
                return -1;
            else
                return 0;
        }

        protected override object OrderByCore(ITask task)
        {
            return task.IsCompleted;
        }
    }
}
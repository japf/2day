using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Groups.Factory
{
    public class PriorityGroupBuilderFactory : GroupBuilderFactoryBase
    {
        public PriorityGroupBuilderFactory(IAbstractFolder folder, ISettings settings)
            : base(folder, settings, true)
        {
        }

        protected override string GroupByCore(ITask task)
        {
            switch (task.Priority)
            {
                case TaskPriority.None:
                    return StringResources.ConverterAction_None;
                case TaskPriority.Low:
                    return StringResources.ConverterPriority_Low;
                case TaskPriority.Medium:
                    return StringResources.ConverterPriority_Medium;
                case TaskPriority.High:
                    return StringResources.ConverterPriority_High;
                case TaskPriority.Star:
                    return StringResources.ConverterPriority_Star;
                default:
                    return StringResources.ConverterAction_None;
            }
        }

        protected override int CompareCore(ITask t1, ITask t2)
        {
            if ((int)t1.Priority < (int)t2.Priority)
                return -1;
            else if ((int)t1.Priority > (int)t2.Priority)
                return 1;
            else
                return 0;
        }

        protected override object OrderByCore(ITask task)
        {
            return task.Priority;
        }
    }
}

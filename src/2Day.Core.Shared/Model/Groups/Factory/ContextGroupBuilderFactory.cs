using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Groups.Factory
{
    public class ContextGroupBuilderFactory : GroupBuilderFactoryBase
    {
        public ContextGroupBuilderFactory(IAbstractFolder folder, ISettings settings)
            : base(folder, settings, true)
        {
        }

        protected override string GroupByCore(ITask task)
        {
            if (task.Context != null)
                return task.Context.Name;
            else
                return StringResources.ConverterContext_None;
        }

        protected override int CompareCore(ITask t1, ITask t2)
        {
            if (t1.Context != null && t2.Context != null)
            {
                if (t1.Context == t2.Context)
                    return 0;

                if (t1.Context == t2.Context || (t1.Context.Order == t2.Context.Order))
                    return 0;
                else if (t1.Context.Order < t2.Context.Order)
                    return -1;
                else
                    return 1;
            }
            else if (t1.Context == null && t2.Context != null)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        protected override object OrderByCore(ITask task)
        {
            return task.Context != null ? task.Context.Name : StringResources.ConverterContext_None;
        }
    }
}
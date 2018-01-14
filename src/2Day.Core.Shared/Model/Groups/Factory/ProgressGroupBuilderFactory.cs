using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Groups.Factory
{
    public class ProgressGroupBuilderFactory : GroupBuilderFactoryBase
    {
        public ProgressGroupBuilderFactory(IAbstractFolder folder, ISettings settings)
            : base(folder, settings, false)
        {
        }

        protected override string GroupByCore(ITask task)
        {
            if (task.Progress == null)
                return StringResources.ConverterProgress_None;
            else
                return string.Format("{0}%", task.Progress.Value * 100);
        }

        protected override int CompareCore(ITask t1, ITask t2)
        {
            if (t1.HasProgress && !t2.HasProgress)
                return 1;
            else if (!t1.HasProgress && t2.HasProgress)
                return -1;
            else if (t1.Progress < t2.Progress)
                return -1;
            else if (t1.Progress > t2.Progress)
                return 1;
            else
                return 0;
        }

        protected override object OrderByCore(ITask task)
        {
            return task.Progress;
        }
    }
}
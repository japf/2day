using System;
using Chartreuse.Today.Core.Shared.Tools.Converter;

namespace Chartreuse.Today.Core.Shared.Model.Groups.Factory
{
    public class StartDateGroupBuilderFactory : GroupBuilderFactoryBase
    {
        private readonly RelativeDateConverter converter;

        public StartDateGroupBuilderFactory(IAbstractFolder folder, ISettings settings)
            : base(folder, settings, true)
        {
            this.converter = new RelativeDateConverter(settings);
        }

        protected override string GroupByCore(ITask task)
        {
            return this.converter.Convert(task.Start);
        }

        protected override int CompareCore(ITask t1, ITask t2)
        {
            DateTime? d1 = t1.Start;
            DateTime? d2 = t2.Start;

            if (!d1.HasValue && d2.HasValue)
                return 1;
            else if (d1.HasValue && !d2.HasValue)
                return -1;
            else if (!d1.HasValue && !d2.HasValue)
                return 0;
            else
            {
                if (d1.Value < d2.Value)
                    return -1;
                else if (d1.Value > d2.Value)
                    return 1;
                else
                    return 0;
            }
        }

        protected override object OrderByCore(ITask task)
        {
            return task.Start.HasValue ? task.Start : DateTime.MaxValue;
        }
    }
}
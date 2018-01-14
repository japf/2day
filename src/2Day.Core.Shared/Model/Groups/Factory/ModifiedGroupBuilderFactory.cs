using System;
using Chartreuse.Today.Core.Shared.Tools.Converter;

namespace Chartreuse.Today.Core.Shared.Model.Groups.Factory
{
    public class ModifiedGroupBuilderFactory : GroupBuilderFactoryBase
    {
        private readonly RelativeDateConverter converter;

        public ModifiedGroupBuilderFactory(IAbstractFolder folder, ISettings settings)
            : base(folder, settings, true)
        {
            this.converter = new RelativeDateConverter(settings);
        }

        protected override string GroupByCore(ITask task)
        {
            return this.converter.Convert(task.Modified);
        }

        protected override int CompareCore(ITask t1, ITask t2)
        {
            DateTime d1 = t1.Modified;
            DateTime d2 = t2.Modified;
            
            if (d1 < d2)
                return -1;
            else if (d1 > d2)
                return 1;
            else
                return 0;
        }

        protected override object OrderByCore(ITask task)
        {
            return task.Modified;
        }
    }
}
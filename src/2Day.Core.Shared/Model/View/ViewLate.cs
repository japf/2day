using System;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewLate : SystemDefinedView
    {
        public override string EmptyHeader
        {
            get { return StringResources.SystemView_Late_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_Late_EmptyHint; }
        }

        public ViewLate(IWorkbook workbook, ISystemView view)
            : base(workbook, view)
        {
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task => task.IsLate;
        }
    }
}
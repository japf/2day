using System;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewStartDate : SystemDefinedView
    {
        public override string EmptyHeader
        {
            get { return StringResources.SystemView_StartDate_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_StartDate_EmptyHint; }
        }

        public ViewStartDate(IWorkbook workbook, ISystemView view)
            : base(workbook, view)
        {
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task => task.Start.HasValue && task.Start.Value > DateTime.Now;
        }
    }
}
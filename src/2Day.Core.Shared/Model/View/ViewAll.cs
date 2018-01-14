using System;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewAll : SystemDefinedView
    {
        public override string EmptyHeader
        {
            get { return StringResources.SystemView_All_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_All_EmptyHint; }
        }

        public ViewAll(IWorkbook workbook, ISystemView view)
            : base(workbook, view)
        {
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task => true;
        }
    }
}
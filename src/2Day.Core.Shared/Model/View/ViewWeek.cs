using System;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewWeek : SystemDefinedView
    {
        public override string EmptyHeader
        {
            get { return StringResources.SystemView_Week_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_Week_EmptyHint; }
        }

        public ViewWeek(IWorkbook workbook, ISystemView view)
            : base(workbook, view)
        {
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task => task.IsLate || (task.Due.HasValue && (task.Due.Value.Date - DateTime.Now.Date).TotalDays <= 7.0) || (this.IncludeNoDate && !task.Due.HasValue);
        }
    }
}
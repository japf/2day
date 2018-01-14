using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewToday : SystemDefinedView
    {
        public override string EmptyHeader
        {
            get { return StringResources.SystemView_Today_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_Today_EmptyHint; }
        }

        public override bool CanReceiveTasks
        {
            get { return true; }
        }

        public ViewToday(IWorkbook workbook, ISystemView view) : base(workbook, view)
        {
        }

        public override void MoveTasks(IEnumerable<ITask> tasks)
        {
            var now = DateTime.Now;
            foreach (var task in tasks)
                task.SetDueAndAdjustReminder(now, tasks);
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task => task.IsLate || (task.Due.HasValue && task.Due.Value.Date == DateTime.Now.Date) || (this.IncludeNoDate && !task.Due.HasValue);
        }
    }
}
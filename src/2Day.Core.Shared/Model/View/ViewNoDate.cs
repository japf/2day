using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewNoDate : SystemDefinedView
    {
        public override string EmptyHeader
        {
            get { return StringResources.SystemView_NoDate_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_NoDate_EmptyHint; }
        }

        public override bool CanReceiveTasks
        {
            get { return true; }
        }

        public ViewNoDate(IWorkbook workbook, ISystemView view)
            : base(workbook, view)
        {
        }

        public override void MoveTasks(IEnumerable<ITask> tasks)
        {
            foreach (var task in tasks)
            {
                task.Due = null;
                task.Modified = DateTime.Now;
            }
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task => !task.Due.HasValue;
        }
    }
}
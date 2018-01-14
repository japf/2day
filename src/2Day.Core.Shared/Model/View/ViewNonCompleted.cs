using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewNonCompleted : SystemDefinedView
    {
        public override string EmptyHeader
        {
            get { return StringResources.SystemView_All_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_All_EmptyHeader; }
        }

        public override bool CanReceiveTasks
        {
            get { return true; }
        }

        public ViewNonCompleted(IWorkbook workbook, ISystemView view)
            : base(workbook, view)
        {
        }

        public override void MoveTasks(IEnumerable<ITask> tasks)
        {
            foreach (var task in tasks)
                task.IsCompleted = false;
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task => !task.IsCompleted;
        }
    }
}
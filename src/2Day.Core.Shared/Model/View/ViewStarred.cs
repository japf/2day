using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewStarred : SystemDefinedView
    {
        public override string EmptyHeader
        {
            get { return StringResources.SystemView_Starred_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_Starred_EmptyHint; }
        }

        public override bool CanReceiveTasks
        {
            get { return true; }
        }

        public ViewStarred(IWorkbook workbook, ISystemView view)
            : base(workbook, view)
        {
        }

        public override void MoveTasks(IEnumerable<ITask> tasks)
        {
            foreach (var task in tasks)
                task.Priority = TaskPriority.Star;
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task => task.Priority == TaskPriority.Star;
        }
    }
}
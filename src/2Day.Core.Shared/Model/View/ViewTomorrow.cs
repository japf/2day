using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewTomorrow : SystemDefinedView
    {
        internal static DateTime? Now { get; set; }

        public override string EmptyHeader
        {
            get { return StringResources.SystemView_Tomorrow_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_Tomorrow_EmptyHint; }
        }

        public override bool CanReceiveTasks
        {
            get { return true; }
        }

        public ViewTomorrow(IWorkbook workbook, ISystemView view)
            : base(workbook, view)
        {
        }

        public override void MoveTasks(IEnumerable<ITask> tasks)
        {
            var now = DateTime.Now.AddDays(1);
            foreach (var task in tasks)
                task.SetDueAndAdjustReminder(now, tasks);
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task =>
            {
                var now = DateTime.Now.Date;

                // for testing purpose
                if (Now.HasValue)
                    now = Now.Value;

                return (task.Due.HasValue && (task.Due.Value.Date - now).TotalDays < 2.0 && (task.Due.Value.Date - now).TotalDays >= 1.0) || (this.IncludeNoDate && !task.Due.HasValue);
            };
        }
    }
}
using System;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewReminder : SystemDefinedView
    {
        public override string EmptyHeader
        {
            get { return StringResources.SystemView_Reminder_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_Reminder_EmptyHint; }
        }

        public ViewReminder(IWorkbook workbook, ISystemView view)
            : base(workbook, view)
        {
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task => task.Alarm.HasValue;
        }
    }
}
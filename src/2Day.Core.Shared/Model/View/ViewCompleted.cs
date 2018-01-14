using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewCompleted : SystemDefinedView
    {
        public override string EmptyHeader
        {
            get { return StringResources.SystemView_Completed_EmptyHeader; }
        }

        public override string EmptyHint
        {
            get { return StringResources.SystemView_Completed_EmptyHint; }
        }

        public override bool CanReceiveTasks
        {
            get { return true; }
        }

        public override bool HasCustomCommand
        {
            get { return true; }
        }

        public ViewCompleted(IWorkbook workbook, ISystemView view)
            : base(workbook, view)
        {
        }

        public override async Task ExecuteCustomCommandAsync()
        {
            // remove completed tasks
            var messageBoxService = Ioc.Resolve<IMessageBoxService>();
            int completedTasksCount = this.Tasks.Count(t => t.IsCompleted);
            if (completedTasksCount == 0)
            {
                await messageBoxService.ShowAsync(StringResources.Message_Information, StringResources.Message_CleanupTaskNoRemove);
            }
            else
            {
                var result = await messageBoxService.ShowAsync(
                    StringResources.Dialog_TitleConfirmation,
                    string.Format(StringResources.Message_CleanupTaskConfirmationFormat, completedTasksCount),
                    DialogButton.OKCancel);

                if (result == DialogResult.OK)
                {
                    var completedTasks = this.Tasks.Where(t => t.IsCompleted).ToList();
                    foreach (var task in completedTasks)
                    {
                        task.Delete();
                    }
                }
            }
        }

        public override void MoveTasks(IEnumerable<ITask> tasks)
        {
            foreach (var task in tasks)
                task.IsCompleted = true;
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return task => task.IsCompleted;
        }
    }
}
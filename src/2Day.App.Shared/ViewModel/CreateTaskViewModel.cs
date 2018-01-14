using System;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Speech;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Task = Chartreuse.Today.Core.Shared.Model.Impl.Task;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class CreateTaskViewModel : TaskViewModelBase
    {
        public override string PageTitle
        {
            get { return StringResources.CreateTaskPage_Title; }
        }

        public override bool IsEdit
        {
            get { return false; }
        }

        public override bool CanDelete
        {
            get { return false; }
        }

        public override bool CanPin
        {
            get { return false; }
        }

        public override bool IsPinned
        {
            get { return false; }
        }
        
        public override bool CanQuickAdd
        {
            get { return true; }
        }

        protected bool NotifyAdd { get; set; }

        public CreateTaskViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, INotificationService notificationService, ISynchronizationManager synchronizationManager, ISpeechService speechService, ITrackingManager trackingManager, IPlatformService platformService) 
            : base(workbook, navigationService, messageBoxService, notificationService, synchronizationManager, speechService, trackingManager, platformService)
        {
            this.TargetFolder = this.Workbook.Folders.FirstOrDefault();

            this.OnTargetFolderChanged();

            this.UpdateTagSuggestions();

            this.DueDate = ModelHelper.GetDefaultDueDate(this.Workbook.Settings);
            this.StartDate = ModelHelper.GetDefaultStartDate(this.Workbook.Settings);
        }

        public void UseTaskCreationParameters(TaskCreationParameters parameters)
        {
            if (parameters.Folder != null)
                this.TargetFolder = parameters.Folder;

            if (parameters.Context != null)
                this.TargetContext = parameters.Context;

            if (parameters.Title != null)
                this.Title = parameters.Title;

            if (parameters.Priority != null)
                this.Priority = parameters.Priority.Value;

            if (parameters.Due != null)
                this.DueDate = parameters.Due;

            if (!string.IsNullOrWhiteSpace(parameters.Tag))
            {
                if (!string.IsNullOrWhiteSpace(parameters.Tag))
                    this.Tags.Add(parameters.Tag);
            }
        }
        
        protected override async void SaveContinueExecute()
        {
            await this.QuickAddAsync();
        }

        protected override void OnTargetFolderChanged()
        {
            if (!this.HasDueDateBeenSet)
            {
                // reset flags has we just updated the due date
                this.HasDueDateBeenSet = false;
            }
        }

        public override bool HasDirtyChanges()
        {
            return !string.IsNullOrEmpty(this.Title);
        }

        protected override async Task<ITask> SaveExecuteCore(bool navigateBack)
        {
            var task = new Task { Added = DateTime.Now };

            bool taskUpdate = this.UpdateTask(task);
            if (taskUpdate)
            {
                this.Title = null;
                this.Note = null;

                if (navigateBack)
                {
                    this.DueDate = null;
                    this.StartDate = null;
                    this.TargetContext = null;
                    this.Progress = null;
                    this.Tags.Clear();
                    this.ClearReminderCommand.Execute(null);
                    this.ClearFrequencyCommand.Execute(null);
                    this.Subtasks.Clear();

                    this.NavigationService.GoBack();
                }
            }

            if (this.NotifyAdd)
                this.ShowNotificationForTaskAdd(task);

            return task;
        }

        public async System.Threading.Tasks.Task QuickAddAsync()
        {
            ITask task = await this.SaveExecute(false);
            if (task != null)
            {
                this.ShowNotificationForTaskAdd(task);

                this.Title = null;
                this.Note = null;

                this.RaiseNoteCleared();
            }
        }

        private void ShowNotificationForTaskAdd(ITask task)
        {
            // defensive check: even if that's case is not supposed to happen, we've already seen it
            if (!string.IsNullOrWhiteSpace(task.Title) && task.Folder != null)
            {
                if (task.Due.HasValue)
                    this.NotificationService.ShowNotification(string.Format(StringResources.Notification_NewTaskCreatedWithDueDateFormat, task.Title, task.Due.FormatLong(false)));
                else
                    this.NotificationService.ShowNotification(string.Format(StringResources.Notification_NewTaskCreatedNoDueDateFormat, task.Title, task.Folder.Name));
            }
        }
    }
}

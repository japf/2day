using System;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Speech;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class EditTaskViewModel : TaskViewModelBase
    {
        private ITask task;
        private readonly ITileManager tileManager;

        public override string PageTitle
        {
            get { return StringResources.EditTaskPage_Title; }
        }

        public override bool IsEdit
        {
            get { return true; }
        }

        public override bool CanDelete
        {
            get { return true; }
        }

        public override bool CanPin
        {
            get { return true; }
        }

        public override bool IsPinned
        {
            get { return this.tileManager.IsPinned(this.task); }
        }

        public override string PinDescription
        {
            get
            {
                if (this.IsPinned)
                    return StringResources.AppMenu_ContextMenu_Folder_Unpin;
                else
                    return StringResources.AppMenu_ContextMenu_Folder_Pin;
            }
        }

        public override bool ShowUpdateDescription
        {
            get { return true; }
        }

        public override string CreationDescription
        {
            get { return string.Format("{0}: {1}", StringResources.Task_CreationDate, this.task.Added.FormatLong(true)); }    
        }

        public override string ModificationDescription
        {
            get { return string.Format("{0}: {1}", StringResources.Task_ModificationDate, this.task.Modified.FormatLong(true)); }
        }

        public ITask EditedTask
        {
            get { return this.task; }
        }

        public EditTaskViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, INotificationService notificationService, ITileManager tileManager, ISynchronizationManager synchronizationManager, ISpeechService speechService, ITrackingManager trackingManager, IPlatformService platformService)
            : base(workbook, navigationService, messageBoxService, notificationService, synchronizationManager, speechService, trackingManager, platformService)
        {
            if (tileManager == null)
                throw new ArgumentNullException(nameof(tileManager));

            this.tileManager = tileManager;
        }

        public void LoadTask(ITask task)
        {
            this.task = task;

            this.Title = task.Title;
            this.Note = task.Note;
            this.Priority = task.Priority;
            this.TargetFolder = task.Folder;
            this.TargetContext = task.Context;
            this.DueDate = task.Due;

            if (task.Start.HasValue)
            {
                this.StartDate = task.Start.Value.Date;
                this.StartTime = task.Start.Value.TimeOfDay;
            }
            else
            {
                this.StartDate = null;
                this.StartTime = null;
            }

            if (task.Alarm.HasValue)
            {
                this.ReminderDate = task.Alarm.Value.Date;
                this.ReminderTime = task.Alarm.Value.TimeOfDay;
            }
            else
            {
                this.ReminderDate = null;
                this.ReminderTime = null;
            }

            this.Progress = task.Progress;

            this.IsCompleted = task.IsCompleted;

            this.Tags.Clear();
            this.Tags.AddRange(task.ReadTags());

            // selectedFrequency must map a frequency object contained in the frequencies list
            if (task.CustomFrequency != null && task.FrequencyType.HasValue)
            {
                var frequency = this.Frequencies.FirstOrDefault(f => f.CustomFrequency.FrequencyType == task.CustomFrequency.FrequencyType);
                if (frequency == null)
                {
                    // create a new instance of ICustomFrequency that will be edited and applied back to the task if user save its changes
                    ICustomFrequency customFrequency = FrequencyFactory.GetCustomFrequency<ICustomFrequency>(task.FrequencyType.Value, task.CustomFrequency.Value);
                    frequency = new Frequency(customFrequency);
                }

                this.SelectedFrequency = frequency;
                this.SelectedFrequency.UseFixedDate = task.UseFixedDate;
            }
            else
            {
                this.SelectedFrequency = null;
            }

            this.Subtasks.Clear();
            foreach (var subtask  in task.Children)
            {
                // use a copy of the actual subtask so that we can revert changes
                this.Subtasks.Add(new Core.Shared.Model.Impl.Task
                {
                    Id = subtask.Id,
                    Title = subtask.Title,
                    IsCompleted = subtask.IsCompleted
                });
            }

            this.UpdateTagSuggestions();

            this.RaisePropertyChanged("HasCallAction");
            this.RaisePropertyChanged("PhoneNumber");
        }
        
        protected override async void PinExecute()
        {
            if (this.IsPinned)
                await this.tileManager.UnpinAsync(this.task);
            else
                await this.tileManager.PinAsync(this.task);

            this.RaisePropertyChanged("IsPinned");
            this.RaisePropertyChanged("PinDescription");
        }

        public override bool HasDirtyChanges()
        {
            return !this.task.IsEqualsTo(
                this.Title, 
                this.Note,
                ModelHelper.GetTags(this.Tags),
                this.DueDate, this.Start, this.Reminder,
                this.SelectedFrequency != null ? this.SelectedFrequency.CustomFrequency : null,
                this.SelectedFrequency != null ? this.SelectedFrequency.UseFixedDate : false,
                this.Priority,
                this.Progress,
                this.IsCompleted,
                this.task.Action,       // action, ignored for now
                this.task.ActionName,   // action name, ignored for now 
                this.task.ActionValue,  // action value, ignored for now
                this.TargetContext,
                this.TargetFolder,
                this.Subtasks);
        }

        protected override void OnProgressChanged()
        {
            if (!this.IsCompleted)
            {
                if (this.Progress == 1.0)
                {
                    this.IsCompleted = true;
                }
            }
            else
            {
                if (this.Progress != 1.0)
                {
                    this.IsCompleted = false;
                }
            }
        }

        protected override Task<ITask> SaveExecuteCore(bool navigateBack)
        {
            if (this.UpdateTask(this.task) && navigateBack)
            {
                this.NavigationService.GoBack();
                return Task.FromResult(this.task);
            }

            return null;
        }

        protected override async void DeleteExecute()
        {
            var result = await this.MessageBoxService.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_DeleteTask, DialogButton.YesNo);
            if (result == DialogResult.Yes)
            {
                this.task.Delete();
                this.NavigationService.GoBack();   
            }            
        }
    }
}
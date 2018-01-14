using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Speech;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Task = System.Threading.Tasks.Task;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public abstract class TaskViewModelBase : PageViewModelBase
    {
        private static readonly string[] emptyStringArray = new string[0];

        private readonly IMessageBoxService messageBoxService;
        private readonly INotificationService notificationService;
        private readonly ISynchronizationManager synchronizationManager;
        private readonly ISpeechService speechService;
        private readonly ITrackingManager trackingManager;
        private readonly IPlatformService platformService;

        private readonly SyncPrioritySupport syncPrioritySupport;

        private readonly ICommand addFolderCommand;
        private readonly ICommand addContextCommand;
        private readonly ICommand clearDueDateCommand;
        private readonly ICommand clearStartDateCommand;
        private readonly ICommand clearReminderCommand;
        private readonly ICommand clearProgressCommand;
        private readonly ICommand clearContextCommand;
        private readonly ICommand clearFrequencyCommand;
        private readonly ICommand addTagCommand;
        private readonly ICommand deleteTagCommand;
        private readonly ICommand openLinkCommand;
        private readonly ICommand startSpeechTitleCommand;
        private readonly ICommand startSpeechNoteCommand;
        private readonly ICommand quickAddCommand;
        private readonly ICommand editPreviousCommand;
        private readonly ICommand editNextCommand;
        private readonly ICommand callPhoneNumberCommand;
        private readonly ICommand addSubtaskCommand;
        private readonly ICommand deleteSubtaskCommand;
        private readonly ICommand openNotesCommand;

        private readonly ObservableCollection<string> tags;
        private readonly ObservableCollection<string> possibleTags;
        private readonly ObservableCollection<ItemCountViewModel> availableTags;

        private readonly ObservableCollection<ITask> subtasks;

        private string title;
        private bool isCompleted;
        private string note;
        private bool hasHtmlNote;
        private TaskPriority priority;
        private IFolder targetFolder;
        private IContext targetContext;
        private DateTime? dueDate;

        private string subtaskTitle;

        protected bool HasDueDateBeenSet;
        
        private DateTime? startDate;
        private TimeSpan? startTime;

        private DateTime? reminderDate;
        private TimeSpan? reminderTime;

        private double? progress;

        private readonly ObservableCollection<Frequency> frequencies;
        private readonly int defaultFrequenciesCount;
        private Frequency selectedFrequency;

        private string currentTag;
        private string phoneNumber;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.RaisePropertyChanged("Title");
                    this.RaisePropertyChanged("HasLink");
                    this.RaisePropertyChanged("HasCallAction");
                }
            }
        }

        public string SubtaskTitle
        {
            get { return this.subtaskTitle; }
            set
            {
                this.subtaskTitle = value;
                this.RaisePropertyChanged("SubtaskTitle");
            }
        }

        public bool IsCompleted
        {
            get
            {
                return this.isCompleted;
            }
            set
            {
                if (this.isCompleted != value)
                {
                    this.isCompleted = value;
                    this.RaisePropertyChanged("IsCompleted");

                    if (this.Workbook.Settings.GetValue<bool>(CoreSettings.CompleteTaskSetProgress))
                    {
                        if (this.isCompleted)
                            this.Progress = 1;
                        else
                            this.Progress = 0;
                    }

                    if (this.subtasks.Count > 0)
                    {
                        foreach (var subtask in this.subtasks)
                        {
                            subtask.IsCompleted = true;
                        }
                    }
                }
            }
        }

        public string Note
        {
            get
            {
                return this.note;
            }
            set
            {
                if (this.note != value)
                {
                    this.note = value;
                    this.hasHtmlNote = this.note.HasHtml();

                    this.RaisePropertyChanged("Note");
                    this.RaisePropertyChanged("HasNote");
                    this.RaisePropertyChanged("HasCallAction");
                }
            }
        }

        public bool HasSubtasks
        {
            get { return this.subtasks.Count > 0; }
        }

        public ObservableCollection<ITask> Subtasks
        {
            get { return this.subtasks; }
        }

        public bool HasNote
        {
            get { return !string.IsNullOrWhiteSpace(this.note); }
        }

        public bool HasHtmlNote
        {
            get { return this.hasHtmlNote; }
        }

        public TaskPriority Priority
        {
            get
            {
                return this.priority;
            }
            set
            {
                if (this.priority != value)
                {
                    this.priority = value;

                    this.RaisePropertyChanged("Priority");
                    this.RaisePropertyChanged("PriorityText");
                }
            }
        }

        public string PriorityText
        {
            get { return this.priority.GetDescription().ToLower(); }
        }

        public IEnumerable<IFolder> Folders
        {
            get { return this.Workbook.Folders; }
        }

        public IEnumerable<IContext> RealContexts
        {
            get
            {
                return this.Workbook.Contexts;
            }
        }

        public IEnumerable<IContext> Contexts
        {
            get
            {
                return new IContext[] { new Context { Name = " " } }.Concat(this.Workbook.Contexts);
            }
        }

        public bool HasContexts
        {
            get { return this.Workbook.Contexts.Any(); }
        }

        public IFolder TargetFolder
        {
            get
            {
                return this.targetFolder;
            }
            set
            {
                if (this.targetFolder != value)
                {
                    this.targetFolder = value;
                    this.RaisePropertyChanged("TargetFolder");

                    this.OnTargetFolderChanged();
                }
            }
        }

        public bool HasContext
        {
            get { return this.TargetContext != null; }
        }

        public IContext TargetContext
        {
            get
            {
                return this.targetContext;
            }
            set
            {
                if (this.targetContext != value)
                {
                    this.targetContext = value;
                    this.RaisePropertyChanged("HasContext");
                    this.RaisePropertyChanged("TargetContext");
                }
            }
        }

        public DateTime? StartDate
        {
            get
            {
                return this.startDate;
            }
            set
            {
                if (this.startDate != value)
                {
                    this.startDate = value;

                    this.synchronizationManager.SetStartDate(this.dueDate, this.startDate, (v) => this.DueDate = v);

                    // show a message if this is the first time the user sets a start date
                    if (!this.Workbook.Settings.GetValue<bool>(CoreSettings.HasUsedStartDate))
                    {
                        ISystemView scheduledView = this.Workbook.Views.FirstOrDefault(v => v.ViewKind == ViewKind.StartDate);
                        if (scheduledView != null && !scheduledView.IsEnabled)
                            scheduledView.IsEnabled = true;

                        this.messageBoxService.ShowAsync(StringResources.StartDate_FirstTimeTitle, StringResources.StartDate_FirstTimeMessage);
                        this.Workbook.Settings.SetValue(CoreSettings.HasUsedStartDate, true);
                    }

                    this.RaisePropertyChanged("StartDate");
                    this.RaisePropertyChanged("StartDateLong");
                    this.RaisePropertyChanged("HasStartDate");
                }
            }
        }

        public TimeSpan? StartTime
        {
            get
            {
                return this.startTime;
            }
            set
            {
                if (this.startTime != value)
                {
                    this.startTime = value;
                    if (!this.startDate.HasValue && value != null)
                        this.StartDate = DateTime.Now.Date;

                    this.RaisePropertyChanged("StartTime");
                    this.RaisePropertyChanged("StartDateLong");
                    this.RaisePropertyChanged("HasStartTime");
                }
            }
        }

        public bool HasStartTime
        {
            get { return this.startTime.HasValue; }
        }

        public DateTime? Start
        {
            get
            {
                DateTime value;
                if (!this.StartDate.HasValue)
                    return null;

                value = this.StartDate.Value;
                if (this.StartTime.HasValue)
                    return value.Add(this.StartTime.Value);

                return value;
            }
        }

        public bool HasStartDate
        {
            get
            {
                return this.startDate.HasValue;
            }
            set
            {
                this.platformService.DispatchActionIdleAsync(() => this.RaisePropertyChanged("HasStartDate"));
            }
        }

        public string StartDateLong
        {
            get
            {
                if (this.startDate.HasValue)
                {
                    if (this.startTime.HasValue)
                        return this.startDate.Value.Add(this.startTime.Value).FormatLong(true);
                    else
                        return this.startDate.FormatLong(true);
                }

                return string.Empty;
            }
        }

        public DateTime? ReminderDate
        {
            get
            {
                return this.reminderDate;
            }
            set
            {
                if (this.reminderDate != value)
                {
                    this.reminderDate = value;

                    this.RaisePropertyChanged("ReminderDate");
                    this.RaisePropertyChanged("ReminderLong");
                    this.RaisePropertyChanged("HasReminder");
                }
            }
        }

        public TimeSpan? ReminderTime
        {
            get
            {
                return this.reminderTime;
            }
            set
            {
                if (this.reminderTime != value)
                {
                    this.reminderTime = value;
                    if (!this.reminderDate.HasValue)
                        this.ReminderDate = DateTime.Now.Date;

                    this.RaisePropertyChanged("ReminderTime");
                    this.RaisePropertyChanged("ReminderLong");
                    this.RaisePropertyChanged("HasReminderTime");
                    this.RaisePropertyChanged("HasReminder");
                }
            }
        }

        public bool HasReminderTime
        {
            get { return this.reminderTime.HasValue; }
        }

        public DateTime? Reminder
        {
            get
            {
                DateTime value;
                if (!this.ReminderDate.HasValue)
                    return null;

                value = this.ReminderDate.Value;
                if (this.ReminderTime.HasValue)
                    return value.Add(this.ReminderTime.Value);

                return value;
            }
        }

        public bool HasReminder
        {
            get
            {
                return this.reminderDate.HasValue;
            }
            set
            {
                this.platformService.DispatchActionIdleAsync(() => this.RaisePropertyChanged("HasReminder"));
            }
        }

        public string ReminderLong
        {
            get
            {
                if (this.reminderDate.HasValue)
                {
                    if (this.reminderTime.HasValue)
                        return this.reminderDate.Value.Add(this.reminderTime.Value).FormatLong(true);
                    else
                        return this.reminderDate.FormatLong(true);
                }

                return string.Empty;
            }
        }

        public DateTime? DueDate
        {
            get
            {
                return this.dueDate;
            }
            set
            {
                if (this.dueDate != value)
                {
                    this.dueDate = value;
                    this.HasDueDateBeenSet = true;

                    if (this.selectedFrequency != null)
                    {
                        this.selectedFrequency.CustomFrequency.SetReferenceDate(this.dueDate);
                    }

                    this.synchronizationManager.SetDueDate(this.dueDate, this.Start, v =>
                    {
                        this.StartDate = v;
                        this.StartTime = v?.TimeOfDay;
                    });

                    this.RaisePropertyChanged("DueDate");
                    this.RaisePropertyChanged("DueDateLong");
                    this.RaisePropertyChanged("HasNoDueDate");
                    this.RaisePropertyChanged("HasDueDate");
                    this.RaisePropertyChanged("IsDueDateUndefined");
                    this.RaisePropertyChanged("IsDueDateToday");
                    this.RaisePropertyChanged("IsDueDateTomorrow");
                    this.RaisePropertyChanged("IsDueDateCustom");
                    this.RaisePropertyChanged("FrequencyText");
                }
            }
        }

        public string DueDateLong
        {
            get { return this.dueDate.FormatLong(false); }
        }

        public bool HasDueDate
        {
            get { return !this.HasNoDueDate; }
        }

        public bool HasNoDueDate
        {
            get
            {
                return !this.dueDate.HasValue;
            }
            set
            {
                if (value)
                    this.DueDate = null;
                else
                    this.DueDate = DateTime.Now;

                this.RaisePropertyChanged("DueDate");
                this.RaisePropertyChanged("HasNoDueDate");
                this.RaisePropertyChanged("HasDueDate");
            }
        }

        public bool IsDueDateUndefined
        {
            get
            {
                return !this.dueDate.HasValue;
            }
            set
            {
                if (value)
                    this.DueDate = null;
            }
        }

        public bool IsDueDateToday
        {
            get
            {
                return this.dueDate.HasValue && this.dueDate.Value.Date == DateTime.Now.Date;
            }
            set
            {
                if (value)
                    this.DueDate = DateTime.Now;
            }
        }

        public bool IsDueDateTomorrow
        {
            get
            {
                return this.dueDate.HasValue && this.dueDate.Value.Date == DateTime.Now.Date.AddDays(1);
            }
            set
            {
                if (value)
                    this.DueDate = DateTime.Now.AddDays(1);
            }
        }

        public bool IsDueDateCustom
        {
            get { return this.dueDate.HasValue && !this.IsDueDateToday && !this.IsDueDateTomorrow; }
            set
            {
                // prevent the databound button to get the isChecked state immediately
                this.platformService.DispatchActionIdleAsync(() => this.RaisePropertyChanged("IsDueDateCustom"));
            }
        }

        public bool HasProgress
        {
            get { return this.progress.HasValue; }
        }

        public double? Progress
        {
            get
            {
                return this.progress; }
            set
            {
                if (this.progress != value)
                {
                    this.progress = value;

                    if (this.progress.HasValue)
                    {
                        // make sure the value is in a valid range
                        // ie not less than 0
                        this.progress = Math.Max(this.progress.Value, 0);
                        // and not more than 1
                        this.progress = Math.Min(this.progress.Value, 1);
                    }

                    this.OnProgressChanged();

                    this.RaisePropertyChanged("Progress");
                    this.RaisePropertyChanged("HasProgress");
                }
            }
        }

        public string CurrentTag
        {
            get
            {
                return this.currentTag;
            }
            set
            {
                if (this.currentTag != value)
                {
                    this.currentTag = value;
                    this.RaisePropertyChanged("CurrentTag");
                }
            }
        }

        public ObservableCollection<string> Tags
        {
            get { return this.tags; }
        }

        public bool HasTags
        {
            get { return this.tags.Count > 0; }
        }

        public ObservableCollection<string> PossibleTags
        {
            get { return this.possibleTags; }
        }

        public bool HasLink
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Title))
                    return false;
                else
                    return this.Title.TryGetHyperlink() != null;
            }
        }

        public bool HasCallAction
        {
            get
            {
                string number = RegexHelper.FindPhoneNumber(this.platformService, this.title, this.note);

                if (this.phoneNumber != number)
                {
                    this.phoneNumber = number;
                    this.RaisePropertyChanged("HasCallAction");
                    this.RaisePropertyChanged("PhoneNumber");
                }

                return this.phoneNumber != null;
            }
        }

        public string PhoneNumber
        {
            get { return this.phoneNumber; }
        }

        #region frequency

        public IEnumerable<Frequency> Frequencies
        {
            get { return this.frequencies; }
        }

        public bool HasFrequency
        {
            get { return (this.SelectedFrequency != null && !(this.SelectedFrequency.CustomFrequency is OnceOnlyFrequency))  || this.HasCustomFrequency; }
        }

        public Frequency DisplaySelectedFrequency
        {
            get
            {
                if (this.frequencies.Contains(this.selectedFrequency))
                {
                    return this.selectedFrequency;
                }
                else
                {
                    if (this.frequencies.Count == this.defaultFrequenciesCount)
                    {
                        this.frequencies.Add(new Frequency(new CustomFrequencyDisplayOnly(this.selectedFrequency)));
                    }
                    return this.frequencies.LastOrDefault();
                }
            }
            set { this.SelectedFrequency = value; }
        }

        public Frequency SelectedFrequency
        {
            get { return this.selectedFrequency; }
            set
            {
                if (this.selectedFrequency != value)
                {
                    this.selectedFrequency = value;

                    if (this.frequencies.Contains(value) && this.frequencies.Count == this.defaultFrequenciesCount + 1)
                    {
                        this.frequencies.Remove(this.frequencies[this.frequencies.Count - 1]);
                    }

                    if (this.selectedFrequency != null && !this.selectedFrequency.IsCustom)
                    {
                        this.selectedFrequency.CustomFrequency.SetReferenceDate(this.dueDate);
                    }
                }

                // always raise property changes because settings of a frequency can change without changing
                // the frequency object itself
                this.RaisePropertyChanged("DisplaySelectedFrequency");
                this.RaisePropertyChanged("SelectedFrequency");
                this.RaisePropertyChanged("FrequencyText");
                this.RaisePropertyChanged("HasCustomFrequency");
                this.RaisePropertyChanged("HasFrequency");
            }
        }

        public string FrequencyText
        {
            get
            {
                if (this.selectedFrequency != null && this.selectedFrequency.CustomFrequency != null)
                    return this.selectedFrequency.CustomFrequency.DisplayValue.FirstLetterToLower();
                else
                    return string.Empty;
            }
        }

        public bool HasCustomFrequency
        {
            get
            {
                return this.selectedFrequency != null && this.selectedFrequency.IsCustom;
            }
            set
            {
                this.platformService.DispatchActionIdleAsync(() =>
                {
                    this.RaisePropertyChanged("HasCustomFrequency");
                    this.RaisePropertyChanged("HasFrequency");
                });
            }
        }

        public virtual bool ShowUpdateDescription
        {
            get { return false; }
        }

        public virtual string CreationDescription
        {
            get { return string.Empty; }
        }

        public virtual string ModificationDescription
        {
            get { return string.Empty; }
        }

        public SyncPrioritySupport SyncPrioritySupport
        {
            get { return this.syncPrioritySupport; }
        }

        #endregion

        public ICommand AddFolderCommand
        {
            get { return this.addFolderCommand; }
        }

        public ICommand AddContextCommand
        {
            get { return this.addContextCommand; }
        }

        public ICommand ClearDueDateCommand
        {
            get { return this.clearDueDateCommand; }
        }

        public ICommand ClearStartDateCommand
        {
            get { return this.clearStartDateCommand; }
        }

        public ICommand ClearReminderCommand
        {
            get { return this.clearReminderCommand; }
        }

        public ICommand ClearProgressCommand
        {
            get { return this.clearProgressCommand; }
        }

        public ICommand ClearContextCommand
        {
            get { return this.clearContextCommand; }
        }

        public ICommand ClearFrequencyCommand
        {
            get { return this.clearFrequencyCommand; }
        }

        public ICommand AddTagCommand
        {
            get { return this.addTagCommand; }
        }

        public ICommand DeleteTagCommand
        {
            get { return this.deleteTagCommand; }
        }

        public ICommand OpenLinkCommand
        {
            get { return this.openLinkCommand; }
        }

        public ICommand StartSpeechTitleCommand
        {
            get { return this.startSpeechTitleCommand; }
        }

        public ICommand StartSpeechNoteCommand
        {
            get { return this.startSpeechNoteCommand; }
        }

        public ICommand QuickAddCommand
        {
            get { return this.quickAddCommand; }
        }

        public ICommand EditPreviousCommand
        {
            get { return this.editPreviousCommand; }
        }

        public ICommand EditNextCommand
        {
            get { return this.editNextCommand; }
        }

        public ICommand CallPhoneNumberCommand
        {
            get { return this.callPhoneNumberCommand; }
        }

        public ICommand AddSubtaskCommand
        {
            get { return this.addSubtaskCommand; }
        }

        public ICommand DeleteSubtaskCommand
        {
            get { return this.deleteSubtaskCommand; }
        }

        public ICommand OpenNotesCommand
        {
            get { return this.openNotesCommand; }
        }

        public abstract string PageTitle
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating if the task can be completed. This is false for new task and true for edited tasks.
        /// </summary>
        public virtual bool IsEdit
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating if quick add is enabled. Quick add enables to save a task and immediately start creating a new one
        /// without a back navigation
        /// </summary>
        public virtual bool CanQuickAdd
        {
            get { return false; }
        }

        protected INotificationService NotificationService
        {
            get { return this.notificationService; }
        }

        protected IMessageBoxService MessageBoxService
        {
            get { return this.messageBoxService; }
        }

        protected IPlatformService PlatformService
        {
            get { return this.platformService; }
        }

        public virtual string PinDescription
        {
            get { return string.Empty; }
        }

        public ObservableCollection<ItemCountViewModel> AvailableTags
        {
            get { return this.availableTags; }
        }

        public event EventHandler<EventArgs<string>> NoteSpeechRecognitionCompleted;
        public event EventHandler<EventArgs> NoteCleared;

        public event EventHandler NavigatePrevious;
        public event EventHandler NavigateNext;
        public event EventHandler Disposed;
        
        protected TaskViewModelBase(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, INotificationService notificationService, ISynchronizationManager synchronizationManager, ISpeechService speechService, ITrackingManager trackingManager, IPlatformService platformService)
            : base(workbook, navigationService)
        {
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (notificationService == null)
                throw new ArgumentNullException(nameof(notificationService));
            if (synchronizationManager == null)
                throw new ArgumentNullException(nameof(synchronizationManager));
            if (speechService == null)
                throw new ArgumentNullException(nameof(speechService));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));
            if (platformService == null)
                throw new ArgumentNullException(nameof(platformService));

            this.messageBoxService = messageBoxService;
            this.notificationService = notificationService;
            this.synchronizationManager = synchronizationManager;
            this.speechService = speechService;
            this.trackingManager = trackingManager;
            this.platformService = platformService;

            this.syncPrioritySupport = this.synchronizationManager.SyncPrioritySupport;
            
            this.priority = this.Workbook.Settings.GetValue<TaskPriority>(CoreSettings.DefaultPriority);
            if (this.syncPrioritySupport == SyncPrioritySupport.LowMediumHigh && (this.priority == TaskPriority.None || this.priority == TaskPriority.Star))
                this.priority = TaskPriority.Low;

            var context = ModelHelper.GetDefaultContext(this.Workbook);
            if (context != null)
                this.TargetContext = context;

            this.dueDate = null;
            this.title = string.Empty;
            this.note = string.Empty;
            this.tags = new ObservableCollection<string>();

            this.subtasks = new ObservableCollection<ITask>();

            this.targetFolder = workbook.Folders.FirstOrDefault();

            this.frequencies = new ObservableCollection<Frequency>
                                   {
                                       new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.Once)),
                                       new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.Daily)),
                                       new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.Weekly)),
                                       new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.Monthly)),
                                       new Frequency(FrequencyFactory.GetCustomFrequency(FrequencyType.Yearly))                                       
                                   };
            this.defaultFrequenciesCount = this.frequencies.Count;

            // by default, select "once"
            this.selectedFrequency = this.frequencies.First();

            this.addFolderCommand = new RelayCommand(this.AddFolderExecute);
            this.addContextCommand = new RelayCommand(this.AddContextExecute);
            this.clearDueDateCommand = new RelayCommand(this.ClearDueDateExecute);
            this.clearStartDateCommand = new RelayCommand(this.ClearStartDateExecute);
            this.clearReminderCommand = new RelayCommand(this.ClearReminderExecute);
            this.clearProgressCommand = new RelayCommand(() => this.Progress = null);
            this.clearContextCommand = new RelayCommand(() => this.TargetContext = null);
            this.clearFrequencyCommand = new RelayCommand(() => this.SelectedFrequency = null);
            this.addTagCommand = new RelayCommand<string>(this.AddTagExecute);
            this.deleteTagCommand = new RelayCommand<string>(this.DeleteTagExecute);
            this.openLinkCommand = new RelayCommand(this.OpenLinkExecute);
            this.startSpeechTitleCommand = new RelayCommand(this.SpeechTitleExecute);
            this.startSpeechNoteCommand = new RelayCommand(this.SpeechNoteExecute);
            this.quickAddCommand = new RelayCommand(this.SaveContinueExecute);
            this.editPreviousCommand = new RelayCommand(this.EditPreviousExecute);
            this.editNextCommand = new RelayCommand(this.EditNextExecute);
            this.callPhoneNumberCommand = new RelayCommand(this.CallPhoneNumberExecute);
            this.addSubtaskCommand = new RelayCommand(this.AddSubtaskExecute);
            this.deleteSubtaskCommand = new RelayCommand<ITask>(this.DeleteSubtaskExecute);
            this.openNotesCommand = new RelayCommand(this.OpenNotesExecute);

            this.possibleTags = new ObservableCollection<string>();
            this.availableTags = new ObservableCollection<ItemCountViewModel>(); 
        }

        public override void Dispose()
        {
            if (this.Disposed != null)
                this.Disposed(this, EventArgs.Empty);            
        }

        private void OpenNotesExecute()
        {
            this.NavigationService.FlyoutTo(ViewLocator.EditNotesPage, this);
        }

        private async void CallPhoneNumberExecute()
        {
            if (!string.IsNullOrWhiteSpace(this.phoneNumber))
            {
                try
                {
                    this.trackingManager.TagEvent("Quick action", new Dictionary<string, string> { { "Type", "Phone dial"} });
                    this.platformService.ShowPhoneCallUI(this.phoneNumber, this.phoneNumber);
                }
                catch (Exception ex)
                {
                    await this.messageBoxService.ShowAsync(StringResources.Message_Warning, ex.Message);
                }
            }
        }

        protected async void EditNextExecute()
        {
            bool cancel = await this.ShouldCancelChangesAsync();
            if (!cancel)
                this.NavigateNext.Raise(this);
        }

        protected async void EditPreviousExecute()
        {
            bool cancel = await this.ShouldCancelChangesAsync();
            if (!cancel)
                this.NavigatePrevious.Raise(this);
        }

        protected void RaiseNoteCleared()
        {
            if (this.NoteCleared != null)
                this.NoteCleared(this, EventArgs.Empty);
        }

        protected virtual void SaveContinueExecute()
        {
        }

        private async void SpeechNoteExecute()
        {
            await this.SpeechExecute((text) => this.Note = text, true);
        }

        private async void SpeechTitleExecute()
        {
            await this.SpeechExecute((text) => this.Title = text, false);
        }

        private async Task SpeechExecute(Action<string> onSuccess, bool isNote)
        {
            SpeechResult result = await this.speechService.RecognizeAsync(null, null);
            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Text))
            {
                onSuccess(result.Text);
                if (isNote && this.NoteSpeechRecognitionCompleted != null)
                    this.NoteSpeechRecognitionCompleted(this, new EventArgs<string>(result.Text));
            }
            else
            {
                await this.messageBoxService.ShowAsync(
                    StringResources.Message_Warning,
                    StringResources.Speech_ErrorDuringRecognitionFormat.TryFormat(result.Text));
            }
        }

        private void AddSubtaskExecute()
        {
            if (!string.IsNullOrWhiteSpace(this.subtaskTitle))
            {
                this.subtasks.Add(new Core.Shared.Model.Impl.Task
                {
                    Title = this.subtaskTitle,                    
                    Added = DateTime.Now
                });
                this.SubtaskTitle = null;
                this.RaisePropertyChanged("HasSubtasks");
            }
        }

        private void DeleteSubtaskExecute(ITask task)
        {
            if (task != null && this.subtasks.Contains(task))
            {
                this.subtasks.Remove(task);
                this.RaisePropertyChanged("HasSubtasks");
            }
        }

        protected void UpdateTagSuggestions()
        {
            IDictionary<string, int> tagUsages = this.Workbook.GetTagsUsage();
            this.availableTags.Clear();
            foreach (var tagUsage in tagUsages)
                this.availableTags.Add(new ItemCountViewModel(tagUsage.Key, tagUsage.Value, this.AddTagCommand));

            this.possibleTags.Clear();

            var allTags = this.Workbook.Tasks
                .SelectMany(t => t.Tags != null ? t.Tags.Split(new[] { Constants.TagSeparator }) : emptyStringArray)
                .Distinct()
                .ToList();

            foreach (var possibleTag in allTags)
            {
                if (!this.Tags.Contains(possibleTag))
                    this.possibleTags.Add(possibleTag);
            }
        }

        protected override async Task<bool> CancelGoBackAsync()
        {
            return await this.ShouldCancelChangesAsync();
        }

        public async Task<bool> ShouldCancelChangesAsync()
        {
            if (this.HasDirtyChanges())
            {
                var result = await this.messageBoxService.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_LooseChangesContent, DialogButton.YesNo);
                return result != DialogResult.Yes;
            }
            else
            {
                return false;
            }
        }

        public abstract bool HasDirtyChanges();

        public void Refresh()
        {
            this.RaisePropertyChanged("Folders");
            this.RaisePropertyChanged("Contexts");
            this.RaisePropertyChanged("DueDate");
            this.RaisePropertyChanged("IsDueDateUndefined");
            this.RaisePropertyChanged("IsDueDateToday");
            this.RaisePropertyChanged("IsDueDateTomorrow");
            this.RaisePropertyChanged("IsDueDateCustom");
        }

        protected virtual void OnProgressChanged()
        {
        }

        protected bool UpdateTask(ITask task)
        {
            task.Title = this.Title;
            
            // this should never happens but...
            if (this.TargetFolder == null)
                return false;
            
            task.Folder = this.TargetFolder;

            if (this.TargetContext != null && !string.IsNullOrEmpty(this.TargetContext.Name) && this.TargetContext.Name != " ")
                task.Context = this.TargetContext;
            else
                task.Context = null;
            
            task.Note = this.Note;
            task.Priority = this.Priority;
            task.Due = this.DueDate;
            task.Progress = this.Progress;

            if (this.StartDate.HasValue)
            {
                task.Start = this.Start;                
            }
            else
            {
                task.Start = null;
            }

            if (this.ReminderDate.HasValue)
                task.Alarm = this.Reminder;
            else
                task.Alarm = null;

            if (this.selectedFrequency != null)
            {
                task.CustomFrequency = this.selectedFrequency.CustomFrequency;
                task.UseFixedDate = this.selectedFrequency.UseFixedDate;
            }
            else
            {
                task.CustomFrequency = null;
            }

            // check deleted subtasks
            foreach (var subtask in task.Children.ToList())
            {
                bool stillExists = this.subtasks.Any(t => t.Id == subtask.Id);
                if (!stillExists)
                {
                    task.RemoveChild(subtask);
                    subtask.Delete();                    
                }
            }

            // check new and updated subtasks
            foreach (var subtask in this.subtasks)
            {
                bool updated = false;
                if (subtask.Id > 0)
                {
                    var existing = task.Children.FirstOrDefault(t => t.Id == subtask.Id);
                    if (existing != null)
                    {
                        existing.Folder = this.targetFolder;
                        existing.IsCompleted = subtask.IsCompleted;
                        existing.Title = subtask.Title;

                        updated = true;
                    }
                }

                if (!updated)
                {
                    task.AddChild(subtask);
                    subtask.Folder = this.targetFolder;
                }
            }

            task.WriteTags(this.Tags);
            task.Modified = DateTime.Now;
            task.IsCompleted = this.IsCompleted;

            return true;
        }

        protected override async void SaveExecute()
        {
            await this.SaveExecute(true);
        }

        protected async Task<ITask> SaveExecute(bool navigateBack)
        {
            this.Title = this.Title.TryTrim();
            if (string.IsNullOrEmpty(this.Title))
            {
                await this.messageBoxService.ShowAsync(StringResources.Message_Warning, StringResources.Message_TitleCannotBeEmpty);
                return null;
            }
            else
            {
                if (this.SelectedFrequency != null && this.SelectedFrequency.CustomFrequency != null && this.SelectedFrequency.CustomFrequency.FrequencyType != FrequencyType.Once && !this.DueDate.HasValue && !this.StartDate.HasValue)
                {
                    this.messageBoxService.ShowAsync(StringResources.Message_Warning, StringResources.Notification_DueCannotBeNull);
                    return null;
                }
                else
                {
                    string frequency = "None";
                    if (this.SelectedFrequency != null && this.SelectedFrequency.CustomFrequency != null)
                        frequency = this.SelectedFrequency.CustomFrequency.FrequencyType.ToString();

                    Dictionary<string, string> attributes = new Dictionary<string, string>()
                    {
                        { "DueDate", this.DueDate.HasValue.ToString() },
                        { "Priority", this.Priority.ToString() },
                        { "Context", (this.TargetContext != null).ToString() },
                        { "Frequency", frequency },
                        { "Reminder", this.Reminder.HasValue.ToString() },
                        { "StartDate", this.StartDate.HasValue.ToString() },
                        { "Progress", this.Progress.HasValue.ToString() },
                        { "Tags", (this.Tags.Count > 0).ToString() },
                        { "Notes", (!string.IsNullOrWhiteSpace(this.Note)).ToString() },
                    };

                    if (this.Reminder.HasValue)
                        attributes.Add("ReminderTimeMod5", (this.Reminder.Value.TimeOfDay.Minutes % 5 == 0).ToString());
                    if (this.Start.HasValue)
                        attributes.Add("StartTimeMod5", (this.Start.Value.TimeOfDay.Minutes % 5 == 0).ToString());

                    string eventName = "Edit Task";
                    if (this is CreateTaskViewModel)
                        eventName = "Add task";
                    if (this is QuickAddTaskViewModel)
                        eventName = "Quick add task";

                    this.trackingManager.TagEvent(eventName, attributes);

                    return await this.SaveExecuteCore(navigateBack);                    
                }
            }
        }

        protected abstract Task<ITask> SaveExecuteCore(bool navigateBack);

        private void AddFolderExecute()
        {
            this.NavigationService.FlyoutTo(ViewLocator.CreateEditFolderPage);
        }

        private async void AddContextExecute()
        {
            string newName = await this.messageBoxService.ShowCustomTextEditDialogAsync(StringResources.AddContext_Title, StringResources.EditContext_Placeholder);
            if (!string.IsNullOrEmpty(newName))
            {
                var context = this.Workbook.AddContext(newName);
                if (context != null)
                {
                    this.RaisePropertyChanged("Contexts");
                    this.RaisePropertyChanged("HasContexts");
                    this.TargetContext = context;
                }
            }
        }

        private void ClearDueDateExecute()
        {
            this.DueDate = null;
        }

        private void ClearStartDateExecute()
        {
            this.StartDate = null;
            this.StartTime = null;
        }

        private void ClearReminderExecute()
        {
            this.ReminderTime = null;
            this.ReminderDate = null;
        }

        private async void AddTagExecute(string parameter)
        {
            if (!string.IsNullOrEmpty(parameter))
                this.CurrentTag = parameter;
            else
                parameter = this.CurrentTag;

            if (this.CurrentTag != null)
                this.CurrentTag = parameter.TryTrim();

            if (string.IsNullOrWhiteSpace(this.currentTag))
                return;

            if (this.tags.FirstOrDefault(t => t.Equals(this.currentTag, StringComparison.OrdinalIgnoreCase)) != null)
            {
                await this.messageBoxService.ShowAsync(StringResources.Message_Warning, StringResources.CreateEditTask_TagAlreadyExists, DialogButton.OK);
            }
            else
            {
                this.tags.Add(this.currentTag);
                this.RaisePropertyChanged("HasTags");

                this.CurrentTag = string.Empty;

                this.UpdateTagSuggestions();
            }
        }

        private void DeleteTagExecute(string tag)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                this.tags.Remove(tag);
                this.RaisePropertyChanged("HasTags");

                this.UpdateTagSuggestions();
            }
        }

        private void OpenLinkExecute()
        {
            if (!string.IsNullOrEmpty(this.Title))
            {
                string uri = this.Title.TryGetHyperlink();
                if (!string.IsNullOrEmpty(uri))
                {
                    Ioc.Resolve<IPlatformService>().OpenWebUri(uri);
                }
            }
        }

        protected virtual void OnTargetFolderChanged()
        {
        }

        public void EvaluateHtmlNote()
        {
            this.RaisePropertyChanged("HasHtmlNote");
        }
    }
}
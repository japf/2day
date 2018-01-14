using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.LaunchArguments;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class GeneralSettingsPageViewModel : PageViewModelBase
    {
        private readonly ITileManager tileManager;

        private readonly List<IContext> defaultContextChoices;
        private readonly List<IAbstractFolder> badgeValues;

        private string selectedAutoDelete;
        private string selectedDefaultPriority;
        private IContext selectedDefaultContext;
        private string selectedDefaultDueDate;
        private string selectedDefaultStartDate;
        private string selectedCompletedTaskMode;

        private IAbstractFolder selectedBadgeValue;
        private readonly IAbstractFolder originalSelectedBadgeValue;

        private bool useGroupedDates;
        private bool showNoDueWithOther;
        private bool showFutureStartDates;
        private bool autoDeleteTags;
        private bool completeTaskSetsProgress;

        public IEnumerable<string> AutoDeleteChoices
        {
            get { return AutoDeleteFrequencyConverter.Descriptions; }
        }

        public string SelectedAutoDelete
        {
            get
            {
                return this.selectedAutoDelete;
            }
            set
            {
                if (this.selectedAutoDelete != value)
                {
                    this.selectedAutoDelete = value;
                    this.RaisePropertyChanged("SelectedAutoDelete");
                }
            }
        }

        public IEnumerable<string> DefaultPriorityChoices
        {
            get { return TaskPriorityConverter.Descriptions; }
        }

        public string SelectedDefaultPriority
        {
            get
            {
                return this.selectedDefaultPriority;
            }
            set
            {
                if (this.selectedDefaultPriority != value)
                {
                    this.selectedDefaultPriority = value;
                    this.RaisePropertyChanged("SelectedDefaultPriority");
                }
            }
        }

        public IEnumerable<IContext> DefaultContextChoices
        {
            get { return this.defaultContextChoices; }
        }

        public IContext SelectedDefaultContext
        {
            get
            {
                return this.selectedDefaultContext;
            }
            set
            {
                if (this.selectedDefaultContext != value)
                {
                    this.selectedDefaultContext = value;
                    this.RaisePropertyChanged("SelectedDefaultContext");
                }
            }
        }

        public IEnumerable<IAbstractFolder> BadgeValues
        {
            get { return this.badgeValues; }
        }

        public IAbstractFolder SelectedBadgeValue
        {
            get
            {
                return this.selectedBadgeValue;
            }
            set
            {
                if (this.selectedBadgeValue != value)
                {
                    this.selectedBadgeValue = value;
                    this.RaisePropertyChanged("SelectedBadgeValue");
                }
            }
        }

        public IEnumerable<string> DefaultDueDates
        {
            get { return DefaultDateConverter.Descriptions; }
        }

        public string SelectedDefaultDueDate
        {
            get
            {
                return this.selectedDefaultDueDate;
            }
            set
            {
                if (this.selectedDefaultDueDate != value)
                {
                    this.selectedDefaultDueDate = value;
                    this.RaisePropertyChanged("SelectedDefaultDueDate");
                }
            }
        }

        public IEnumerable<string> DefaultStartDates
        {
            get { return DefaultDateConverter.Descriptions; }
        }

        public string SelectedDefaultStartDate
        {
            get
            {
                return this.selectedDefaultStartDate;
            }
            set
            {
                if (this.selectedDefaultStartDate != value)
                {
                    this.selectedDefaultStartDate = value;
                    this.RaisePropertyChanged("SelectedDefaultStartDate");
                }
            }
        }

        public IEnumerable<string> FirstDaysOfTheWeek
        {
            get { return DateTimeExtensions.DaysOfTheWeek; }
        }

        public string SelectedFirstDaysOfTheWeek
        {
            get
            {
                return DateTimeExtensions.GetDayOfWeekString(this.Settings.GetValue<DayOfWeek>(CoreSettings.FirstDayOfWeek));
            }
            set
            {
                if (this.SelectedFirstDaysOfTheWeek != value)
                {
                    this.Settings.SetValue(CoreSettings.FirstDayOfWeek, DateTimeExtensions.GetDayOfWeek(value));
                }
            }
        }
        
        public IEnumerable<string> CompletedTaskModes
        {
            get { return CompletedTaskModeConverter.Descriptions; }
        }

        public string SelectedCompletedTaskMode
        {
            get
            {
                return this.selectedCompletedTaskMode;
            }
            set
            {
                if (this.selectedCompletedTaskMode != value)
                {
                    this.selectedCompletedTaskMode = value;
                    this.RaisePropertyChanged("SelectedCompletedTaskMode");
                }
            }
        }

        public bool UseGroupedDates
        {
            get { return this.useGroupedDates; }
            set
            {
                if (this.useGroupedDates != value)
                {
                    this.useGroupedDates = value;
                    this.RaisePropertyChanged("UseGroupedDates");
                }
            }
        }

        public bool ShowNoDueWithOther
        {
            get { return this.showNoDueWithOther; }
            set
            {
                if (this.showNoDueWithOther != value)
                {
                    this.showNoDueWithOther = value;
                    this.RaisePropertyChanged("ShowNoDueWithOther");
                }
            }
        }

        public bool ShowFutureStartDates
        {
            get { return this.showFutureStartDates; }
            set
            {
                if (this.showFutureStartDates != value)
                {
                    this.showFutureStartDates = value;
                    this.RaisePropertyChanged("ShowFutureStartDates");
                }
            }
        }

        public bool AutoDeleteTags
        {
            get { return this.autoDeleteTags; }
            set
            {
                if (this.autoDeleteTags != value)
                {
                    this.autoDeleteTags = value;
                    this.RaisePropertyChanged("AutoDeleteTags");
                }
            }
        }

        public bool CompleteTaskSetsProgress
        {
            get { return this.completeTaskSetsProgress; }
            set
            {
                if (this.completeTaskSetsProgress != value)
                {
                    this.completeTaskSetsProgress = value;
                    this.RaisePropertyChanged("CompleteTaskSetsProgress");
                }
            }
        }

        public bool EnableQuickAddTile
        {
            get
            {
                return this.tileManager.IsQuickAddTileEnabled;
            }
            set
            {
                if (this.tileManager.IsQuickAddTileEnabled != value)
                {
                    if (value)
                        this.tileManager.PinQuickAdd();
                    else
                        this.tileManager.UnpinQuickAdd();

                    this.RaisePropertyChanged("EnableQuickAddTile");
                }
            }
        }

        public GeneralSettingsPageViewModel(IWorkbook workbook, INavigationService navigationService, ITileManager tileManager) 
            : base(workbook, navigationService)
        {
            if (tileManager == null)
                throw new ArgumentNullException(nameof(tileManager));

            this.tileManager = tileManager;
            this.defaultContextChoices = new List<IContext>(this.Workbook.Contexts);
            this.defaultContextChoices.Insert(0, null);

            this.badgeValues = new List<IAbstractFolder> { new Folder { Name = StringResources.Settings_NoBadgeValue.ToLowerInvariant() } };
            this.badgeValues.AddRange(this.Workbook.Views);
            this.badgeValues.AddRange(this.Workbook.SmartViews);
            this.badgeValues.AddRange(this.Workbook.Folders);
            this.badgeValues.AddRange(this.Workbook.Contexts);
            this.badgeValues.AddRange(this.Workbook.Tags);
            
            AutoDeleteFrequency frequency = this.Settings.GetValue<AutoDeleteFrequency>(CoreSettings.AutoDeleteFrequency);
            this.selectedAutoDelete       = frequency.GetDescription();

            TaskPriority priority = this.Settings.GetValue<TaskPriority>(CoreSettings.DefaultPriority);
            this.selectedDefaultPriority = priority.GetDescription();

            string badgeValue = this.Settings.GetValue<string>(CoreSettings.BadgeValue);
            if (!string.IsNullOrWhiteSpace(badgeValue))
            {
                var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.Workbook, badgeValue);
                if (descriptor != null && descriptor.Folder != null)
                {
                    this.originalSelectedBadgeValue = descriptor.Folder;
                    this.selectedBadgeValue = descriptor.Folder;
                }
            }

            if (this.selectedBadgeValue == null)
            {
                this.selectedBadgeValue = this.badgeValues[0];
            }

            int contextId = this.Settings.GetValue<int>(CoreSettings.DefaultContext);
            IContext defaultContext = this.Workbook.Contexts.FirstOrDefault(c => c.Id == contextId);
            if (defaultContext != null)
                this.selectedDefaultContext = defaultContext;

            DefaultDate defaultDueDate = this.Settings.GetValue<DefaultDate>(CoreSettings.DefaultDueDate);
            this.selectedDefaultDueDate = defaultDueDate.GetDescription();

            DefaultDate defaultStartDate = this.Settings.GetValue<DefaultDate>(CoreSettings.DefaultStartDate);
            this.selectedDefaultStartDate = defaultStartDate.GetDescription();

            CompletedTaskMode completedTaskMode = this.Settings.GetValue<CompletedTaskMode>(CoreSettings.CompletedTasksMode);
            this.selectedCompletedTaskMode = completedTaskMode.GetDescription();

            this.useGroupedDates          = this.Settings.GetValue<bool>(CoreSettings.UseGroupedDates);
            this.showNoDueWithOther       = this.Settings.GetValue<bool>(CoreSettings.IncludeNoDateInViews);
            this.showFutureStartDates     = this.Settings.GetValue<bool>(CoreSettings.ShowFutureStartDates);
            this.autoDeleteTags           = this.Settings.GetValue<bool>(CoreSettings.AutoDeleteTags);
            this.completeTaskSetsProgress = this.Settings.GetValue<bool>(CoreSettings.CompleteTaskSetProgress);           
        }

        public override void Dispose()
        {
            this.Settings.SetValue(CoreSettings.AutoDeleteFrequency         , AutoDeleteFrequencyConverter.FromDescription(this.selectedAutoDelete));
            this.Settings.SetValue(CoreSettings.DefaultPriority             , TaskPriorityConverter.FromDescription(this.selectedDefaultPriority));
            this.Settings.SetValue(CoreSettings.DefaultDueDate              , DefaultDateConverter.FromDescription(this.selectedDefaultDueDate));
            this.Settings.SetValue(CoreSettings.DefaultStartDate            , DefaultDateConverter.FromDescription(this.selectedDefaultStartDate));
            this.Settings.SetValue(CoreSettings.CompletedTasksMode          , CompletedTaskModeConverter.FromDescription(this.selectedCompletedTaskMode));
            this.Settings.SetValue(CoreSettings.UseGroupedDates             , this.useGroupedDates);
            this.Settings.SetValue(CoreSettings.ShowFutureStartDates        , this.showFutureStartDates);
            this.Settings.SetValue(CoreSettings.IncludeNoDateInViews        , this.showNoDueWithOther);
            this.Settings.SetValue(CoreSettings.AutoDeleteTags              , this.autoDeleteTags);
            this.Settings.SetValue(CoreSettings.CompleteTaskSetProgress     , this.completeTaskSetsProgress);

            if (this.selectedDefaultContext != null)
                this.Settings.SetValue(CoreSettings.DefaultContext, this.selectedDefaultContext.Id);
            else
                this.Settings.SetValue(CoreSettings.DefaultContext, -1);

            if (this.selectedBadgeValue != this.originalSelectedBadgeValue)
            {
                string settingValue = null;
                if (this.selectedBadgeValue != this.badgeValues[0])
                    settingValue = LaunchArgumentsHelper.GetArgSelectFolder(this.selectedBadgeValue);

                this.Settings.SetValue(CoreSettings.BadgeValue, settingValue);
                this.tileManager.UpdateTiles();
            }
        }
    }
}

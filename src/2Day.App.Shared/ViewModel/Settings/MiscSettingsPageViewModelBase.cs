using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public abstract class MiscSettingsPageViewModelBase : PageViewModelBase
    {
        protected readonly IMessageBoxService messageBoxService;
        private readonly INotificationService notificationService;
        protected readonly IPlatformService platformService;
        protected readonly ITrackingManager trackingManager;
        protected readonly IPersistenceLayer persistenceLayer;

        private readonly ICommand clearSearchHistoryCommand;
        private readonly ICommand deleteAllCommand;
        private readonly ICommand deleteTasksCommand;
        private readonly ICommand openWelcomeScreenCommand;
        private readonly ICommand deleteCompletedTasksCommand;
        private readonly ICommand completeTasksCommand;
        private readonly ICommand createBackupCommand;
        private readonly ICommand restoreBackupCommand;
        private readonly ICommand sortFoldersCommand;
        private readonly ICommand sortSmartViewsCommands;
        private readonly ICommand sortContextsCommand;
        private readonly ICommand sortTagsCommand;

        private readonly List<string> languages;
        private readonly string previousLanguage;

        private string selectedLanguage;
        private bool sendAnalytics;

        public ICommand ClearSearchHistoryCommand
        {
            get { return this.clearSearchHistoryCommand; }
        }

        public ICommand OpenWelcomeScreenCommand
        {
            get { return this.openWelcomeScreenCommand; }
        }

        public ICommand DeleteAllCommand
        {
            get { return this.deleteAllCommand; }
        }

        public ICommand DeleteTasksCommand
        {
            get { return this.deleteTasksCommand; }
        }

        public ICommand DeleteCompletedTasksCommand
        {
            get { return this.deleteCompletedTasksCommand; }
        }

        public ICommand CompleteTasksCommand
        {
            get { return this.completeTasksCommand; }
        }

        public ICommand CreateBackupCommand
        {
            get { return this.createBackupCommand; }
        }

        public ICommand RestoreBackupCommand
        {
            get { return this.restoreBackupCommand; }
        }

        public ICommand SortFoldersCommand
        {
            get { return this.sortFoldersCommand; }
        }

        public ICommand SortSmartViewsCommand
        {
            get { return this.sortSmartViewsCommands; }
        }

        public ICommand SortContextsCommand
        {
            get { return this.sortContextsCommand; }
        }

        public ICommand SortTagsCommand
        {
            get { return this.sortTagsCommand; }
        }

        public List<string> Languages
        {
            get { return this.languages; }
        }

        public string SelectedLanguage
        {
            get { return this.selectedLanguage; }
            set
            {
                if (this.selectedLanguage != value)
                {
                    this.selectedLanguage = value;

                    string code = null;
                    if (!string.IsNullOrWhiteSpace(this.selectedLanguage))
                        code = SupportedCultures.GetLanguageCodeFromName(this.selectedLanguage);

                    this.Workbook.Settings.SetValue(CoreSettings.OverrideLanguage, code);

                    this.RaisePropertyChanged("SelectedLanguage");
                }
            }
        }

        public bool CanChangeLanguage
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public bool SendAnalytics
        {
            get { return this.sendAnalytics; }
            set
            {
                if (this.sendAnalytics != value)
                {
                    this.sendAnalytics = value;
                    this.RaisePropertyChanged("SendAnalytics");
                }
            }
        }
        
        public string BackgroundLastStatus
        {
            get { return this.Workbook.Settings.GetValue<BackgroundExecutionStatus>(CoreSettings.BackgroundLastStatus).ToString(); }
        }

        public string BackgroundLastStartExecution
        {
            get { return this.Workbook.Settings.GetValue<DateTime>(CoreSettings.BackgroundLastStartExecution).ToString("G"); }
        }

        public string BackgroundLastEndExecution
        {
            get { return this.Workbook.Settings.GetValue<DateTime>(CoreSettings.BackgroundLastEndExecution).ToString("G"); }
        }

        public string BackgroundHasAccess
        {
            get { return this.Workbook.Settings.GetValue<bool>(CoreSettings.BackgroundAccess).ToString(); }
        }

        protected MiscSettingsPageViewModelBase(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, INotificationService notificationService, IPlatformService platformService, ITrackingManager trackingManager, IPersistenceLayer persistenceLayer) : base(workbook, navigationService)
        {
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (notificationService == null)
                throw new ArgumentNullException(nameof(notificationService));
            if (platformService == null)
                throw new ArgumentNullException(nameof(platformService));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));
            if (persistenceLayer == null)
                throw new ArgumentNullException(nameof(persistenceLayer));

            this.messageBoxService = messageBoxService;
            this.notificationService = notificationService;
            this.platformService = platformService;
            this.trackingManager = trackingManager;
            this.persistenceLayer = persistenceLayer;

            this.languages = new List<string> { " " };
            this.languages.AddRange(SupportedCultures.Languages);

            this.clearSearchHistoryCommand = new RelayCommand(this.ClearSearchHistoryExecute);
            this.openWelcomeScreenCommand = new RelayCommand(this.OpenWelcomeScreenExecute);
            this.deleteAllCommand = new RelayCommand(this.DeleteAllExecute);
            this.deleteTasksCommand = new RelayCommand(this.DeleteTasksExecute);
            this.deleteCompletedTasksCommand = new RelayCommand(this.DeleteCompletedTasksExecute);
            this.completeTasksCommand = new RelayCommand(this.CompleteTasksExecute);
            this.createBackupCommand = new RelayCommand(this.CreateBackupExecute);
            this.restoreBackupCommand = new RelayCommand(this.RestoreBackupExecute);
            this.sortFoldersCommand = new RelayCommand(() => this.SortElements(SortType.Folders));
            this.sortSmartViewsCommands = new RelayCommand(() => this.SortElements(SortType.SmartViews));
            this.sortContextsCommand = new RelayCommand(() => this.SortElements(SortType.Contexts));
            this.sortTagsCommand = new RelayCommand(() => this.SortElements(SortType.Tags));

            string language = this.Workbook.Settings.GetValue<string>(CoreSettings.OverrideLanguage);
            if (!string.IsNullOrEmpty(language))
            {
                this.selectedLanguage = SupportedCultures.GetLanguageNameFromCode(language);
                this.previousLanguage = this.selectedLanguage;
            }

            this.sendAnalytics = this.Workbook.Settings.GetValue<bool>(CoreSettings.SendAnalytics);
        }

        protected abstract void RestoreBackupExecute();

        protected abstract void CreateBackupExecute();

        private void SortElements(SortType sortType)
        {
            Action cancel = null;

            if (sortType == SortType.Folders)
            {
                var actualOrder = this.Workbook.Folders.ToList();
                var newOrder = this.Workbook.Folders.OrderBy(af => af.Name).ToList();
                cancel = () => { this.Workbook.ApplyFolderOrder(actualOrder); };
                this.Workbook.ApplyFolderOrder(newOrder);
            }
            else if (sortType == SortType.SmartViews)
            {
                var actualOrder = this.Workbook.SmartViews.ToList();
                var newOrder = this.Workbook.SmartViews.OrderBy(af => af.Name).ToList();
                cancel = () => { this.Workbook.ApplySmartViewOrder(actualOrder); };
                this.Workbook.ApplySmartViewOrder(newOrder);
            }
            else if (sortType == SortType.Contexts)
            {
                var actualOrder = this.Workbook.Contexts.ToList();
                var newOrder = this.Workbook.Contexts.OrderBy(af => af.Name).ToList();
                cancel = () => { this.Workbook.ApplyContextOrder(actualOrder); };
                this.Workbook.ApplyContextOrder(newOrder);
            }
            else if (sortType == SortType.Tags)
            {
                var actualOrder = this.Workbook.Tags.ToList();
                var newOrder = this.Workbook.Tags.OrderBy(af => af.Name).ToList();
                cancel = () => { this.Workbook.ApplyTagOrder(actualOrder); };
                this.Workbook.ApplyTagOrder(newOrder);
            }
            
            if (cancel != null)
                this.notificationService.ShowNotification("Sort completed", ToastType.Info, cancel);
        }

        public override async void Dispose()
        {
            if (this.selectedLanguage != this.previousLanguage)
            {
                bool restart = false;
                var result = await this.messageBoxService.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_RestartToApply, DialogButton.YesNo);
                if (result == DialogResult.Yes)
                    restart = true;

                if (restart)
                    await this.platformService.ExitAppAsync();
            }

            this.Workbook.Settings.SetValue(CoreSettings.SendAnalytics, this.sendAnalytics);
        }

        private void ClearSearchHistoryExecute()
        {
            ResetSearch();

            this.platformService.ClearSearchHistory();

            this.notificationService.ShowNotification(StringResources.Notification_SearchHistoryCleared);
            this.TrackAction("clear history");
        }

        private void OpenWelcomeScreenExecute()
        {
            this.NavigationService.Navigate(ViewLocator.WelcomePage);
            this.TrackAction("open welcome screen");
        }

        private async void DeleteAllExecute()
        {
            var result = await this.messageBoxService.ShowAsync(
                StringResources.Dialog_TitleConfirmation,
                StringResources.Message_DeleteAllMessage,
                DialogButton.YesNo);

            if (result == DialogResult.Yes)
            {
                ResetSearch();
                this.Workbook.RemoveAll();
                this.TrackAction("delete all");
            }
        }

        private async void CompleteTasksExecute()
        {
            var result = await this.messageBoxService.ShowAsync(
                StringResources.Dialog_TitleConfirmation,
                StringResources.Dialog_AreYouSure,
                DialogButton.YesNo);

            if (result == DialogResult.Yes)
            {
                this.Workbook.CompleteTasks();
                this.TrackAction("complete all tasks");
            }
        }

        private async void DeleteTasksExecute()
        {
            var result = await this.messageBoxService.ShowAsync(
                StringResources.Dialog_TitleConfirmation,
                StringResources.Dialog_AreYouSure,
                DialogButton.YesNo);

            if (result == DialogResult.Yes)
            {
                ResetSearch();
                this.Workbook.RemoveAllTasks();
                this.TrackAction("delete all tasks");
            }
        }

        private async void DeleteCompletedTasksExecute()
        {
            int completedTasksCount = this.Workbook.Tasks.Count(t => t.IsCompleted);
            if (completedTasksCount == 0)
            {
                await this.messageBoxService.ShowAsync(StringResources.Message_Information, StringResources.Message_CleanupTaskNoRemove);
            }
            else
            {
                var result = await this.messageBoxService.ShowAsync(
                    StringResources.Dialog_TitleConfirmation,
                    string.Format(StringResources.Message_CleanupTaskConfirmationFormat, completedTasksCount),
                    DialogButton.OKCancel);

                if (result == DialogResult.OK)
                {
                    this.Workbook.RemoveCompletedTasks();
                    this.TrackAction("complete all completed tasks");
                }
            }
        }

        private static void ResetSearch()
        {
            Ioc.Resolve<IMainPageViewModel>().SearchText = null;
        }

        private void TrackAction(string action)
        {
            this.trackingManager.TagEvent("Misc action", new Dictionary<string, string> { { "action", action } });
        }

        private enum SortType
        {
            Folders,
            SmartViews,
            Contexts,
            Tags
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Chartreuse.Today.App.Shared.ViewModel.Sync;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.View;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Speech;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Collection;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Task = System.Threading.Tasks.Task;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public abstract class MainPageViewModelBase : PageViewModelBase, IMainPageViewModel
    {
        #region fields

        protected readonly ISynchronizationManager synchronizationManager;
        protected readonly IMessageBoxService messageBoxService;
        private readonly IPlatformService platformService;
        private readonly ITileManager tileManager;
        private readonly INotificationService notificationService;
        protected readonly ITrackingManager trackingManager;
        private readonly ISpeechService speechService;

        private readonly ICommand addViewCommand;
        private readonly ICommand addSmartViewCommand;
        private readonly ICommand addFolderCommand;
        private readonly ICommand addContextCommand;

        private readonly ICommand addTaskCommand;
        private readonly ICommand syncCommand;
        private readonly ICommand openSettingsCommand;
        private readonly ICommand openDebugCommand;
        private readonly ICommand clearSearchCommand;
        private readonly ICommand speechCommand;
        private readonly ICommand quickSpeechCommand;
        private readonly ICommand printCommand;
        private readonly ICommand shareCommand;
        private readonly ICommand editCommand;

        private readonly ICommand deleteSelectionCommand;
        private readonly ICommand completeSelectionCommand;
        private readonly ICommand setPrioritySelectionCommand;
        private readonly ICommand toggleTaskCompletionCommand;

        private readonly SortableObservableCollection<MenuItemViewModel> menuItems;

        private ISelectionManager selectionManager;
        private MenuItemViewModel selectedMenuItem;
        private string searchText;

        private readonly ViewSearch viewSearch;
        private readonly FolderItemViewModel searchFolderItem;
        private MenuItemViewModel selectedMenuItemBeforeSearch;

        #endregion

        #region properties

        public ICommand AddViewCommand
        {
            get { return this.addViewCommand; }
        }

        public ICommand AddSmartViewCommand
        {
            get { return this.addSmartViewCommand; }
        }

        public ICommand AddFolderCommand
        {
            get { return this.addFolderCommand; }
        }

        public ICommand AddContextCommand
        {
            get { return this.addContextCommand; }
        }

        public ICommand AddTaskCommand
        {
            get { return this.addTaskCommand; }
        }
        
        public ICommand SyncCommand
        {
            get { return this.syncCommand; }
        }

        public ICommand OpenSettingsCommand
        {
            get { return this.openSettingsCommand; }
        }

        public ICommand OpenDebugCommand
        {
            get { return this.openDebugCommand; }
        }
        
        public ICommand DeleteSelectionCommand
        {
            get { return this.deleteSelectionCommand; }
        }

        public ICommand CompleteSelectionCommand
        {
            get { return this.completeSelectionCommand; }
        }

        public ICommand ToggleTaskCompletionCommand
        {
            get { return this.toggleTaskCompletionCommand; }
        }
        
        public ICommand SetPrioritySelectionCommand
        {
            get { return this.setPrioritySelectionCommand; }
        }

        public ICommand SpeechCommand
        {
            get { return this.speechCommand; }
        }

        public ICommand QuickSpeechCommand
        {
            get { return this.quickSpeechCommand; }
        }

        public ICommand PrintCommand
        {
            get { return this.printCommand; }
        }

        public ICommand ShareCommand
        {
            get { return this.shareCommand; }
        }

        public ICommand EditCommand
        {
            get { return this.editCommand; }
        }
        
        public ObservableCollection<MenuItemViewModel> MenuItems
        {
            get { return this.menuItems; }
        }
        
        public MenuItemViewModel SelectedMenuItem
        {
            get { return this.selectedMenuItem; }
            set
            {
                if (this.selectedMenuItem != value && value is FolderItemViewModel)
                {
                    if (this.SelectedFolderItem != null)
                        this.SelectedFolderItem.IsSelected = false;

                    if (this.SelectedFolder == this.viewSearch)
                    {
                        this.selectedMenuItemBeforeSearch = null;
                        this.searchText = null;
                        this.viewSearch.SearchText = null;
                        this.RaisePropertyChanged("SearchText");
                    }

                    this.selectedMenuItem = value;

                    if (this.SelectedFolderItem != null)
                        this.SelectedFolderItem.IsSelected = true;                        

                    this.RaisePropertyChanged("SelectedMenuItem");
                    this.RaisePropertyChanged("SelectedFolderItem");
                    this.RaisePropertyChanged("SearchDescription");
                    this.RaisePropertyChanged("EditDescription");
                }
            }
        }

        public IAbstractFolder SelectedFolder
        {
            get
            {
                return this.SelectedFolderItem != null ? this.SelectedFolderItem.Folder : null;
            }
            set
            {
                if (value != null)
                {
                    var menuItem = this.MenuItems.OfType<FolderItemViewModel>().FirstOrDefault(f => f.Folder == value);
                    if (menuItem != null)
                        this.SelectedMenuItem = menuItem;
                }
            }
        }

        public FolderItemViewModel SelectedFolderItem
        {
            get { return this.selectedMenuItem as FolderItemViewModel; }
        }

        public string SearchText
        {
            get
            {
                return this.searchText;
            }
            set
            {
                if (this.searchText != value)
                {
                    this.searchText = value;

                    this.viewSearch.SearchText = value;
                    this.viewSearch.Rebuild();

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (this.selectedMenuItemBeforeSearch == null)
                            this.selectedMenuItemBeforeSearch = this.SelectedMenuItem;

                        this.SelectedMenuItem = this.searchFolderItem;

                        this.notificationService.ShowNotification(this.SearchDescription, ToastType.Search);
                    }
                    else if (this.selectedMenuItemBeforeSearch != null)
                    {
                        this.SelectedMenuItem = this.selectedMenuItemBeforeSearch;
                        this.selectedMenuItemBeforeSearch = null;
                    }
                    
                    this.RaisePropertyChanged("SearchText");
                    this.RaisePropertyChanged("SearchDescription");
                }
            }
        }

        public string SearchDescription
        {
            get
            {
                return string.Format(StringResources.MainPage_SearchDescriptionFormat, this.SearchText);
            }
        }

        public ICommand ClearSearchCommand
        {
            get { return this.clearSearchCommand; }
        }
        
        public string AppBackgroundPattern
        {
            get
            {
                string backgroundImage = this.Workbook.Settings.GetValue<string>(CoreSettings.BackgroundImage);
                if (!string.IsNullOrEmpty(backgroundImage))
                    return backgroundImage;

                string backgroundPattern = this.Workbook.Settings.GetValue<string>(CoreSettings.BackgroundPattern);
                if (!string.IsNullOrEmpty(backgroundPattern))
                    return ResourcesLocator.BuildPatternPath(backgroundPattern, this.Workbook.Settings.GetValue<bool>(CoreSettings.UseDarkTheme));

                return null;
            }
        }

        public double AppBackgroundOpacity
        {
            get { return this.Workbook.Settings.GetValue<double>(CoreSettings.BackgroundOpacity); }
        }

        public bool ShowDebugCommand
        {
            get
            {
                return this.platformService.IsDebug;
            }
        }

        public SyncPrioritySupport SyncPrioritySupport
        {
            get { return this.synchronizationManager.SyncPrioritySupport; }
        }
        
        public string EditDescription
        {
            get
            {
                if (this.SelectedFolder is IContext)
                    return StringResources.EditContext_Title;
                else if (this.SelectedFolder is ITag)
                    return StringResources.EditTag_Title;
                else if (this.SelectedFolder is ISmartView)
                    return StringResources.SmartView_Edit;
                else if (this.SelectedFolder is IFolder)
                    return StringResources.AppBar_EditFolder;
                else
                    return StringResources.AppBar_EditView;
            }
        }

        #endregion

        protected MainPageViewModelBase(IWorkbook workbook, ISynchronizationManager synchronizationManager, IStartupManager startupManager, IMessageBoxService messageBoxService, INotificationService notificationService, INavigationService navigationService, IPlatformService platformService, ITileManager tileManager, ITrackingManager trackingManager, ISpeechService speechService) 
            : base(workbook, navigationService)
        {
            if (startupManager == null)
                throw new ArgumentNullException(nameof(startupManager));
            if (synchronizationManager == null)
                throw new ArgumentNullException(nameof(synchronizationManager));
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (platformService == null)
                throw new ArgumentNullException(nameof(platformService));
            if (notificationService == null)
                throw new ArgumentNullException(nameof(notificationService));
            if (tileManager == null)
                throw new ArgumentNullException(nameof(tileManager));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));
            if (speechService == null)
                throw new ArgumentNullException(nameof(speechService));

            this.synchronizationManager = synchronizationManager;
            this.messageBoxService = messageBoxService;
            this.notificationService = notificationService;
            this.platformService = platformService;
            this.tileManager = tileManager;
            this.trackingManager = trackingManager;
            this.speechService = speechService;

            this.synchronizationManager.OperationStarted += this.OnSyncStarted;
            this.synchronizationManager.OperationProgressChanged += this.OnSyncProgressChanged;
            this.synchronizationManager.OperationCompleted += this.OnSyncOperationCompleted;
            this.synchronizationManager.OperationFailed += this.OnSyncOperationFailed;
            this.synchronizationManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ActiveService")
                    this.RaisePropertyChanged("SyncPrioritySupport");
            };

            this.Workbook.Settings.KeyChanged += this.OnSettingsChanged;
            this.Workbook.TaskAdded += this.OnTaskAddded;
            
            this.Workbook.FoldersReordered += this.OnFolderReordered;
            this.Workbook.ContextsReordered += this.OnContextReordered;

            this.addViewCommand      = new RelayCommand(this.AddViewExecute);
            this.addSmartViewCommand = new RelayCommand(this.AddSmartViewExecute);
            this.addFolderCommand    = new RelayCommand(this.AddFolderExecute);
            this.addContextCommand   = new RelayCommand(this.AddContextExecute);
            this.addTaskCommand      = new RelayCommand(this.AddTaskExecute);
            this.syncCommand         = new RelayCommand(this.SyncExecute);
            this.openSettingsCommand = new RelayCommand(this.OpenSettingsExecute);
            this.openDebugCommand    = new RelayCommand(this.OpenDebugExecute);
            this.clearSearchCommand  = new RelayCommand(this.ClearSearchExecute);
            this.speechCommand       = new RelayCommand(this.SpeechExecute);
            this.quickSpeechCommand  = new RelayCommand(this.QuickSpeechExecute);
            this.printCommand        = new RelayCommand(this.PrintExecute);
            this.shareCommand        = new RelayCommand(this.ShareExecute);
            this.editCommand         = new RelayCommand(this.EditExecute);

            this.deleteSelectionCommand = new RelayCommand(this.DeleteSelectionExecute);
            this.completeSelectionCommand = new RelayCommand(this.CompleteSelectionExecute);
            this.toggleTaskCompletionCommand = new RelayCommand<ITask>(this.ToggleTaskCompletionExecute);
            this.setPrioritySelectionCommand = new RelayCommand<string>(this.SetPrioritySelectionExecute);

            this.menuItems = new SortableObservableCollection<MenuItemViewModel>();
            
            bool hasViews = this.Workbook.Views.Any(v => v.IsEnabled);
            bool hasSmartViews = this.Workbook.SmartViews.Any();
            bool hasFolders = this.Workbook.Folders.Any();
            bool hasContexts = this.Workbook.Contexts.Any();

            // create search view
            this.viewSearch = new ViewSearch(this.Workbook);
            this.searchFolderItem = new FolderItemViewModel(this.Workbook, this.viewSearch);

            // load views
            foreach (var view in this.Workbook.Views.Where(f => f.IsEnabled))
            {
                this.menuItems.Add(new FolderItemViewModel(workbook, view));
            }

            // load smart views
            if (this.Workbook.SmartViews.Any())
            {
                if (hasViews)
                    this.menuItems.Add(new SeparatorItemViewModel(Constants.SeparatorSmartViewId));

                foreach (var smartview in this.Workbook.SmartViews)
                    this.menuItems.Add(new FolderItemViewModel(workbook, smartview));
            }

            // load folders
            if (this.Workbook.Folders.Count > 0)
            {
                if (hasViews || hasSmartViews)
                    this.menuItems.Add(new SeparatorItemViewModel(Constants.SeparatorFolderId));

                foreach (var folder in this.Workbook.Folders)
                    this.menuItems.Add(new FolderItemViewModel(workbook, folder));
            }

            // load contexts
            if (this.Workbook.Contexts.Count > 0)
            {
                if (hasViews || hasSmartViews || hasFolders)
                    this.menuItems.Add(new SeparatorItemViewModel(Constants.SeparatorContextId));

                foreach (var context in this.Workbook.Contexts)
                    this.menuItems.Add(new FolderItemViewModel(workbook, context));
            }

            // load tags
            if (this.Workbook.Tags.Any())
            {
                if (hasViews || hasSmartViews || hasFolders || hasContexts)
                    this.menuItems.Add(new SeparatorItemViewModel(Constants.SeparatorTagId));

                foreach (var tag in this.Workbook.Tags)
                    this.menuItems.Add(new FolderItemViewModel(workbook, tag));
            }
        }
        
        protected abstract void PrintExecute();

        private void OnContextReordered(object sender, EventArgs e)
        {
            foreach (var folderItemViewModel in this.menuItems.OfType<FolderItemViewModel> ())
            {
                if (folderItemViewModel.Folder.TaskGroup == TaskGroup.Context)
                    folderItemViewModel.Rebuild();
            }
        }

        private void OnFolderReordered(object sender, EventArgs e)
        {
            foreach (var folderItemViewModel in this.menuItems.OfType<FolderItemViewModel>())
            {
                if (folderItemViewModel.Folder.TaskGroup == TaskGroup.Folder)
                    folderItemViewModel.Rebuild();
            }
        }
        
        private void OnTaskAddded(object sender, EventArgs<ITask> e)
        {
            if (e != null && e.Item != null && e.Item.IsPeriodic && e.Item.HasRecurringOrigin)
            {
                if (e.Item.Due.HasValue)
                {
                    string newDate = string.Format("{0} {1}", e.Item.Due.Value.ToString("dddd"), e.Item.Due.Value.ToString("M"));
                    this.notificationService.ShowNotification(string.Format(StringResources.Notification_NewTaskCreatedWithDueDateFormat, e.Item.Title, newDate));
                }
                else if (e.Item.Start.HasValue)
                {
                    string newDate = string.Format("{0} {1}", e.Item.Start.Value.ToString("dddd"), e.Item.Start.Value.ToString("M"));
                    this.notificationService.ShowNotification(string.Format(StringResources.Notification_NewTaskCreatedWithStartDateFormat, e.Item.Title, newDate));
                }
            }
        }
        
        public async Task ActivateAsync(ISelectionManager taskListSelectionManager)
        {
            if (taskListSelectionManager == null)
                throw new ArgumentNullException(nameof(taskListSelectionManager));

            this.selectionManager = taskListSelectionManager;

            IStartupManager startupManager = Ioc.Resolve<IStartupManager>();
            bool canPerformSync = await startupManager.HandleStartupAsync();
            if (!this.platformService.IsNetworkAvailable)
                canPerformSync = false;

            if (canPerformSync && this.synchronizationManager.CanSyncOnStartup)
            {
                await TryWarnVercorsDeprecation();
                await this.synchronizationManager.Sync();
            }
        }

        public abstract Task RefreshAsync();
        
        private async Task TryWarnVercorsDeprecation()
        {
            if (this.synchronizationManager.ActiveService == SynchronizationService.Vercors)
            {
                var result = await this.messageBoxService.ShowAsync(
                    "Warning",
                    "2Day Cloud sync will stop working after Sept. 1st 2019. Because 2Day is no longer supported, we recommend you to switch to Microsoft To-Do instead. Do you want to learn more?",
                    DialogButton.YesNo);

                if (result == DialogResult.Yes)
                {
                    this.platformService.OpenWebUri("https://www.microsoft.com/en-us/p/microsoft-to-do-list-task-reminder/9nblggh5r558");
                }
            }
        }

        private void OnSettingsChanged(object sender, SettingsKeyChanged e)
        {
            if (e.Key == CoreSettings.BackgroundPattern || e.Key == CoreSettings.BackgroundImage || e.Key == CoreSettings.UseDarkTheme)
            {
                this.RaisePropertyChanged("AppBackgroundPattern");
            }
            else if (e.Key == CoreSettings.BackgroundOpacity)
            {
                this.RaisePropertyChanged("AppBackgroundOpacity");
            }
        }

        public override void Dispose()
        {
            this.synchronizationManager.OperationStarted -= this.OnSyncStarted;
            this.synchronizationManager.OperationProgressChanged -= this.OnSyncProgressChanged;
            this.synchronizationManager.OperationCompleted -= this.OnSyncOperationCompleted;
            this.synchronizationManager.OperationFailed -= this.OnSyncOperationFailed;
        }

        private void OnSyncStarted(object sender, EventArgs e)
        {
            this.notificationService.StartAsyncOperation();            
        }

        private async void OnSyncOperationFailed(object sender, SyncFailedEventArgs e)
        {
            this.notificationService.EndAsyncOperationAsync();

            await SyncWarningChecker.HandleFailedSynced(e, this.messageBoxService, this.NavigationService);
        }

        private async void OnSyncOperationCompleted(object sender, EventArgs<string> e)
        {
            this.notificationService.EndAsyncOperationAsync(StringResources.HomePage_SyncCompletedSystrayMessage);

            // if (this.debugModeEnabled)
            //    await this.messageBoxService.ShowAsync(StringResources.Message_Information, e.Item);
        }

        private void OnSyncProgressChanged(object sender, EventArgs<string> e)
        {
            this.notificationService.ReportProgressAsyncOperation(e.Item);
        }

        private async void SyncExecute()
        {
            if (this.synchronizationManager.IsSyncConfigured)
            {
                if (!this.synchronizationManager.IsSyncRunning)
                {
                    if (ModelHelper.SoftEditPending < 1)
                    {
                        await SyncWarningChecker.CheckWarningBeforeSync(this.Workbook, this.synchronizationManager, this.messageBoxService, this.platformService);

                        // sync is configured and not currently running, we can start sync
                        await TryWarnVercorsDeprecation();
                        await this.synchronizationManager.Sync();
                    }
                }
                else
                {
                    // ask for confirmation before cancelling sync as user might have tapped the button without wanting to cancel sync
                    var result = await this.messageBoxService.ShowAsync(
                        StringResources.SyncProgress_SyncInProgress,
                        StringResources.Action_Cancel + "?",
                        DialogButton.YesNo);

                    if (result == DialogResult.Yes && this.synchronizationManager.IsSyncRunning)
                    {
                        this.synchronizationManager.Cancel();
                    }
                }
            }
            else
            {
                this.NavigationService.FlyoutTo(ViewLocator.SettingsSyncPage);
            }
        }

        private void AddViewExecute()
        {
            this.NavigationService.FlyoutTo(ViewLocator.SettingsViewsPage);
        }

        private void AddSmartViewExecute()
        {
            this.NavigationService.FlyoutTo(ViewLocator.CreateEditSmartViewPage);
        }

        private void AddFolderExecute()
        {
            int folderCount = this.Workbook.Folders.Count;

            this.NavigationService.FlyoutTo(ViewLocator.CreateEditFolderPage);

            if (this.Workbook.Folders.Count != folderCount && this.Workbook.Folders.Count > 0)
                this.SelectedFolder = this.Workbook.Folders[this.Workbook.Folders.Count - 1];
        }

        private async void AddContextExecute()
        {
            string newName = await this.messageBoxService.ShowCustomTextEditDialogAsync(StringResources.AddContext_Title, StringResources.EditContext_Placeholder);
            if (!string.IsNullOrEmpty(newName))
            {
                var context = this.Workbook.AddContext(newName);
                this.SelectedFolder = context;
            }
        }
        
        private async void AddTaskExecute()
        {
            await this.EnsureHasFolderBefore(() =>
            {
                this.NavigationService.FlyoutTo(ViewLocator.CreateEditTaskPageNew, this.SelectedFolder.GetTaskCreationParameters());
            });
        }

        private async Task EnsureHasFolderBefore(Action action)
        {
            if (this.Workbook.Folders.Count == 0)
            {
                var result = await this.messageBoxService.ShowAsync(
                    StringResources.Message_Warning,
                    StringResources.Message_CannotCreateTaskNoFolderMessage,
                    DialogButton.YesNo);

                if (result == DialogResult.Yes)
                    this.NavigationService.FlyoutTo(ViewLocator.CreateEditFolderPage);
            }
            else
            {
                action();
            }
        }
        
        private void ClearSearchExecute()
        {
            this.SearchText = string.Empty;
        }

        private async void SpeechExecute()
        {
            SpeechResult result = await this.speechService.RecognizeAsync(null, null);
            if (result.IsSuccess)
            {
                var parameter = this.SelectedFolder.GetTaskCreationParameters();
                parameter.Title = result.Text;
                this.NavigationService.FlyoutTo(ViewLocator.CreateEditTaskPageNew, parameter);
            }
        }

        private async void QuickSpeechExecute()
        {
            SpeechResult result = await this.speechService.RecognizeAsync(null, null);
            if (result.IsSuccess)
            {
                var parameter = this.SelectedFolder.GetTaskCreationParameters();
                parameter.Title = result.Text;

                this.NavigationService.FlyoutTo(ViewLocator.CreateEditTaskPageNew, parameter);
            }
            else
            {
                await this.messageBoxService.ShowAsync(
                    StringResources.Message_Warning,
                    StringResources.Speech_ErrorDuringRecognitionFormat.TryFormat(result.Text));
            }
        }

        protected abstract void ShareExecute();
        
        private async void EditExecute()
        {
            if (this.SelectedFolder is IContext)
            {
                string newName = await this.messageBoxService.ShowCustomTextEditDialogAsync(StringResources.EditContext_Title, StringResources.EditContext_Placeholder, this.SelectedFolder.Name);
                var context = (IContext)this.SelectedFolder;

                context.TryRename(this.Workbook, newName);
            }
            else if (this.SelectedFolder is ITag)
            {
                string newName = await this.messageBoxService.ShowCustomTextEditDialogAsync(StringResources.EditTag_Title, StringResources.EditContext_Placeholder, this.SelectedFolder.Name);
                var tag = (ITag)this.SelectedFolder;

                tag.TryRename(this.Workbook, newName);
            }
            else if (this.SelectedFolder is ISmartView)
            {
                this.NavigationService.FlyoutTo(ViewLocator.CreateEditSmartViewPage, this.SelectedFolder);
            }
            else if (this.SelectedFolder is IFolder)
            {
                this.NavigationService.FlyoutTo(ViewLocator.CreateEditFolderPage, this.SelectedFolder);
            }
            else
            {
                this.NavigationService.FlyoutTo(ViewLocator.CreateEditFolderPage, this.SelectedFolder);
            }
        }

        private void OpenSettingsExecute()
        {
            this.NavigationService.OpenSettings();
        }

        private void OpenDebugExecute()
        {
            //ToastHelper.ToastMessage(StringResources.MessageWelcome_Body_Trial);
            //this.Workbook.Tasks[0].Alarm = DateTime.Now.AddSeconds(2);

            this.NavigationService.Navigate(ViewLocator.DebugPage);
        }
        
        private void DeleteSelectionExecute()
        {
            this.selectionManager.DeleteSelectionAsync();
            this.selectionManager.ClearSelection();
        }

        private void ToggleTaskCompletionExecute(ITask task)
        {
            ModelHelper.SoftComplete(new[] {task});                       
        }

        private void CompleteSelectionExecute()
        {
            using (this.Workbook.WithTransaction())
            {
                ModelHelper.SoftComplete(this.selectionManager.SelectedTasks);                
            }

            this.selectionManager.ClearSelection();
        }

        private void SetPrioritySelectionExecute(string priority)
        {
            TaskPriority taskPriority = TaskPriority.None;
            switch (priority.ToLowerInvariant())
            {
                case "low":
                    taskPriority = TaskPriority.Low;
                    break;
                case "medium":
                    taskPriority = TaskPriority.Medium;
                    break;
                case "high":
                    taskPriority = TaskPriority.High;
                    break;
                case "star":
                    taskPriority = TaskPriority.Star;
                    break;
            }

            using (this.Workbook.WithTransaction())
            {
                foreach (var task in this.selectionManager.SelectedTasks)
                    task.Priority = taskPriority;
            }

            this.selectionManager.ClearSelection();
        }
    }
}

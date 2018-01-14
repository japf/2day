using System;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using Chartreuse.Today.App.Controls;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Tools.Settings;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Views
{
    public sealed partial class TaskPage : Page, ISettingsPage
    {
        private TaskViewModelBase currentViewModel;

        private readonly CreateTaskViewModel createViewModel;
        private readonly EditTaskViewModel editViewModel;

        private readonly IWorkbook workbook;

        public SettingsSizeMode Size
        {
            get { return SettingsSizeMode.Small; }
        }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        public TaskViewModelBase ViewModel
        {
            get { return this.currentViewModel; }
        }

        public TaskPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.NotesEditor.SkipDisposeOnUnload = true;

            this.BtnAddDueDate.Tapped += this.OnBtnTapped;
            this.BtnAddStartDate.Tapped += this.OnBtnTapped;
            this.BtnAddReminder.Tapped += this.OnBtnTapped;
            this.BtnAddFrequency.Tapped += this.OnBtnTapped;
            this.BtnAddProgress.Tapped += this.OnBtnTapped;
            this.BtnAddTags.Tapped += this.OnBtnTapped;
            this.BtnAddContext.Tapped += this.OnBtnTapped;
            this.BtnAddSubtask.Tapped += this.OnBtnTapped;
            this.BtnFolder.Tapped += this.OnBtnTapped;
            this.BtnPriority.Tapped += this.OnBtnTapped;

            this.Loaded += this.OnLoaded;
            this.ItemsControlEntries.SizeChanged += this.OnItemsControlEntriesSizeChanged;

            var navigationService = (NavigationService)Ioc.Resolve<INavigationService>();

            this.createViewModel = Ioc.Build<CreateTaskViewModel>();
            this.createViewModel.Disposed += (s, e) =>
            {
                // do not dispose if the TaskPage is still shown as a flyout, eg. when user close the extended task notes flyout
                var peekFlyout = navigationService.PeekFlyout();
                if (peekFlyout == null || !(peekFlyout.Content is TaskPage))
                    this.currentViewModel = null;
            };

            this.editViewModel = Ioc.Build < EditTaskViewModel>();
            this.editViewModel.Disposed += (s, e) =>
            {
                // do not dispose if the TaskPage is still shown as a flyout, eg. when user close the extended task notes flyout
                var peekFlyout = navigationService.PeekFlyout();
                if (peekFlyout == null || !(peekFlyout.Content is TaskPage))
                    this.currentViewModel = null;
            };
            this.editViewModel.NavigateNext += (s, e) => this.Navigate(forward: true);
            this.editViewModel.NavigatePrevious += (s, e) => this.Navigate(forward: false);

            this.workbook = Ioc.Resolve<IWorkbook>();

            // sync folders
            this.workbook.FolderAdded += (s, e) =>
            {
                if (this.currentViewModel != null)
                {
                    this.currentViewModel.Refresh();
                    this.currentViewModel.TargetFolder = this.workbook.Folders.LastOrDefault();
                }
            };
            this.workbook.FolderRemoved += (s, e) =>
            {
                if (this.currentViewModel != null)
                {
                    this.currentViewModel.Refresh();
                    if (this.currentViewModel.TargetFolder == e.Item)
                        this.currentViewModel.TargetFolder = this.workbook.Folders.FirstOrDefault();
                }
            };
            this.workbook.FolderChanged += (s, e) =>
            {
                if (this.currentViewModel != null)
                    this.currentViewModel.Refresh();
            };
            this.workbook.FoldersReordered += (s, e) =>
            {
                if (this.currentViewModel != null)
                    this.currentViewModel.Refresh();
            };

            // sync contexts
            this.workbook.ContextAdded += (s, e) =>
            {
                if (this.currentViewModel != null)
                {
                    this.currentViewModel.Refresh();
                    this.currentViewModel.TargetContext = this.workbook.Contexts.LastOrDefault();
                }
            };
            this.workbook.ContextRemoved += (s, e) =>
            {
                if (this.currentViewModel != null)
                {
                    this.currentViewModel.Refresh();
                    if (this.currentViewModel.TargetContext == e.Item)
                        this.currentViewModel.TargetContext = this.workbook.Contexts.FirstOrDefault();
                }
            };
            this.workbook.ContextChanged += (s, e) =>
            {
                if (this.currentViewModel != null)
                    this.currentViewModel.Refresh();
            };
            this.workbook.ContextsReordered += (s, e) =>
            {
                if (this.currentViewModel != null)
                    this.currentViewModel.Refresh();
            };
        }

        private void OnBtnTapped(object sender, TappedRoutedEventArgs e)
        {
            var button = (Button) sender;

            // remove event handler because we need this code to run only once
            button.Tapped -= this.OnBtnTapped;

            // call FindName so that the target element is lazily loaded (thanks to BtnPriorityFlyoutContent)
            this.FindName(button.Name + "FlyoutContent");

            if (button.Name == "BtnAddTags" && !ResponsiveHelper.IsUsingSmallLayout())
                this.ScrollViewerTags.MaxHeight = 250;
        }

        private void OnItemsControlEntriesSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double availableHeight = this.ScrollViewer.ActualHeight - e.NewSize.Height - this.GridQuickAdd.ActualHeight;
            double newHeight = availableHeight + 10;

            if (newHeight > this.NotesEditor.MinHeight)
                this.NotesEditor.Height = newHeight; // NotesEditor has a -10 top margin
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ((TaskViewModelBase) this.DataContext).Refresh();

            if (this.Parent is Popup)
            {
                this.root.BorderThickness = new Thickness(1, 0, 0, 0);
            }
            
            // focus the title textbox if we're in creation mode
            var textbox = TreeHelper.FindVisualChild<TextBox>(this);
            if (textbox != null && this.DataContext is CreateTaskViewModel && string.IsNullOrWhiteSpace(this.currentViewModel.Title))
                textbox.Focus(FocusState.Programmatic);
        }
        
        public void Navigate(bool forward)
        {
            if (!(this.currentViewModel is EditTaskViewModel))
                return;

            var mainViewModel = Ioc.Resolve<IMainPageViewModel>();

            ITask task = editViewModel.EditedTask;

            var tasks = mainViewModel.SelectedFolderItem.SmartCollection.Items.SelectMany(g => g).ToList();
            int index = tasks.IndexOf(task);
            int offset = forward ? 1 : -1;

            index = index + offset;
            if (index >= tasks.Count)
                index = 0;
            else if (index < 0)
                index = tasks.Count - 1;

            var newTask = tasks[index];

            editViewModel.LoadTask(newTask);

            this.NotesEditor.SetRichEditBoxContentFromViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.OnNavigatedTo(e.Parameter);
        }

        public async void OnNavigatedTo(object parameter)
        {
            this.SetRequestedTheme();

            if (this.currentViewModel != null)
            {
                bool cancel = await this.currentViewModel.ShouldCancelChangesAsync();
                if (cancel)
                    return;
            }

            if (parameter is ITask)
            {
                // edit mode
                this.currentViewModel = this.editViewModel;
                this.editViewModel.LoadTask((ITask)parameter);
            }
            else
            {
                // creation mode
                this.currentViewModel = this.createViewModel;
                if (parameter is TaskCreationParameters)
                    this.createViewModel.UseTaskCreationParameters((TaskCreationParameters)parameter);
            }

            this.DataContext = this.currentViewModel;
            this.Bindings.Update();
            this.NotesEditor.SetRichEditBoxContentFromViewModel();            
        }
        
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {            
        }

        private void OnAutoCompleteBoxTagKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                this.currentViewModel.AddTagCommand.Execute(null);
                var fe = (FrameworkElement) sender;
                var popup = fe.FindParent<Popup>();
                if (popup != null)
                    popup.IsOpen = false;
            }
        }

        private void OnTextBoxSubTaskKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                this.currentViewModel.AddSubtaskCommand.Execute(null);
                var fe = (FrameworkElement) sender;
                var popup = fe.FindParent<Popup>();
                if (popup != null)
                    popup.IsOpen = false;
            }
        }

        private void OnAutoSuggestTagQuerySubmitted(AutoSuggestBox autoSuggestBox, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion != null && !string.IsNullOrWhiteSpace(e.ChosenSuggestion as string))
                this.currentViewModel.AddTagCommand.Execute(e.ChosenSuggestion);
        }
                
        public void OpenAddSubtaskPopup()
        {
            var extendedFlyout = ExtendedFlyout.GetFlyout(this.BtnAddSubtask);
            if (extendedFlyout != null)
            {
                this.FindName(this.BtnAddSubtask.Name + "FlyoutContent");
                extendedFlyout.Show(this.BtnAddSubtask);
            }
        }

        public bool IsHandlingEscapeKey()
        {
            // this is a kind of hack do detect if the escape key just causes a flyout to close
            // if that's the case, we're saying the escape is handled so that it doesn't cause
            // the parent flyout (that displays this page) to close
            // if that's not the case, the escape key will cause the parent flyout to close normally
            double intervalMs = (DateTime.Now - AppFlyout.PopupCloseLastTime).TotalMilliseconds;
            if (intervalMs < 250)
                return true;

            return false;
        }

        private void OnButtonAddTagTapped(object sender, TappedRoutedEventArgs e)
        {
            // use a standard EventHandler instead of an attached property + a command
            // because adding a tags update the list of suggestion in the AutoSuggestBox
            // and if we use Command + AttachedProperty that causes the AutoSuggestBox
            // to get focus and that doesn't close the popup
            var button = (Button) sender;
            var popup = button.FindParent<Popup>();
            if (popup != null)
                popup.IsOpen = false;

            if (this.currentViewModel != null)
                this.currentViewModel.AddTagCommand.Execute(button.CommandParameter);
        }
    }
}
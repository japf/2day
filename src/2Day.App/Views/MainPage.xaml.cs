using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Chartreuse.Today.App.Controls;
using Chartreuse.Today.App.Manager.UI;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Tools.Export;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Groups;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Manager;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Views
{
    public sealed partial class MainPage : Page
    {
        private const int SlideAnimationMs = 200;

        private readonly MainPageViewModel viewModel;

        private readonly NavigationMenuManager navigationMenuManager;
        private readonly GridViewSelectionManager taskListSelectionManager;
        private readonly DragDropManager dragDropManager;

        private readonly IWorkbook workbook;

        private readonly INavigationService navigationService;
        private readonly IMessageBoxService messageBoxService;
        private readonly ITrackingManager trackingManager;

        private readonly ITileManager tileManager;
        private readonly IPlatformService platformService;

        private bool lastSplitViewOpen;
        private QuickNavBar quickNavBar;

        public MainPageViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        public GridViewSelectionManager SelectionManager
        {
            get { return this.taskListSelectionManager; }
        }

        public MainPage()
        {
            this.InitializeComponent();

            this.workbook = Ioc.Resolve<IWorkbook>();
            this.navigationService = Ioc.Resolve<INavigationService>();
            this.messageBoxService = Ioc.Resolve<IMessageBoxService>();
            this.trackingManager = Ioc.Resolve<ITrackingManager>();
            this.tileManager = Ioc.Resolve<ITileManager>();
            this.platformService = Ioc.Resolve<IPlatformService>();

            // setup cache mode so that this page is cached accross navigation
            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.viewModel = Ioc.Build<MainPageViewModel>();
            this.viewModel.PropertyChanged += this.OnViewModelPropertyChanged;
            Ioc.RegisterInstance<IMainPageViewModel, MainPageViewModel>(this.viewModel);

            this.navigationMenuManager = Ioc.Build<NavigationMenuManager>();
            this.taskListSelectionManager = new GridViewSelectionManager(this.workbook, this.viewModel, this.GridViewTasks);
            this.dragDropManager = new DragDropManager(this, this.GridViewTasks, this.ListViewNavigation, this.workbook, this.navigationService, this.trackingManager);
            
            this.lastSplitViewOpen = !this.workbook.Settings.GetValue<bool>(CoreSettings.NavigationMenuMinimized);

            this.Loaded += this.OnLoaded;
            this.SizeChanged += this.OnSizeChanged;

            this.DataContext = this.viewModel;

            this.HeaderBarAutoSuggestBox.QuerySubmitted += (s, e) =>
            {
                this.HeaderBarAutoSuggestBox.Focus(FocusState.Programmatic);
            };
            this.HeaderBarAutoSuggestBox.GotFocus += (s, e) =>
            {
                this.PanelHeaderContent.Visibility = Visibility.Collapsed;
                this.HeaderBarAutoSuggestBoxBorderMask.Visibility = Visibility.Collapsed;
            };
            this.HeaderBarAutoSuggestBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(this.ViewModel.SearchText))
                {                    
                    this.PanelHeaderContent.Visibility = Visibility.Visible;
                    this.HeaderBarAutoSuggestBoxBorderMask.Visibility = Visibility.Visible;
                }
            };

            // when user types text in the search box, update the view model
            var synchronizationContext = SynchronizationContext.Current;
            Observable
                .FromEventPattern<AutoSuggestBoxTextChangedEventArgs>(this.NavBarAutoSuggestBox, "TextChanged")
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(synchronizationContext)
                .Subscribe(e => this.viewModel.SearchText = this.NavBarAutoSuggestBox.Text);
            Observable
                .FromEventPattern<AutoSuggestBoxTextChangedEventArgs>(this.HeaderBarAutoSuggestBox, "TextChanged")
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(synchronizationContext)
                .Subscribe(e => this.viewModel.SearchText = this.HeaderBarAutoSuggestBox.Text);

            DataTransferManager.GetForCurrentView().DataRequested += this.OnDataRequested;

            this.SplitView.RegisterPropertyChangedCallback(SwipeableSplitView.IsSwipeablePaneOpenProperty, this.OnSplitViewIsOpenChanged);

            this.ContextualActionBar.Visibility = Visibility.Collapsed;
            this.ContextualActionBar.Initialize(this.SelectionManager);
            
            this.ListViewTasks.Tapped += (s, e) =>
            {
                this.navigationService.CloseFlyouts();
            };
        }

        public async Task HideContextualActionBar()
        {
            if (this.ContextualActionBar.Visibility != Visibility.Visible)
                return;

            this.ContextualActionBar.AnimateOpacity(0, TimeSpan.FromMilliseconds(SlideAnimationMs));
            await this.ContextualActionBar.SlideDownAsync(TimeSpan.FromMilliseconds(SlideAnimationMs), 70);

            this.ContextualActionBar.Visibility = Visibility.Collapsed;
            this.ToastIndicator.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.ToastIndicator.Margin = new Thickness(0, 0, 0, 0);
        }

        public void ShowContextualActionBar()
        {
            if (this.ContextualActionBar.Visibility != Visibility.Collapsed)
                return;

            this.ContextualActionBar.Visibility = Visibility.Visible;
            this.ContextualActionBar.AnimateOpacity(1, TimeSpan.FromMilliseconds(SlideAnimationMs));
            this.ContextualActionBar.SlideUpAsync(TimeSpan.FromMilliseconds(SlideAnimationMs), 40);

            this.ToastIndicator.HorizontalAlignment = HorizontalAlignment.Right;
            this.ToastIndicator.Margin = new Thickness(0, 0, 10, 0);
        }
        
        private void OnSplitViewIsOpenChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (!this.SplitView.IsSwipeablePaneOpen)
            {
                double leftMargin = -106;
                int visibleChildrenCount = this.StackPanelTopNavCommands.Children.Count(c => c.Visibility == Visibility.Visible);
                if (visibleChildrenCount == 4)
                    leftMargin = -64;
                else if (visibleChildrenCount == 2)
                    leftMargin = -142;

                this.StackPanelTopNavCommands.Margin = new Thickness(leftMargin, 0, 0, 0);

                leftMargin = -106;
                visibleChildrenCount = this.StackPanelBottomNavCommands.Children.Count(c => c.Visibility == Visibility.Visible);
                if (visibleChildrenCount == 4)
                    leftMargin = -64;
                else if (visibleChildrenCount == 2)
                    leftMargin = -142;
                
                this.StackPanelBottomNavCommands.Margin = new Thickness(leftMargin, 0, 0, 0);
            }
            else
            {
                this.StackPanelTopNavCommands.Margin = new Thickness();
                this.StackPanelBottomNavCommands.Margin = new Thickness();
            }
        }

        private async void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            await HtmlExporter.PrepareExport(args.Request, this.viewModel.SelectedFolderItem);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width > ResponsiveHelper.MinWidth && e.NewSize.Width < ResponsiveHelper.MinWidth)
            {
                // keep track of the current IsSwipeablePaneOpen state
                this.lastSplitViewOpen = this.SplitView.IsSwipeablePaneOpen;

                // switch to small mode
                this.SplitView.IsSwipeablePaneOpen = false;
            }
            else if (e.PreviousSize.Width < ResponsiveHelper.MinWidth && e.NewSize.Width > ResponsiveHelper.MinWidth)
            {
                // switch to large mode
                this.SplitView.IsSwipeablePaneOpen = this.lastSplitViewOpen;
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.OnLoaded;

            var animationHandler = new ListViewAnimationManager(this.ListViewTasks, this.workbook, this.navigationService);
            Ioc.RegisterInstance<IListViewAnimationManager, ListViewAnimationManager>(animationHandler);

            this.navigationMenuManager.AttachMenu(this.ListViewNavigation);

            // first time we create the MainPage
            // do not await this call so that the rest of the code executes immediately
            this.viewModel.ActivateAsync(this.SelectionManager);

            // when we start in overlay mode, hide the pane
            if (this.SplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                this.SplitView.IsSwipeablePaneOpen = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // ignore args in case of a back navigation
            // this fix a problem where the app starts with an arg to display for example a view
            // then user navigates to another page and hit back
            // in that case we don't want to handle args
            if (e.NavigationMode != NavigationMode.Back && e.NavigationMode != NavigationMode.New)
            {
                LauncherHelper.TryHandleArgs(e.Parameter);
            }

            if (this.viewModel != null && this.viewModel.SelectedMenuItem == null)
                this.SetSelectedMenuItem();

            if (this.viewModel != null && !string.IsNullOrEmpty(this.viewModel.SearchText))
            {
                this.NavBarAutoSuggestBox.Text = this.viewModel.SearchText;
                this.HeaderBarAutoSuggestBox.Text = this.viewModel.SearchText;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationService.CloseFlyouts();
            this.HideContextualActionBar();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SearchText" && string.IsNullOrEmpty(this.viewModel.SearchText))
            {
                this.NavBarAutoSuggestBox.Text = string.Empty;
                this.HeaderBarAutoSuggestBox.Text = string.Empty;
            }
            else if (e.PropertyName == "SelectedMenuItem")
            {
                this.workbook.Settings.SetValue(CoreSettings.StartupSelectedItemIndex, this.viewModel.MenuItems.IndexOf(this.viewModel.SelectedMenuItem));

                if (this.SplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                {
                    this.SplitView.TryClose();
                }
            }
        }

        private void OnHamButtonClick(object senter, RoutedEventArgs e)
        {
            this.SplitView.IsSwipeablePaneOpen = !this.SplitView.IsSwipeablePaneOpen;
            this.lastSplitViewOpen = this.SplitView.IsSwipeablePaneOpen;
        }

        private void OnFolderNameTapped(object sender, TappedRoutedEventArgs e)
        {
            if (ResponsiveHelper.IsUsingSmallLayout())
            {
                this.ShowQuickNavBar();
            }
            else
            {
                if (this.viewModel.SelectedFolder != null)
                    this.SelectionManager.ToogleTaskSelection(this.viewModel.SelectedFolder.Tasks.ToList());                
            }            
        }

        private void ShowQuickNavBar()
        {
            if (this.quickNavBar == null)
            {
                this.quickNavBar = new QuickNavBar((f) =>
                {
                    this.viewModel.SelectedMenuItem = this.viewModel.MenuItems.FirstOrDefault(m => m.Folder == f);
                    this.HideQuickNavBar();
                });

                this.quickNavBar.VerticalAlignment = VerticalAlignment.Top;
                this.quickNavBar.RenderTransform = new TranslateTransform();
            }

            if (!this.SplitViewContent.Children.Contains(this.quickNavBar))
            {
                Grid.SetRowSpan(this.quickNavBar, this.SplitViewContent.RowDefinitions.Count);
                this.SplitViewContent.Children.Add(this.quickNavBar);

                this.quickNavBar.Show(this.viewModel.MenuItems.Select(f => f.Folder));
                this.quickNavBar.SlideFromTop(TimeSpan.FromMilliseconds(250), true, () =>
                {
                    Frame rootFrame = Window.Current.Content as Frame;
                    if (rootFrame != null)
                        rootFrame.Tapped += this.OnRootFrameTapped;
                });
            }
            else
            {
                this.HideQuickNavBar();
            }
        }

        private void OnRootFrameTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null)
                rootFrame.Tapped -= this.OnRootFrameTapped;

            this.HideQuickNavBar();
        }

        private void HideQuickNavBar()
        {
            if (this.SplitViewContent.Children.Contains(this.quickNavBar))
            {
                this.quickNavBar.SlideFromTop(
                    TimeSpan.FromMilliseconds(250),
                    false,
                    () => this.SplitViewContent.Children.Remove(this.quickNavBar));
            }
        }

        private void OnGridViewGroupHeaderTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            if (fe.DataContext != null)
            {
                var group = fe.DataContext as Group<ITask>;
                if (group != null)
                    this.SelectionManager.ToogleTaskSelection(group.ToList());
            }
        }

        private void SetSelectedMenuItem()
        {
            int index = -1;
            if (this.workbook.Settings.HasValue(CoreSettings.StartupSelectedItemIndex))
                index = this.workbook.Settings.GetValue<int>(CoreSettings.StartupSelectedItemIndex);

            if (index >= 0 && index < this.viewModel.MenuItems.Count)
                this.viewModel.SelectedMenuItem = this.viewModel.MenuItems[index];
            else
                this.viewModel.SelectedMenuItem = this.viewModel.MenuItems.OfType<FolderItemViewModel>().FirstOrDefault(f => f.Folder is IFolder);
        }

        public void ToggleNavigationMenuState()
        {
            this.SplitView.IsSwipeablePaneOpen = !this.SplitView.IsSwipeablePaneOpen;
        }

        public void FocusSearchBox()
        {
            this.SplitView.IsSwipeablePaneOpen = true;
            this.NavBarAutoSuggestBox.Focus(FocusState.Keyboard);
        }

        public void OnSuspending()
        {
            bool splitViewOpen = this.SplitView.IsSwipeablePaneOpen;
            if (this.RenderSize.Width < ResponsiveHelper.MinWidth)
                splitViewOpen = this.lastSplitViewOpen;

            this.workbook.Settings.SetValue(CoreSettings.NavigationMenuMinimized, !splitViewOpen);
        }        
    }
}

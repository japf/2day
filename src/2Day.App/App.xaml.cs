using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Globalization;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Chartreuse.Today.App.Services;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.App.Views.Settings;
using Chartreuse.Today.App.Views.Sync;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Manager;
using Chartreuse.Today.Core.Universal.Model;
using Chartreuse.Today.Core.Universal.Tools;
using Chartreuse.Today.Core.Universal.Tools.Logging;
using Chartreuse.Today.Core.Universal.Notifications;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private static readonly Color appColor = new Color { R = Constants.AppColorR, G = Constants.AppColorG, B = Constants.AppColorB };

        private Bootstraper bootstraper;
        private INavigationService navigationService;
        private IPlatformService platformService;

        private CrashHandler crashHandler;
        private SuspensionManager suspensionManager;
        private Mutex mutex;
        private ApplicationView mainview;
        private static bool? hasStatusBar;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.crashHandler = new CrashHandler(this);

            this.mutex = new Mutex(true, Constants.SyncPrimitiveAppRunningForeground);

            var logHandler = new WinLogHandler();

            LogService.Initialize(logHandler);
            WinSettings winSettings = WinSettings.Instance;

            // check log file rotation once every 5 launches
            if (winSettings.GetValue<int>(CoreSettings.LaunchCount) % 5 == 0)
                logHandler.CheckLogRotationAsync().Wait(TimeSpan.FromMilliseconds(500));
            
            LogService.Level = winSettings.GetValue<LogLevel>(CoreSettings.LogLevel);

            this.RequestedTheme = winSettings.GetValue<bool>(CoreSettings.UseDarkTheme) ? ApplicationTheme.Dark : ApplicationTheme.Light;

            this.InitializeLanguage(winSettings);

            this.InitializeComponent();

            this.Suspending += this.OnSuspending;
        }

        private void InitializeLanguage(ISettings settings)
        {
            string languageCode = settings.GetValue<string>(CoreSettings.OverrideLanguage);
            if (languageCode == null)
                languageCode = String.Empty;

            ApplicationLanguages.PrimaryLanguageOverride = languageCode;
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            /* helper to be able to attach a debugger when app starts from Cortana activation ;-)
            while (!Debugger.IsAttached)
            {
                await Task.Delay(1000);
            }
            Debugger.Break();*/

            await this.BootstrapFrame(null, args);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            await this.BootstrapFrame(e, null);

            try
            {
                var platform = Ioc.Resolve<IPlatformService>();
                var synchronizationManager = Ioc.Resolve<ISynchronizationManager>();
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Exception while setting update Localytics");
            }
        }

        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            var shareOperation = args.ShareOperation;
            if (shareOperation != null && shareOperation.Data != null)
            {
                string title = null;
                if (shareOperation.Data.Contains(StandardDataFormats.WebLink))
                {
                    Uri uri = await shareOperation.Data.GetWebLinkAsync();
                    if (uri != null)
                        title = uri.ToString();
                }
                else if (shareOperation.Data.Contains(StandardDataFormats.Text))
                {
                    title = await shareOperation.Data.GetTextAsync();
                }

                if (!String.IsNullOrWhiteSpace(title))
                {
                    if (this.mainview != null)
                    {
                        ApplicationViewSwitcher.SwitchAsync(this.mainview.Id);
                        CoreApplication.MainView.Dispatcher.RunIdleAsync((c) =>
                        {
                            this.navigationService.FlyoutTo(typeof(TaskPage), new TaskCreationParameters { Title = title });
                            ToastHelper.ToastMessage(StringResources.Message_TapToContinue);
                        });
                    }
                    else
                    {
                        await this.BootstrapFrame(null, null, title);
                    }
                }
            }
        }

        private async Task BootstrapFrame(LaunchActivatedEventArgs launchActivatedEventArgs, IActivatedEventArgs activatedEventArgs, string addTaskTitle = null)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                try
                {
                    ApplicationView appView = ApplicationView.GetForCurrentView();
                    this.mainview = appView;
                    SetupTitleBar(appView);
                    SetupStatusBar(appColor);

                    appView.SetPreferredMinSize(new Size(Constants.AppMinWidth, Constants.AppMinHeight));

                    this.bootstraper = new Bootstraper(ApplicationVersion.GetAppVersion());

                    InitializeViewLocator();

                    await this.bootstraper.ConfigureAsync(rootFrame);

                    this.navigationService = Ioc.Resolve<INavigationService>();
                    this.platformService = Ioc.Resolve<IPlatformService>();

                    this.suspensionManager = new SuspensionManager(Ioc.Resolve<IPersistenceLayer>(), Ioc.Resolve<ISynchronizationManager>(), Ioc.Resolve<ITileManager>());

                    rootFrame.Navigated += this.OnNavigated;
                    rootFrame.NavigationFailed += this.OnNavigationFailed;

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;

                    if (rootFrame.Content == null)
                    {
                        Type startupPage = typeof (MainPage);
                        object navigationParameter = launchActivatedEventArgs?.Arguments;

                        var startupManager = Ioc.Resolve<IStartupManager>();
                        if (startupManager.IsFirstLaunch)
                        {
                            startupPage = typeof (WelcomePage);
                        }
                        else if (!String.IsNullOrWhiteSpace(addTaskTitle))
                        {
                            startupPage = typeof (WelcomePage);
                            navigationParameter = new TaskCreationParameters {Title = addTaskTitle};
                        }

                        SystemNavigationManager.GetForCurrentView().BackRequested += this.OnBackRequested;
                        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = rootFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                        Window.Current.VisibilityChanged += this.OnVisibilityChanged;
                        
                        // When the navigation stack isn't restored navigate to the first page,
                        // configuring the new page by passing required information as a navigation parameter
                        rootFrame.Navigate(startupPage, navigationParameter);
                    }

                    if (launchActivatedEventArgs != null)
                        LauncherHelper.TryHandleArgs(launchActivatedEventArgs.Arguments);
                    else if (activatedEventArgs != null)
                        LauncherHelper.TryHandleArgs(activatedEventArgs);

                    // Ensure the current window is active
                    Window.Current.Activate();
                }
                catch (Exception ex)
                {
                    var messageBoxService = new MessageBoxService(new NavigationService(rootFrame));
                    await messageBoxService.ShowAsync("Error", "2Day was unable to start please send a screenshot of this page to the development team. Details: " + ex);

                    DeviceFamily deviceFamily = DeviceFamily.Unkown;
                    if (this.platformService != null)
                        deviceFamily = this.platformService.DeviceFamily;

                    var trackingManager = new TrackingManager(true, deviceFamily);
                    trackingManager.Exception(ex, "Bootstrap", true);
                }
            }
            else
            {
                if (launchActivatedEventArgs != null)
                    LauncherHelper.TryHandleArgs(launchActivatedEventArgs.Arguments);
                else if (activatedEventArgs != null)
                    LauncherHelper.TryHandleArgs(activatedEventArgs);
            }
        }

        private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            try
            {
                if (e.Visible && Ioc.HasType<IMainPageViewModel>())
                {
                    Ioc.Resolve<IMainPageViewModel>().RefreshAsync();
                }

                if (this.platformService != null && this.platformService.DeviceFamily == DeviceFamily.WindowsMobile)
                {
                    if (!e.Visible)
                    {
                        this.mutex.ReleaseMutex();
                        this.mutex.Dispose();
                    }
                    else
                    {
                        this.mutex = new Mutex(true, Constants.SyncPrimitiveAppRunningForeground);
                    }
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Exception App in OnVisibilityChanged");
            }
        }

        private static void SetupTitleBar(ApplicationView appView)
        {
            var titleBar = appView.TitleBar;
            titleBar.BackgroundColor = appColor;
            titleBar.ForegroundColor = Colors.White;

            titleBar.ButtonBackgroundColor = appColor;
            titleBar.ButtonForegroundColor = Colors.White;

            titleBar.ButtonInactiveBackgroundColor = appColor;
            titleBar.ButtonInactiveForegroundColor = Colors.LightGray;

            titleBar.InactiveBackgroundColor = appColor;
            titleBar.InactiveForegroundColor = Colors.LightGray;
        }

        private static async Task SetupStatusBar(Color appColor)
        {
            if (!hasStatusBar.HasValue)
                hasStatusBar = ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

            if (hasStatusBar.Value)
            {
                var statusbar = StatusBar.GetForCurrentView();
                await statusbar.ShowAsync();
                statusbar.BackgroundColor = appColor;
                statusbar.BackgroundOpacity = 1;
                statusbar.ForegroundColor = Colors.White;
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            var frame = (Frame)sender;
            object frameContent = frame.Content;
            
            SetupStatusBar(appColor);

            // each time a navigation event occurs, update the Back button's visibility
            // never display the back button on the main page
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    frame.CanGoBack && !(frameContent is MainPage) ?
                        AppViewBackButtonVisibility.Visible :
                        AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (this.navigationService != null)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame != null && rootFrame.Content is Page)
                {
                    Page page = (Page)rootFrame.Content;

                    // from main page, a back request put the app in background (and we have nothing to do)
                    if (page is MainPage)
                        return;

                    if (page.Content is FrameworkElement)
                    {
                        FrameworkElement pageContent = (FrameworkElement)page.Content;
                        if (pageContent.DataContext is PageViewModelBase)
                        {
                            PageViewModelBase viewmodel = (PageViewModelBase)pageContent.DataContext;
                            e.Handled = true;
                            viewmodel.GoBackCommand.Execute(null);
                            return;
                        }
                    }
                }

                this.navigationService.GoBack();
                e.Handled = true;
            }
            else
            {
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame.CanGoBack)
                {
                    e.Handled = true;
                    rootFrame.GoBack();
                }
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            if (this.suspensionManager != null)
                this.suspensionManager.SuspendAsync(e);
        }

        public static void InitializeViewLocator()
        {
            ViewLocator.WelcomePage = typeof(WelcomePage);

            ViewLocator.SettingsAboutPage = typeof(AboutSettingsPage);
            ViewLocator.SettingsBackupPage = typeof(BackupSettingsPage);
            ViewLocator.SettingsContextsPage = typeof(ContextSettingsPage);
            ViewLocator.SettingsDisplayPage = typeof(DisplaySettingsPage);
            ViewLocator.SettingsFoldersPage = typeof(FolderSettingsPage);
            ViewLocator.SettingsGeneralPage = typeof(GeneralSettingsPage);
            ViewLocator.SettingsMiscPage = typeof(MiscSettingsPage);
            ViewLocator.SettingsSmartViewsPage = typeof(SmartViewSettingsPage);
            ViewLocator.SettingsTaskOrderingPage = typeof(TaskOrderSettingsPage);
            ViewLocator.SettingsViewsPage = typeof(ViewSettingsPage);
            ViewLocator.SettingsSyncPage = typeof(SyncSettingsPage);

            ViewLocator.SyncOutlookActiveSyncSettingsPage = typeof(OutlookActiveSyncSettingsPage);
            ViewLocator.SyncActiveSyncSettingsPage = typeof(ActiveSyncSettingsPage);
            ViewLocator.SyncExchangeSettingsPage = typeof(ExchangeSettingsPage);
            ViewLocator.SyncToodleDoSettingsPage = typeof(ToodleDoSettingsPage);
            ViewLocator.SyncVercorsSettingsPage = typeof(VercorsSettingsPage);
            ViewLocator.SyncAdvancedSyncSettingsPage = typeof(AdvancedSyncSettingsPage);

            ViewLocator.CreateEditTaskPageNew = typeof(TaskPage);
            ViewLocator.EditNotesPage = typeof(TaskNotesPage);
            ViewLocator.CreateEditFolderPage = typeof(FolderPage);
            ViewLocator.CreateEditSmartViewPage = typeof(CreateEditSmartViewPage);

            ViewLocator.DebugPage = typeof(DebugPage);
            ViewLocator.LogViewerSettingsPage = typeof(LogViewerSettingsPage);
        }
    }
}

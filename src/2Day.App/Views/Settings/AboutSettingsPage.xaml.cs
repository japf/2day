using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Shared.ViewModel.Settings;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Settings
{
    public sealed partial class AboutSettingsPage : Page
    {
        private readonly INavigationService navigationService;
        private readonly AboutPageViewModel viewModel;

        public AboutPageViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        public AboutSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.viewModel = Ioc.Build<AboutPageViewModel>();
            this.DataContext = this.viewModel;

            this.navigationService = Ioc.Resolve<INavigationService>();
        }

        private void OnAboutItemTapped(object sender, TappedRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe != null && fe.DataContext is HeadedSettingItemViewModel)
            {
                ((HeadedSettingItemViewModel)fe.DataContext).NavigateCommand.Execute(null);
            }
        }
        
        private void OnWebLinkTapped(object sender, TappedRoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(SafeUri.Get(Constants.WebSiteAddress));
        }

        public void OnLogoDoubleTapped(object source, RoutedEventArgs arg)
        {
            this.navigationService.Navigate(ViewLocator.DebugPage);
        }
    }
}

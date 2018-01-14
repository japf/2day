using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel.Sync;
using Chartreuse.Today.App.Tools.Settings;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Exchange;
using Chartreuse.Today.Exchange.Model;
using Chartreuse.Today.Exchange.Resources;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Sync
{
    public sealed partial class ExchangeSettingsPage : Page, ISettingsPage
    {
        public SettingsSizeMode Size
        {
            get { return SettingsSizeMode.Small; }
        }

        public ExchangeSettingsViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        private readonly IWorkbook workbook;
        private readonly ICryptoService cryptoService;
        private readonly ExchangeSettingsViewModel viewModel;

        public ExchangeSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.workbook = Ioc.Resolve<IWorkbook>();
            this.cryptoService = Ioc.Resolve<ICryptoService>();

            var navigationService = Ioc.Resolve<INavigationService>();
            var synchronizationManager = Ioc.Resolve<ISynchronizationManager>();
            var messageBoxService = Ioc.Resolve<IMessageBoxService>();
            var trackingManager = Ioc.Resolve<ITrackingManager>();

            this.viewModel = new ExchangeSettingsViewModel(this.workbook, navigationService, messageBoxService, synchronizationManager, trackingManager);
            this.DataContext = this.viewModel;

            this.Loaded += (s, e) =>
            {
                // for whatever reason using x:Name does not work here... so we lookup for the control in the visual tree instead
                var passwordBox = TreeHelper.FindVisualChild<PasswordBox>(this);
                byte[] password = this.workbook.Settings.GetValue<byte[]>(ExchangeSettings.ExchangePassword);
                if (password != null)
                    passwordBox.Password = this.cryptoService.Decrypt(password);

                // use an event handler because we cannot use DataBinding with a PasswordBox
                passwordBox.PasswordChanged += this.OnPasswordChanged;
            };
        }
        
        private void OnPasswordChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            this.workbook.Settings.SetValue(
                ExchangeSettings.ExchangePassword,
                this.cryptoService.Encrypt(((PasswordBox)sender).Password));
        }

        private async void OnHelpTapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(SafeUri.Get(Constants.HelpPageExchangeAddress));
        }

        public void OnNavigatedTo(object parameter)
        {
            if (parameter is string && this.viewModel != null)
            {
                if ((string) parameter == "office365")
                {
                    this.flyoutHost.Text = ExchangeResources.ExchangeOffice365_SettingsTitle;
                    this.viewModel.ExchangeVersion = ExchangeServerVersion.ExchangeOffice365.GetString();
                } else if ((string)parameter == "exchange2013")
                {
                    this.viewModel.ExchangeVersion = ExchangeServerVersion.Exchange2013.GetString();
                }
            }
        }

        private void OnGetHelpMicrosoftAccount(object sender, TappedRoutedEventArgs e)
        {
            var platformService = Ioc.Resolve<IPlatformService>();
            platformService.OpenWebUri(Constants.WebSiteAddress);
        }
    }
}

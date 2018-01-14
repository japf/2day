using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel.Sync;
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
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Sync
{
    public sealed partial class ActiveSyncSettingsPage : Page
    {
        private readonly IWorkbook workbook;
        private readonly ICryptoService cryptoService;
        private readonly ActiveSyncSettingsViewModel viewModel;

        public ActiveSyncSettingsViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        public ActiveSyncSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.workbook = Ioc.Resolve<IWorkbook>();
            this.cryptoService = Ioc.Resolve<ICryptoService>();

            var navigationService = Ioc.Resolve<INavigationService>();
            var synchronizationManager = Ioc.Resolve<ISynchronizationManager>();
            var messageBoxService = Ioc.Resolve<IMessageBoxService>();
            var trackingManager = Ioc.Resolve<ITrackingManager>();

            this.viewModel = new ActiveSyncSettingsViewModel(workbook, navigationService, messageBoxService, synchronizationManager, trackingManager);
            this.DataContext = this.viewModel;

            this.Loaded += (s, e) =>
            {
                // for whatever reason using x:Name does not work here... so we lookup for the control in the visual tree instead
                var passwordBox = TreeHelper.FindVisualChild<PasswordBox>(this);
                byte[] password = this.workbook.Settings.GetValue<byte[]>(ExchangeSettings.ActiveSyncPassword);
                if (password != null)
                    passwordBox.Password = this.cryptoService.Decrypt(password);

                // use an event handler because we cannot use DataBinding with a PasswordBox
                passwordBox.PasswordChanged += this.OnPasswordChanged;
            };
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            this.workbook.Settings.SetValue(
                ExchangeSettings.ActiveSyncPassword,
                this.cryptoService.Encrypt(((PasswordBox)sender).Password));
        }

        private void OnCreateAppPasswordTapped(object sender, RoutedEventArgs e)
        {
            Ioc.Resolve<IPlatformService>().OpenWebUri(Constants.LiveAccountCreateAppPassword);
        }
    }
}

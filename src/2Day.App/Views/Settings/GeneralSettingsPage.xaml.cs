using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel.Settings;
using Chartreuse.Today.App.Tools.Settings;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Settings
{
    public sealed partial class GeneralSettingsPage : Page, ISettingsPage
    {
        private readonly GeneralSettingsPageViewModel viewModel;

        public SettingsSizeMode Size
        {
            get { return SettingsSizeMode.Small; }
        }

        public GeneralSettingsPageViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        public GeneralSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.viewModel = Ioc.Build<GeneralSettingsPageViewModel>();
            this.DataContext = this.viewModel;            
        }

        public void OnNavigatedTo(object parameter)
        {
        }
    }
}

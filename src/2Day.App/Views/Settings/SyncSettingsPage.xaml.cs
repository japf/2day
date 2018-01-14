using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel.Settings;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Settings
{
    public sealed partial class SyncSettingsPage : Page
    {
        private readonly SyncSettingsPageViewModel viewModel;

        public SyncSettingsPageViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        public ApplicationResources ApplicationResources
        {
            get { return ApplicationResources.Instance; }
        }

        public SyncSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.viewModel = Ioc.Build<SyncSettingsPageViewModel>();
            this.DataContext = this.viewModel;
        }        
    }
}

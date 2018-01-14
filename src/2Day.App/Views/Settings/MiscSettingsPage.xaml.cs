using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Settings
{
    public sealed partial class MiscSettingsPage : Page
    {
        private readonly MiscSettingsPageViewModel viewModel;

        public MiscSettingsPageViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        public MiscSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.viewModel = Ioc.Build<MiscSettingsPageViewModel>();
            this.DataContext = this.viewModel;
        }
    }
}

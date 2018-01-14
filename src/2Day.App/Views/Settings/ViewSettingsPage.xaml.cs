using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel.Settings;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Settings
{
    public sealed partial class ViewSettingsPage : Page
    {
        public ViewSettingsPageViewModel ViewModel { get; }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        public ViewSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.ViewModel = Ioc.Build<ViewSettingsPageViewModel>();
            this.DataContext = this.ViewModel;
        }
    }
}

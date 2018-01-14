using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Shared.ViewModel.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Settings
{
    public sealed partial class AdvancedSyncSettingsPage : Page
    {
        public AdvancedSyncSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.DataContext = Ioc.Build<AdvancedSyncSettingsPageViewModel>();
        }
    }
}

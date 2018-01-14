using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Shared.ViewModel.Settings;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Settings
{
    public sealed partial class BackupSettingsPage : Page
    {
        public BackupSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.DataContext = Ioc.Build<BackupSettingsPageViewModel>();
        }
    }
}

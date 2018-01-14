using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel.Sync;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Universal.Model;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Sync
{
    public sealed partial class VercorsSettingsPage : Page
    {
        private readonly VercorsSettingsViewModel viewmodel;

        public VercorsSettingsViewModel ViewModel
        {
            get { return this.viewmodel; }
        }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }
        
        public VercorsSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.viewmodel = Ioc.Build<VercorsSettingsViewModel>();
            this.DataContext = this.viewmodel;
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!WinSettings.Instance.GetValue<bool>(CoreSettings.UseDarkTheme))
                this.imgCloudOverview.Source = new BitmapImage(SafeUri.Get("ms-appx:///Resources/Icons/Sync/vercors-cloud-overview-dark.png", UriKind.RelativeOrAbsolute));
        }
    }
}

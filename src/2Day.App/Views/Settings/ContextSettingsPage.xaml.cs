using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel.Settings;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views.Settings
{
    public sealed partial class ContextSettingsPage : Page
    {
        public ContextSettingsPageViewModel ViewModel { get; }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        public ContextSettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.ViewModel = Ioc.Build<ContextSettingsPageViewModel>();
            this.DataContext = this.ViewModel;

            this.listview.Tapped += this.OnListViewTapped;
        }

        private void OnListViewTapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && e.OriginalSource is FrameworkElement && this.ViewModel != null)
            {
                var fe = (FrameworkElement)e.OriginalSource;
                if (fe.DataContext != null)
                {
                    this.ViewModel.EditItemCommand.Execute(fe.DataContext);
                }
            }
        }
    }
}

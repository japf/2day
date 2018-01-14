using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Views
{
    public sealed partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) => this.OpeningStoryboard.Begin();
        }

        private void OnBtnGoClick(object sender, RoutedEventArgs e)
        {
            var navigationService = Ioc.Resolve<INavigationService>();

            // if we can go back, it's not first launch and we just display the previous page
            // otherwise, this is first launch
            if (navigationService.CanGoBack)
            {
                navigationService.GoBack();
            }
            else
            {
                navigationService.Navigate(typeof(MainPage));
                navigationService.ClearBackStack();
            }
        }
    }
}

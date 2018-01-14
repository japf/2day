using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools.Settings;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.App.Tools;

namespace Chartreuse.Today.App.Views
{
    public sealed partial class FolderPage : Page, ISettingsPage
    {
        private FolderViewModelBase viewmodel;

        public SettingsSizeMode Size
        {
            get { return SettingsSizeMode.Small; }
        }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        public FolderViewModelBase ViewModel
        {
            get { return this.viewmodel; }
        }

        public FolderPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Popup)
            {
                this.root.BorderThickness = new Thickness(1, 0, 0, 0);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.OnNavigatedTo(e.Parameter);
        }
        
        public void OnNavigatedTo(object parameter)
        {
            var workbook = Ioc.Resolve<IWorkbook>();
            var navigationService = Ioc.Resolve<INavigationService>();
            var messageBoxService = Ioc.Resolve<IMessageBoxService>();
            var platformService = Ioc.Resolve<IPlatformService>();
            var tileManager = Ioc.Resolve<ITileManager>();

            if (parameter is IFolder)
            {
                this.viewmodel = new EditFolderViewModel((IFolder)parameter, workbook, navigationService, messageBoxService, platformService, tileManager);
            }
            else
            {
                this.viewmodel = new CreateFolderViewModel(workbook, navigationService, messageBoxService, platformService);
            }

            this.DataContext = this.viewmodel;
        }
    }
}

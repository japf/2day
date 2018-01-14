using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Shared.ViewModel.Settings;
using Chartreuse.Today.Core.Shared.Tools;
using Windows.UI.Xaml;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;

namespace Chartreuse.Today.App.Views.Settings
{
    public sealed partial class DisplaySettingsPage : Page
    {
        private DisplaySettingsPageViewModel viewmodel;

        public DisplaySettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();

            this.viewmodel = Ioc.Build<DisplaySettingsPageViewModel>();
            this.viewmodel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "UseDarkTheme" || e.PropertyName == "UseLightTheme")
                {
                    this.UpdateAppTheme();
                }
            };
            this.DataContext = this.viewmodel;            
        }

        private void UpdateAppTheme()
        {
            // set the theme on the main page + on opened flyouts
            ElementTheme theme = this.viewmodel.UseLightTheme ? ElementTheme.Light : ElementTheme.Dark;
            Frame rootFrame = Window.Current.Content as Frame;
            Page page = rootFrame.Content as Page;
            if (page != null)
                page.RequestedTheme = theme;

            var navigationService = (NavigationService)Ioc.Resolve<INavigationService>();
            foreach (var flyout in navigationService.Flyouts)
                ((FrameworkElement)flyout.Content).RequestedTheme = theme;

            ResourcesLocator.UpdateTheme(this.viewmodel.UseLightTheme);

            var workbook = Ioc.Resolve<IWorkbook>();
            foreach (var view in workbook.Views)
                ((ModelEntityBase)view).RaisePropertyChanged("Color");
            foreach (var context in workbook.Contexts)
                ((ModelEntityBase)context).RaisePropertyChanged("Color");
            foreach (var tag in workbook.Tags)
                ((ModelEntityBase)tag).RaisePropertyChanged("Color");
            foreach (var smartview in workbook.SmartViews)
                ((ModelEntityBase)smartview).RaisePropertyChanged("Color");
        }
    }
}

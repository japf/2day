using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Shared.ViewModel.Settings;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Shared.Model;
using System;

namespace Chartreuse.Today.App.Views
{
    public sealed partial class SettingsPage : Page
    {
        private readonly SettingsPageViewModel viewModel;

        public SettingsPageViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        public SettingsPage()
        {
            this.InitializeComponent();
            this.SetRequestedTheme();
            
            this.viewModel = Ioc.Build<SettingsPageViewModel>();
            this.DataContext = this.viewModel;
        }
        
        private void OnSettingItemTapped(object sender, TappedRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe != null && fe.DataContext is SettingsItemViewModel)
            {
                ((SettingsItemViewModel)fe.DataContext).NavigateCommand.Execute(null);
            }
        }
    }
}

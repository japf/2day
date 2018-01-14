using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Chartreuse.Today.App.Resources;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools.Settings;
using Chartreuse.Today.App.Tools;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Chartreuse.Today.App.Views
{
    public sealed partial class TaskNotesPage : Page, ISettingsPage
    {
        private TaskViewModelBase taskViewModel;

        public SettingsSizeMode Size
        {
            get { return SettingsSizeMode.Large; }
        }

        public TaskViewModelBase ViewModel
        {
            get { return this.taskViewModel; }
        }

        public StringResourcesAccessor Strings
        {
            get { return ApplicationResources.Instance.StringResources; }
        }

        public TaskNotesPage()
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

        public void OnNavigatedTo(object parameter)
        {
            this.taskViewModel = (TaskViewModelBase)parameter;
            this.DataContext = this.taskViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.OnNavigatedTo(e.Parameter);
        }
    }
}

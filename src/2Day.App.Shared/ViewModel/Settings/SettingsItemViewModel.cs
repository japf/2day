using System;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Icons;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class SettingsItemViewModel : ViewModelBase
    {
        private readonly AppIconType icon;
        private readonly string headline;
        private readonly Func<string> getDescription;
        private string description;
        private readonly Action navigate;
        private readonly ICommand navigateCommand;

        public string Headline
        {
            get { return this.headline; }
        }

        public string Description
        {
            get { return this.description; }
            set
            {
                if (this.description != value)
                {
                    this.description = value;
                    this.RaisePropertyChanged("Description");
                }
            }
        }

        public ICommand NavigateCommand
        {
            get { return this.navigateCommand; }
        }

        public AppIconType Icon
        {
            get { return this.icon; }
        }

        public SettingsItemViewModel(AppIconType icon, string headline, Func<string> getDescription, Action navigate)
        {
            this.icon = icon;
            this.headline = headline;
            this.getDescription = getDescription;

            this.description = getDescription();
            this.navigate = navigate;

            this.navigateCommand = new RelayCommand(this.NavigateExecute);
        }

        private void NavigateExecute()
        {
            this.navigate();
        }

        public void UpdateDescription()
        {
            this.Description = this.getDescription();
        }
    }
}

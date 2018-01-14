using System;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Icons;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class HeadedSettingItemViewModel
    {
        private readonly AppIconType icon;
        private readonly string headline;
        private readonly string description;
        private readonly ICommand navigateCommand;

        public AppIconType Icon
        {
            get { return this.icon; }
        }

        public string Headline
        {
            get { return this.headline; }
        }

        public string Description
        {
            get { return this.description; }
        }

        public ICommand NavigateCommand
        {
            get { return this.navigateCommand; }
        }

        public HeadedSettingItemViewModel(AppIconType icon, string headline, string description, Action navigateAction)
        {
            if (string.IsNullOrEmpty(headline))
                throw new ArgumentNullException(nameof(headline));
            if (navigateAction == null)
                throw new ArgumentNullException(nameof(navigateAction));

            this.icon = icon;
            this.headline = headline;
            this.description = description;

            this.navigateCommand = new RelayCommand(navigateAction);
        }
    }
}

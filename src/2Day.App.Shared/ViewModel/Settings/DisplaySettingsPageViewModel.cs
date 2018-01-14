using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class DisplaySettingsPageViewModel : PageViewModelBase
    {
        private readonly ICommand pickCustomImageCommand;
        private readonly IMessageBoxService messageBoxService;
        private readonly IPlatformService platformService;
        private readonly List<string> patterns;
        private readonly bool previousUseDarkTheme;

        private string backgroundSource;
        private string selectedPattern;
        private bool useDarkTheme;

        private bool useBackgroundNone;
        private bool useBackgroundPattern;
        private bool useBackgroundImage;
        private double opacity;

        public bool UseBackgroundNone
        {
            get { return this.useBackgroundNone; }
            set
            {
                if (this.useBackgroundNone != value && value)
                {
                    this.useBackgroundNone = true;
                    this.useBackgroundPattern = false;
                    this.useBackgroundImage = false;

                    this.backgroundSource = null;
                    this.selectedPattern = null;
                    this.Opacity = 1.0;
                    
                    this.Settings.SetValue<string>(CoreSettings.BackgroundPattern, null);
                    this.Settings.SetValue<string>(CoreSettings.BackgroundImage, null);

                    this.RaisePropertyChanged();
                }
            }
        }

        public bool UseBackgroundPattern
        {
            get { return this.useBackgroundPattern; }
            set
            {
                if (this.useBackgroundPattern != value && value)
                {
                    this.useBackgroundNone = false;
                    this.useBackgroundPattern = true;
                    this.useBackgroundImage = false;

                    if (this.selectedPattern == null)
                        this.selectedPattern = this.patterns[0];

                    this.backgroundSource = ResourcesLocator.BuildPatternPath(
                        this.selectedPattern,
                        this.Settings.GetValue<bool>(CoreSettings.UseDarkTheme));
                    this.Opacity = 1.0;

                    this.Settings.SetValue<string>(CoreSettings.BackgroundPattern, this.selectedPattern);
                    this.Settings.SetValue<string>(CoreSettings.BackgroundImage, null);

                    this.RaisePropertyChanged();
                }
            }
        }

        public bool UseBackgroundImage
        {
            get { return this.useBackgroundImage; }
            set
            {
                if (this.useBackgroundImage != value && value)
                {
                    this.useBackgroundNone = false;
                    this.useBackgroundPattern = false;
                    this.useBackgroundImage = true;

                    this.backgroundSource = null;
                    this.Opacity = 1.0;

                    if (this.useBackgroundImage)
                    {
                        string currentImage = this.Settings.GetValue<string>(CoreSettings.BackgroundImage);
                        if (this.backgroundSource != currentImage && !string.IsNullOrEmpty(currentImage))
                        {
                            // remove old file
                            this.platformService.DeleteFileAsync(currentImage).Wait(TimeSpan.FromMilliseconds(500));
                        }

                        this.Settings.SetValue<string>(CoreSettings.BackgroundImage, this.backgroundSource);
                        this.Settings.SetValue(CoreSettings.BackgroundPattern, string.Empty);
                    }

                    this.RaisePropertyChanged();
                }
            }
        }

        public IEnumerable<string> Patterns
        {
            get { return this.patterns; }
        }

        public string SelectedPattern
        {
            get
            {
                return this.selectedPattern;
            }
            set
            {
                if (this.selectedPattern != value)
                {
                    this.selectedPattern = value;
                    this.RaisePropertyChanged("SelectedPattern");

                    this.backgroundSource = ResourcesLocator.BuildPatternPath(this.selectedPattern, this.Settings.GetValue<bool>(CoreSettings.UseDarkTheme));
                    this.Settings.SetValue<string>(CoreSettings.BackgroundImage, this.backgroundSource);

                    this.RaisePropertyChanged("BackgroundSource");
                }
            }
        }

        public ICommand PickCustomImageCommand
        {
            get { return this.pickCustomImageCommand; }
        }

        public double Opacity
        {
            get { return this.opacity; }
            set
            {
                if (this.opacity != value)
                {
                    this.opacity = value;
                    this.Settings.SetValue(CoreSettings.BackgroundOpacity, this.opacity);

                    this.RaisePropertyChanged("Opacity");
                }
            }
        }

        public string BackgroundSource
        {
            get { return this.backgroundSource; }
        }

        public bool UseDarkTheme
        {
            get { return this.useDarkTheme; }
            set
            {
                if (this.useDarkTheme != value)
                {
                    this.useDarkTheme = value;
                    this.Settings.SetValue(CoreSettings.UseDarkTheme, this.useDarkTheme);
                    this.backgroundSource = ResourcesLocator.BuildPatternPath(this.selectedPattern, this.useDarkTheme);
                    
                    this.Settings.SetValue<string>(CoreSettings.BackgroundImage, this.backgroundSource);

                    this.RaisePropertyChanged("UseDarkTheme");
                    this.RaisePropertyChanged("UseLightTheme");
                    this.RaisePropertyChanged("BackgroundSource");
                }
            }
        }

        public bool UseLightTheme
        {
            get { return !this.useDarkTheme; }
            set
            {
                if (!this.useDarkTheme != value)
                {
                    this.useDarkTheme = !value;
                    this.Settings.SetValue(CoreSettings.UseDarkTheme, this.useDarkTheme);
                    this.backgroundSource = ResourcesLocator.BuildPatternPath(this.selectedPattern, this.useDarkTheme);

                    this.Settings.SetValue<string>(CoreSettings.BackgroundImage, this.backgroundSource);

                    this.RaisePropertyChanged("UseDarkTheme");
                    this.RaisePropertyChanged("UseLightTheme");
                    this.RaisePropertyChanged("BackgroundSource");
                }
            }
        }

        public DisplaySettingsPageViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, IPlatformService platformService) 
            : base(workbook, navigationService)
        {
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (platformService == null)
                throw new ArgumentNullException(nameof(platformService));

            this.messageBoxService = messageBoxService;
            this.platformService = platformService;

            this.useDarkTheme = this.Settings.GetValue<bool>(CoreSettings.UseDarkTheme);
            this.previousUseDarkTheme = this.useDarkTheme;

            this.patterns = new List<string>(ResourcesLocator.Patterns);

            this.pickCustomImageCommand = new RelayCommand(this.PickCustomImageExecute);

            this.opacity = this.Settings.GetValue<double>(CoreSettings.BackgroundOpacity);

            string backgroundImage = this.Settings.GetValue<string>(CoreSettings.BackgroundImage);
            string backgroundPatternName = this.Settings.GetValue<string>(CoreSettings.BackgroundPattern);
            string backgroundPattern = ResourcesLocator.BuildPatternPath(
                backgroundPatternName,
                this.Settings.GetValue<bool>(CoreSettings.UseDarkTheme));

            this.useBackgroundPattern = !string.IsNullOrEmpty(backgroundPattern);
            this.useBackgroundImage = !string.IsNullOrEmpty(backgroundImage) && !this.useBackgroundPattern;
            this.useBackgroundNone = !this.useBackgroundPattern && !this.useBackgroundImage;

            if (this.useBackgroundPattern)
                this.backgroundSource = backgroundPattern;
            else if (this.useBackgroundImage)
                this.backgroundSource = backgroundImage;

            this.selectedPattern = backgroundPatternName;
            this.selectedPattern = this.patterns.FirstOrDefault(p => p.Equals(this.selectedPattern, StringComparison.OrdinalIgnoreCase));
        }

        private async void PickCustomImageExecute()
        {
            string imagePath = await this.platformService.PickImageAsync(this.backgroundSource);
            if (imagePath != null)
            {
                this.backgroundSource = imagePath;
                this.Settings.SetValue<string>(CoreSettings.BackgroundImage, this.backgroundSource);

                this.RaisePropertyChanged();
            }
        }

        public override async void Dispose()
        {
          
        }
    }
}

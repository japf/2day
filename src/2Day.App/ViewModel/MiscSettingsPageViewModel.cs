using System.Threading.Tasks;
using System.Windows.Input;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Shared.ViewModel.Settings;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.ViewModel
{
    public class MiscSettingsPageViewModel : MiscSettingsPageViewModelBase
    {
        private readonly BackupHelper backupHelper;
        private readonly ICommand showLogCommand;

        private bool isLogEnabled;
        
        public ICommand ShowLogCommand
        {
            get { return this.showLogCommand; }
        }

        public bool IsLogEnabled
        {
            get { return this.isLogEnabled; }
            set
            {
                if (this.isLogEnabled != value)
                {
                    this.isLogEnabled = value;

                    if (this.isLogEnabled)
                    {
                        this.Workbook.Settings.SetValue(CoreSettings.LogLevel, LogLevel.Verbose);
                        LogService.Level = LogLevel.Verbose;
                    }
                    else
                    {
                        this.Workbook.Settings.SetValue(CoreSettings.LogLevel, LogLevel.None);
                        LogService.Level = LogLevel.None;
                    }

                    this.RaisePropertyChanged("IsLogEnabled");
                }
            }
        }
        
        public MiscSettingsPageViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, INotificationService notificationService, IPlatformService platformService, ITrackingManager trackingManager, IPersistenceLayer persistenceLayer) 
            : base(workbook, navigationService, messageBoxService, notificationService, platformService, trackingManager, persistenceLayer)
        {
            this.showLogCommand = new RelayCommand(this.ShowLogExecute);
            
            this.backupHelper = new BackupHelper(this.persistenceLayer, this.messageBoxService, this.trackingManager);

            this.isLogEnabled = this.Workbook.Settings.GetValue<LogLevel>(CoreSettings.LogLevel) > LogLevel.None;
        }

        private void ShowLogExecute()
        {
            this.NavigationService.FlyoutTo(ViewLocator.LogViewerSettingsPage);
        }

        protected override async void RestoreBackupExecute()
        {
            await this.backupHelper.RestoreBackupAsync();            
        }

        protected override async void CreateBackupExecute()
        {
            await this.backupHelper.CreateBackupAsync();            
        }        
    }
}

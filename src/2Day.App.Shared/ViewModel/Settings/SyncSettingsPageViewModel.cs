using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public class SyncSettingsPageViewModel : PageViewModelBase
    {
        #region fields

        private readonly ISynchronizationManager synchronizationManager;
        private readonly IMessageBoxService messageBoxService;
        private readonly IPlatformService platformService;

        private readonly ObservableCollection<SyncProviderViewModel> availableProviders;
        private readonly ICommand debugCommand;
        private readonly ICommand showLogCommand;
        private readonly ICommand openHelpCommand;

        private SyncProviderViewModel activeProvider;
        private bool isLogEnabled;

        #endregion

        #region properties

        public bool HasNoProviderSetup
        {
            get { return !this.HasProviderSetup; }
        }

        public bool HasProviderSetup
        {
            get { return this.synchronizationManager.ActiveService != SynchronizationService.None; }
        }

        public SyncProviderViewModel ActiveProvider
        {
            get
            {
                return this.activeProvider;
            }
            private set
            {
                if (this.activeProvider != value)
                {
                    this.activeProvider = value;

                    this.RaisePropertyChanged("ActiveProvider");
                    this.RaisePropertyChanged("HasNoProviderSetup");
                    this.RaisePropertyChanged("HasProviderSetup");
                }
            }
        }

        public ObservableCollection<SyncProviderViewModel> AvailableProviders
        {
            get { return this.availableProviders; }
        }

        public bool SyncBackground
        {
            get
            {
                return this.Settings.GetValue<bool>(CoreSettings.BackgroundSync);
            }
            set
            {
                if (this.SyncBackground != value)
                {
                    this.Settings.SetValue(CoreSettings.BackgroundSync, value);
                    this.RaisePropertyChanged("SyncBackground");
                }
            }
        }
       
        public bool SyncOnStartup
        {
            get
            {
                return this.Settings.GetValue<bool>(CoreSettings.SyncOnStartup);
            }
            set
            {
                if (this.SyncOnStartup != value)
                {
                    this.Settings.SetValue(CoreSettings.SyncOnStartup, value);
                    this.RaisePropertyChanged("SyncOnStartup");
                }
            }
        }

        public bool AutoSync
        {
            get
            {
                return this.Settings.GetValue<bool>(CoreSettings.SyncAutomatic);
            }
            set
            {
                if (this.AutoSync != value)
                {
                    this.Settings.SetValue(CoreSettings.SyncAutomatic, value);
                    this.RaisePropertyChanged("AutoSync");
                }
            }
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

        public ICommand DebugCommand
        {
            get { return this.debugCommand; }
        }

        public ICommand ShowLogCommand
        {
            get { return this.showLogCommand; }
        }

        public ICommand OpenHelpCommand
        {
            get { return this.openHelpCommand; }
        }

        #endregion

        public SyncSettingsPageViewModel(IWorkbook workbook, IMessageBoxService messageBoxService, INavigationService navigationService, IPlatformService platformService, ISynchronizationManager synchronizationManager)
            : base(workbook, navigationService)
        {
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));
            if (platformService == null)
                throw new ArgumentNullException(nameof(platformService));
            if (synchronizationManager == null)
                throw new ArgumentNullException(nameof(synchronizationManager));

            this.messageBoxService = messageBoxService;
            this.platformService = platformService;
            this.synchronizationManager = synchronizationManager;
            this.synchronizationManager.PropertyChanged += this.OnSyncProviderPropertyChanged;

            var exchangeProvider = this.synchronizationManager.GetProvider(SynchronizationService.Exchange);
            var toodledoProvider = this.synchronizationManager.GetProvider(SynchronizationService.ToodleDo);
            var outlookActiveSyncProvider = this.synchronizationManager.GetProvider(SynchronizationService.OutlookActiveSync);
            var activeSyncProvider = this.synchronizationManager.GetProvider(SynchronizationService.ActiveSync);
            var vercorsProvider = this.synchronizationManager.GetProvider(SynchronizationService.Vercors);

            this.availableProviders = new ObservableCollection<SyncProviderViewModel>
                                          {
                                              new SyncProviderViewModel(vercorsProvider   ,this.synchronizationManager, true, () => this.OpenProviderSettings(SynchronizationService.Vercors)),
                                              new Office365SyncProviderViewModel(exchangeProvider  ,this.synchronizationManager, false, () => this.OpenProviderSettings(SynchronizationService.Exchange, "office365")),
                                              new ExchangeSyncProviderViewModel(exchangeProvider  ,this.synchronizationManager, false, () => this.OpenProviderSettings(SynchronizationService.Exchange, "exchange2013")),
                                              new SyncProviderViewModel(toodledoProvider  ,this.synchronizationManager, false, () => this.OpenProviderSettings(SynchronizationService.ToodleDo)),
                                              new SyncProviderViewModel(outlookActiveSyncProvider,this.synchronizationManager, false, () => this.OpenProviderSettings(SynchronizationService.OutlookActiveSync)),
                                              new SyncProviderViewModel(activeSyncProvider, this.synchronizationManager, false, () => this.OpenProviderSettings(SynchronizationService.ActiveSync))
                                          };

            this.RefreshCurrentProvider();

            this.debugCommand = new RelayCommand(this.DebugExecute);
            this.showLogCommand = new RelayCommand(this.ShowLogExecute);
            this.openHelpCommand = new RelayCommand(this.OpenHelpExecute);

            this.isLogEnabled = this.Workbook.Settings.GetValue<LogLevel>(CoreSettings.LogLevel) > LogLevel.None;
        }

        private void ShowLogExecute()
        {
            this.NavigationService.FlyoutTo(ViewLocator.LogViewerSettingsPage);
        }

        private void DebugExecute()
        {
            this.NavigationService.FlyoutTo(ViewLocator.SyncAdvancedSyncSettingsPage);            
        }

        private void OpenHelpExecute()
        {
            this.platformService.OpenWebUri(Constants.HelpPageFixSyncIssues);
        }

        private void OpenProviderSettings(SynchronizationService synchronizationService, string parameter = null)
        {
            if (synchronizationService == SynchronizationService.None)
                return;

            switch (synchronizationService)
            {
                case SynchronizationService.ToodleDo:
                    this.NavigationService.FlyoutTo(ViewLocator.SyncToodleDoSettingsPage, parameter);
                    break;
                case SynchronizationService.Exchange:
                case SynchronizationService.ExchangeEws:
                    this.NavigationService.FlyoutTo(ViewLocator.SyncExchangeSettingsPage, parameter);
                    break;
                case SynchronizationService.OutlookActiveSync:
                    this.NavigationService.FlyoutTo(ViewLocator.SyncOutlookActiveSyncSettingsPage, parameter);
                    break;
                case SynchronizationService.ActiveSync:
                    this.NavigationService.FlyoutTo(ViewLocator.SyncActiveSyncSettingsPage, parameter);
                    break;
                case SynchronizationService.Vercors:
                    this.NavigationService.FlyoutTo(ViewLocator.SyncVercorsSettingsPage, parameter);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("synchronizationService");
            }
        }

        private async void RemoveActiveProviderExecute(bool askConfirmation = true)
        {
            bool remove = true;

            if (askConfirmation)
            {
                var result = await this.messageBoxService.ShowAsync(
                    StringResources.Dialog_TitleConfirmation,
                    StringResources.SyncSettingsPage_ConfirmDeleteMessage,
                    DialogButton.OKCancel);

                remove = (result == DialogResult.OK);
            }

            if (remove)
            {
                this.synchronizationManager.Reset();
                await this.synchronizationManager.SaveAsync();

                var result = await this.messageBoxService.ShowAsync(
                    StringResources.Dialog_TitleConfirmation,
                    StringResources.Message_DeleteAllMessage,
                    DialogButton.YesNo);

                if (result == DialogResult.Yes)
                    this.Workbook.RemoveAll();
            }
        }

        private void OnSyncProviderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RefreshCurrentProvider();
        }

        private void RefreshCurrentProvider()
        {
            if (this.synchronizationManager.ActiveProvider != null && this.synchronizationManager.ActiveService != SynchronizationService.None)
            {
                this.ActiveProvider = new SyncProviderViewModel(
                    this.synchronizationManager.ActiveProvider, 
                    this.synchronizationManager, 
                    this.synchronizationManager.ActiveService == SynchronizationService.Vercors,
                    () => this.OpenProviderSettings(this.synchronizationManager.ActiveService), 
                    () => this.RemoveActiveProviderExecute());
            }
            else
                this.ActiveProvider = null;
        }

        public override void Dispose()
        {
            this.synchronizationManager.PropertyChanged -= this.OnSyncProviderPropertyChanged;            
        }
    }
}

using System;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Exchange;
using Chartreuse.Today.Exchange.Model;
using Chartreuse.Today.Exchange.Resources;

namespace Chartreuse.Today.App.Shared.ViewModel.Sync
{
    public class OutlookActiveSyncSettingsViewModel : SyncSettingsViewModelBase
    {
        public string Email
        {
            get
            {
                return this.Settings.GetValue<string>(ExchangeSettings.ActiveSyncEmail);
            }
            set
            {
                if (this.Email != value)
                {
                    this.Settings.SetValue(ExchangeSettings.ActiveSyncEmail, value);
                    this.RaisePropertyChanged("Email");
                }
            }
        }
        
        public string ServerUri
        {
            get
            {
                return this.Settings.GetValue<string>(ExchangeSettings.ActiveSyncServerUri);
            }
            set
            {
                if (this.ServerUri != value)
                {
                    this.Settings.SetValue(ExchangeSettings.ActiveSyncServerUri, value);
                    this.RaisePropertyChanged("ServerUri");
                }
            }
        }

        protected override SynchronizationService SynchronizationService
        {
            get { return SynchronizationService.OutlookActiveSync; }
        }

        public OutlookActiveSyncSettingsViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, ISynchronizationManager synchronizationManager, ITrackingManager trackingManager) 
            : base(workbook, navigationService, messageBoxService, synchronizationManager, trackingManager)
        {
            this.CheckAndUpdateServerUri();
        }

        private void CheckAndUpdateServerUri()
        {
            if (!string.IsNullOrEmpty(this.ServerUri))
            {
                this.ServerUri = this.ServerUri.GetUriCompatibleString();

                if (!this.ServerUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !this.ServerUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    this.ServerUri = "https://" + this.ServerUri;
            }
        }

        protected override async void CheckSettingsExecute()
        {
            using (this.ExecuteBusyAction(ExchangeResources.Exchange_CheckingCredentials))
            {
                // first try with ExchangeEws and Office 365 endpoint
                ISynchronizationProvider provider = this.SynchronizationManager.GetProvider(SynchronizationService.ExchangeEws);
                this.Settings.SetValue(ExchangeSettings.ExchangeServerUri, Constants.Office365Endpoint);
                this.Settings.SetValue(ExchangeSettings.ExchangeEmail, this.Email);
                this.Settings.SetValue(ExchangeSettings.ExchangeUsername, this.Email);
                this.Settings.SetValue(ExchangeSettings.ExchangeVersion, ExchangeServerVersion.ExchangeOffice365);
                this.Settings.SetValue(ExchangeSettings.ExchangeSyncFlaggedItems, false);
                this.Settings.SetValue(ExchangeSettings.ExchangePassword, this.Settings.GetValue<byte[]>(ExchangeSettings.ActiveSyncPassword));

                bool result = await provider.CheckLoginAsync();
                if (result)
                {
                    this.SynchronizationManager.Metadata.Reset();
                    this.SynchronizationManager.ActiveService = SynchronizationService.ExchangeEws;
                    
                    await this.HandleGoodSettings();
                    return;
                }

                // if that doesn't work, fallback to ActiveSync
                if (!result)
                {
                    this.Settings.SetValue<string>(ExchangeSettings.ExchangeServerUri, null);
                    this.Settings.SetValue<string>(ExchangeSettings.ExchangeEmail, null);
                    this.Settings.SetValue<string>(ExchangeSettings.ExchangeUsername, null);
                    this.Settings.SetValue<bool>(ExchangeSettings.ExchangeSyncFlaggedItems, false);
                    this.Settings.SetValue<byte[]>(ExchangeSettings.ExchangePassword, null);

                    this.CheckAndUpdateServerUri();

                    provider = this.SynchronizationManager.GetProvider(SynchronizationService.OutlookActiveSync);

                    provider.OperationFailed += this.OnProviderOperationFailed;
                    result = await provider.CheckLoginAsync();
                    provider.OperationFailed -= this.OnProviderOperationFailed;

                    if (result)
                    {
                        this.SynchronizationManager.Metadata.Reset();
                        this.SynchronizationManager.ActiveService = SynchronizationService.OutlookActiveSync;

                        await this.HandleGoodSettings();
                    }
                }
            }
        }

        private async void OnProviderOperationFailed(object sender, EventArgs<string> e)
        {
            this.HandleBadSettings();

            await this.MessageBoxService.ShowAsync(StringResources.Message_Warning, string.Format(ExchangeResources.Exchange_InvalidCredentialsFormat, e.Item));
        }
    }
}

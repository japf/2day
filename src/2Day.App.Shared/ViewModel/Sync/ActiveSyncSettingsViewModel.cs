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
using Chartreuse.Today.Exchange.Resources;

namespace Chartreuse.Today.App.Shared.ViewModel.Sync
{
    public class ActiveSyncSettingsViewModel : SyncSettingsViewModelBase
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
            get { return SynchronizationService.ActiveSync; }
        }

        public ActiveSyncSettingsViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, ISynchronizationManager synchronizationManager, ITrackingManager trackingManager) 
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
                this.CheckAndUpdateServerUri();

                ISynchronizationProvider provider = this.SynchronizationManager.GetProvider(SynchronizationService.ActiveSync);

                provider.OperationFailed += this.OnProviderOperationFailed;
                var result = await provider.CheckLoginAsync();
                provider.OperationFailed -= this.OnProviderOperationFailed;

                if (result)
                {
                    this.SynchronizationManager.Metadata.Reset();
                    this.SynchronizationManager.ActiveService = SynchronizationService.ActiveSync;

                    await this.HandleGoodSettings();
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

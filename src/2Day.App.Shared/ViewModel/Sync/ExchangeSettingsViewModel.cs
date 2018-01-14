using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ExchangeSettingsViewModel : SyncSettingsViewModelBase
    {
        private const string HttpPrefix = "http://";
        private const string HttpsPrefix = "https://";

        private bool useSSL;
        private bool syncFlaggedItems;
        private string exchangeVersion;

        public string Email
        {
            get 
            {
                return this.Settings.GetValue<string>(ExchangeSettings.ExchangeEmail);
            }
            set
            {
                if (this.Email != value)
                {
                    this.Settings.SetValue(ExchangeSettings.ExchangeEmail, value);
                    this.RaisePropertyChanged("Email");
                    this.RaisePropertyChanged("ShowMicrosoftHelp");
                }
            }
        }

        public bool ShowMicrosoftHelp
        {
            get { return this.Email != null && this.Email.ToLowerInvariant().Contains("@microsoft.com"); }
        }

        public string Username
        {
            get
            {
                return this.Settings.GetValue<string>(ExchangeSettings.ExchangeUsername);
            }
            set
            {
                if (this.Username != value)
                {
                    this.Settings.SetValue(ExchangeSettings.ExchangeUsername, value);
                    this.RaisePropertyChanged("Username");
                }
            }
        }
        
        public string Domain
        {
            get
            {
                return this.Settings.GetValue<string>(ExchangeSettings.ExchangeDomain);
            }
            set
            {
                if (this.Domain != value)
                {
                    this.Settings.SetValue(ExchangeSettings.ExchangeDomain, value);
                    this.RaisePropertyChanged("Domain");
                }
            }
        }

        public string ServerUri
        {
            get
            {
                return this.Settings.GetValue<string>(ExchangeSettings.ExchangeServerUri);
            }
            set
            {
                if (this.ServerUri != value)
                {
                    this.Settings.SetValue(ExchangeSettings.ExchangeServerUri, value);
                    this.RaisePropertyChanged("ServerUri");

                    this.UpdateSSLUsage();
                }
            }
        }

        public bool UseSSL
        {
            get { return this.useSSL; }
            set
            {
                if (this.useSSL != value)
                {
                    this.useSSL = value;
                    this.RaisePropertyChanged("UseSSL");

                    // if the server uri is set but does not match the SSL option update it
                    if (!string.IsNullOrEmpty(this.ServerUri))
                    {
                        if (this.useSSL && this.ServerUri.StartsWith(HttpPrefix))
                            this.ServerUri = this.ServerUri.Replace(HttpPrefix, HttpsPrefix);
                        else if (!this.useSSL && this.ServerUri.StartsWith(HttpsPrefix))
                            this.ServerUri = this.ServerUri.Replace(HttpsPrefix, HttpPrefix);
                    }
                }
            }
        }
        
        public bool SyncFlaggedItems
        {
            get { return this.syncFlaggedItems; }
            set
            {
                if (this.syncFlaggedItems != value)
                {
                    this.syncFlaggedItems = value;
                    this.RaisePropertyChanged("SyncFlaggedItems");
                }
            }
        }
        
        public string ExchangeVersion
        {
            get { return this.exchangeVersion; }
            set
            {
                if (this.exchangeVersion != value)
                {
                    this.exchangeVersion = value;
                    this.RaisePropertyChanged("ExchangeVersion");

                    if (this.exchangeVersion == ExchangeServerVersion.ExchangeOffice365.GetString() &&
                        string.IsNullOrEmpty(this.ServerUri))
                    {
                        this.ServerUri = Constants.Office365Endpoint;
                    }
                    else if (this.exchangeVersion != ExchangeServerVersion.ExchangeOffice365.GetString() &&
                        this.ServerUri != null && this.ServerUri.Equals(Constants.Office365Endpoint, StringComparison.OrdinalIgnoreCase))
                    {
                        this.ServerUri = null;
                    }
                }
            }
        }

        public List<string> ExchangeVersions
        {
            get { return ExchangeServerVersionHelper.ExchangeVersions; }
        }

        protected override SynchronizationService SynchronizationService
        {
            get
            {
                return SynchronizationService.ExchangeEws;
            }
        }

        public ExchangeSettingsViewModel(IWorkbook workbook, INavigationService navigationService, IMessageBoxService messageBoxService, ISynchronizationManager synchronizationManager, ITrackingManager trackingManager)
            : base(workbook, navigationService, messageBoxService, synchronizationManager, trackingManager)
        {
            if (!this.Settings.HasValue(ExchangeSettings.ExchangeVersion))
                this.Settings.SetValue(ExchangeSettings.ExchangeVersion, ExchangeServerVersion.ExchangeOffice365);

            if (!string.IsNullOrEmpty(this.ServerUri))
                this.ServerUri = this.ServerUri.GetUriCompatibleString();

            this.exchangeVersion = ExchangeServerVersionHelper.GetString(this.Settings.GetValue<ExchangeServerVersion>(ExchangeSettings.ExchangeVersion));
            this.syncFlaggedItems = this.Settings.GetValue<bool>(ExchangeSettings.ExchangeSyncFlaggedItems);

            this.UpdateSSLUsage();
        }

        private void UpdateSSLUsage()
        {
            this.UseSSL = !string.IsNullOrEmpty(this.ServerUri) && this.ServerUri.StartsWith(HttpsPrefix) || string.IsNullOrEmpty(this.ServerUri);
        }

        protected override async void CheckSettingsExecute()
        {
            this.Settings.SetValue(ExchangeSettings.ExchangeVersion, ExchangeServerVersionHelper.GetEnum(this.exchangeVersion));
            this.Settings.SetValue(ExchangeSettings.ExchangeSyncFlaggedItems, this.syncFlaggedItems);
                            
            if (string.IsNullOrEmpty(this.Domain) && !string.IsNullOrEmpty(this.Email))
            {
                // try to use the email address to find out what the domain is
                string[] emailComponents = this.Email.Split('@');
                if (emailComponents.Length > 1)
                {
                    this.Domain = emailComponents[1];
                }
            }

            if (!string.IsNullOrEmpty(this.ServerUri))
            {
                this.ServerUri = this.ServerUri.GetUriCompatibleString();

                if (!this.ServerUri.StartsWith(HttpPrefix) && !this.ServerUri.StartsWith(HttpsPrefix))
                {
                    // http prefix is missing add it
                    if (this.UseSSL)
                        this.ServerUri = HttpsPrefix + this.ServerUri;
                    else
                        this.ServerUri = HttpPrefix + this.ServerUri;
                }
            }

            if (string.IsNullOrEmpty(this.Username) && !string.IsNullOrEmpty(this.Email))
            {
                // if the username is empty while the email is not
                // use the email as the username
                this.Username = this.Email;
            }

            using (this.ExecuteBusyAction(ExchangeResources.Exchange_CheckingCredentials))
            {
                SynchronizationService synchronizationService = SynchronizationService.ExchangeEws;
                ISynchronizationProvider provider = this.SynchronizationManager.GetProvider(synchronizationService);
                bool result = false;
                if (provider != null)
                {
                    provider.OperationFailed += this.ProviderOnOperationFailed;
                    result = await provider.CheckLoginAsync();
                    provider.OperationFailed -= this.ProviderOnOperationFailed;
                }

                // notify potential changes  to the server uri
                this.RaisePropertyChanged("ServerUri");
                this.UpdateSSLUsage();

                if (result)
                {
                    this.SynchronizationManager.Metadata.Reset();
                    this.SynchronizationManager.ActiveService = synchronizationService;

                    await this.HandleGoodSettings();
                }
            }
        }

        private async void ProviderOnOperationFailed(object sender, EventArgs<string> eventArgs)
        {
            this.HandleBadSettings();
            await this.MessageBoxService.ShowAsync(StringResources.Message_Warning, string.Format(ExchangeResources.Exchange_InvalidCredentialsFormat, eventArgs.Item));
        }
    }
}

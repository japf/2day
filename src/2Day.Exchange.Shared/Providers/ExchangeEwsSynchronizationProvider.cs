using System;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Exchange.Ews;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Chartreuse.Today.Exchange.Resources;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange.Providers
{
    public class ExchangeEwsSynchronizationProvider : ExchangeSynchronizationProviderBase<EwsSyncService>
    {
        public override SynchronizationService Service
        {
            get { return SynchronizationService.ExchangeEws; }
        }

        public override SyncFeatures SupportedFeatures
        {
            get
            {
                return SyncFeatures.Title |
                       SyncFeatures.DueDate |
                       SyncFeatures.Priority |
                       SyncFeatures.Folder |
                       SyncFeatures.Recurrence |
                       SyncFeatures.Reminders |
                       SyncFeatures.Notes |
                       SyncFeatures.Progress |
                       SyncFeatures.StartDate;
            }
        }

        public override string LoginInfo
        {
            get { return this.Workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeEmail); }
        }

        public override string ServerInfo
        {
            get { return this.Workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeServerUri); }
        }

        public override string Name
        {
            get
            {
                string serverInfo = this.ServerInfo;
                if (!string.IsNullOrWhiteSpace(serverInfo) && serverInfo.ToLower().Contains("office365"))
                    return "Office 365";
                else
                    return "Microsoft Exchange";
            }
        }

        public override string Headline
        {
            get { return ExchangeResources.Exchange_Headline; }
        }

        public override string Description
        {
            get { return ExchangeResources.Exchange_Description; }
        }

        public override string DefaultFolderName
        {
            get { return "Exchange"; }
        }

        public ExchangeEwsSynchronizationProvider(ISynchronizationManager synchronizationManager, ICryptoService crypto) 
            : base(synchronizationManager, new EwsSyncService(synchronizationManager.Workbook), crypto)
        {
        }

        protected override ExchangeInfoBuild BuildExchangeConnectionInfo()
        {
            var settings = this.Workbook.Settings;

            string plainPassword = this.CryptoService.Decrypt(settings.GetValue<byte[]>(ExchangeSettings.ExchangePassword));
            if (string.IsNullOrEmpty(plainPassword))
            {
                this.OnSynchronizationFailed(ExchangeResources.Exchange_EmptyPassword);
                return new ExchangeInfoBuild { IsValid = false };
            }

            var connectionInfo = new ExchangeConnectionInfo
            {
                Username = settings.GetValue<string>(ExchangeSettings.ExchangeUsername),
                Password = plainPassword,
                Email = settings.GetValue<string>(ExchangeSettings.ExchangeEmail),
                Domain = settings.GetValue<string>(ExchangeSettings.ExchangeDomain),
                TimeZoneStandardName = TimeZoneInfo.Local.StandardName,
                UtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours,
                SyncFlaggedItems = settings.GetValue<bool>(ExchangeSettings.ExchangeSyncFlaggedItems)
            };

            if (string.IsNullOrEmpty(connectionInfo.Username))
            {
                this.OnSynchronizationFailed(ExchangeResources.Exchange_EmptyUsername);
                return new ExchangeInfoBuild { IsValid = false };
            }
            else if (string.IsNullOrEmpty(plainPassword))
            {
                this.OnSynchronizationFailed(ExchangeResources.Exchange_EmptyPassword);
                return new ExchangeInfoBuild { IsValid = false };
            }
            else if (string.IsNullOrEmpty(connectionInfo.Email))
            {
                this.OnSynchronizationFailed(ExchangeResources.Exchange_EmptyEmail);
                return new ExchangeInfoBuild { IsValid = false };
            }

            var uriString = settings.GetValue<string>(ExchangeSettings.ExchangeServerUri);
            if (!string.IsNullOrEmpty(uriString))
                connectionInfo.ServerUri = SafeUri.Get(uriString, UriKind.RelativeOrAbsolute);

            connectionInfo.Version = settings.GetValue<ExchangeServerVersion>(ExchangeSettings.ExchangeVersion);

            return new ExchangeInfoBuild { IsValid = true, ConnectionInfo = connectionInfo };
        }

        protected override void ClearSettings()
        {
            var settings = this.Workbook.Settings;

            settings.SetValue<string>(ExchangeSettings.ExchangeUsername, null);
            settings.SetValue<string>(ExchangeSettings.ExchangeEmail, null);
            settings.SetValue<byte[]>(ExchangeSettings.ExchangePassword, null);
            settings.SetValue<string>(ExchangeSettings.ExchangeServerUri, null);
            settings.SetValue<string>(ExchangeSettings.ExchangeDomain, null);
        }

        protected override void UpdateSettingsAfterSync(ExchangeConnectionInfo connectionInfo, string serverUri)
        {
            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangeServerUri, serverUri);
        }

        protected override bool HaveSameId(ITask task, ExchangeTask exchangeTask)
        {
            return task.SyncId != null && exchangeTask.Id != null && task.SyncId.GetEwsItemIdentifier().Id == exchangeTask.Id.GetEwsItemIdentifier().Id;
        }
    }
}

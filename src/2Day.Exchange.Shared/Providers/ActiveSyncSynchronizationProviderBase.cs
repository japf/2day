using System;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Chartreuse.Today.Exchange.Resources;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange.Providers
{
    public abstract class ActiveSyncSynchronizationProviderBase : ExchangeSynchronizationProviderBase<ActiveSyncService>
    {
        private readonly string deviceId;
        
        public override SyncFeatures SupportedFeatures
        {
            get
            {
                return SyncFeatures.Title |
                       SyncFeatures.DueDate |
                       SyncFeatures.Priority |
                       SyncFeatures.Folder |
                       SyncFeatures.Recurrence |
                       SyncFeatures.Notes |
                       SyncFeatures.StartDate;
            }
        }

        public override string LoginInfo
        {
            get { return this.Workbook.Settings.GetValue<string>(ExchangeSettings.ActiveSyncEmail); } 
        }

        public override string ServerInfo
        {
            get { return this.Workbook.Settings.GetValue<string>(ExchangeSettings.ActiveSyncServerUri); }
        }

        public override string FolderInfo
        {
            get { return this.SyncService.TaskFolderName; }
        }

        protected ActiveSyncSynchronizationProviderBase(ISynchronizationManager synchronizationManager, ICryptoService crypto, string deviceId)
            : base(synchronizationManager, new ActiveSyncService(), crypto)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException("deviceId");

            this.deviceId = deviceId;
        }

        protected override ExchangeInfoBuild BuildExchangeConnectionInfo()
        {
            var settings = this.Workbook.Settings;

            byte[] encryptedPasswordData = settings.GetValue<byte[]>(ExchangeSettings.ActiveSyncPassword);

            string serverUri = settings.GetValue<string>(ExchangeSettings.ActiveSyncServerUri);

            var connectionInfo = new ExchangeConnectionInfo
            {
                Username = settings.GetValue<string>(ExchangeSettings.ActiveSyncEmail),
                EncryptedPassword = encryptedPasswordData,
                Password = this.CryptoService.Decrypt(encryptedPasswordData),
                Email = settings.GetValue<string>(ExchangeSettings.ActiveSyncEmail),
                Domain = string.Empty,
                ServerUri = StringExtension.TryCreateUri(serverUri),
                DeviceId = this.deviceId,
                PolicyKey = settings.GetValue<uint>(ExchangeSettings.ActiveSyncPolicyKey)
            };

            if (connectionInfo.EncryptedPassword == null || string.IsNullOrEmpty(connectionInfo.Password))
            {
                this.OnSynchronizationFailed(ExchangeResources.Exchange_EmptyPassword);
                return new ExchangeInfoBuild { IsValid = false };
            }
            
            if (string.IsNullOrEmpty(connectionInfo.Email))
            {
                this.OnSynchronizationFailed(ExchangeResources.Exchange_EmptyEmail);
                return new ExchangeInfoBuild { IsValid = false };
            }

            return new ExchangeInfoBuild { IsValid = true, ConnectionInfo = connectionInfo };
        }

        protected override void ClearSettings()
        {
            var settings = this.Workbook.Settings;

            settings.SetValue<string>(ExchangeSettings.ActiveSyncEmail, null);
            settings.SetValue<byte[]>(ExchangeSettings.ActiveSyncPassword, null);
            settings.SetValue<string>(ExchangeSettings.ActiveSyncServerUri, null);
        }

        protected override void UpdateSettingsAfterSync(ExchangeConnectionInfo connectionInfo, string serverUri)
        {
            this.Workbook.Settings.SetValue<uint>(ExchangeSettings.ActiveSyncPolicyKey, connectionInfo.PolicyKey);
            this.Workbook.Settings.SetValue<string>(ExchangeSettings.ActiveSyncServerUri, serverUri);
        }
    }
}

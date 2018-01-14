using System;
using System.IO;
using System.Reflection;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Chartreuse.Today.Exchange.Resources;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange.Providers
{
    public class ExchangeSynchronizationProvider : ExchangeSynchronizationProviderBase<ExchangeSyncService>
    {
        private const string PublicKeyTxtName = "Chartreuse.Today.Exchange.ServerPublicKey.txt";

        public override SynchronizationService Service
        {
            get { return SynchronizationService.Exchange; }
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

        public override string FolderInfo
        {
            get { return this.SyncService.TaskFolderName; }
        }

        public override string Name
        {
            get { return "Microsoft Exchange"; }
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

        public ExchangeSynchronizationProvider(ISynchronizationManager synchronizationManager, ICryptoService crypto)
            : base(synchronizationManager, new ExchangeSyncService(Constants.AzureExchangeServiceAdress), crypto)
        {
        }

        protected override ExchangeInfoBuild BuildExchangeConnectionInfo()
        {
            ISettings settings = this.Workbook.Settings;
            
            // encrypt the password using the public RSA key of the server
            Stream publicKeyStream =  Assembly.Load(new AssemblyName("2Day.Exchange.Shared")).GetManifestResourceStream(PublicKeyTxtName);
            string publicKey = new StreamReader(publicKeyStream).ReadToEnd();
            
            byte[] encryptedPassword = settings.GetValue<byte[]>(ExchangeSettings.ExchangePassword);
            if (encryptedPassword == null || encryptedPassword.Length == 0)
            {
                this.OnSynchronizationFailed(ExchangeResources.Exchange_EmptyPassword);
                return new ExchangeInfoBuild { IsValid = false };
            }
            string plainPassword = this.CryptoService.Decrypt(encryptedPassword);
            
            var encryptedPasswordData = this.CryptoService.RsaEncrypt(publicKey, plainPassword);

            var connectionInfo = new ExchangeConnectionInfo
            {
                Username = settings.GetValue<string>(ExchangeSettings.ExchangeUsername),
                EncryptedPassword = encryptedPasswordData,
                Email = settings.GetValue<string>(ExchangeSettings.ExchangeEmail),
                Domain = settings.GetValue<string>(ExchangeSettings.ExchangeDomain),
                TimeZoneStandardName = TimeZoneInfo.Local.StandardName,
                UtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours,
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
            {
                try
                {
                    connectionInfo.ServerUri = SafeUri.Get(uriString, UriKind.RelativeOrAbsolute);
                }
                catch (Exception)
                {
                    this.OnSynchronizationFailed(ExchangeResources.Exchange_InvalidServerUri);                    
                    return new ExchangeInfoBuild { IsValid = false };
                }
            }

            connectionInfo.Version = settings.GetValue<ExchangeServerVersion>(ExchangeSettings.ExchangeVersion);
            connectionInfo.IsBackgroundSync = this.Manager.IsBackground;
            connectionInfo.Source = this.Manager.Platform;

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
            this.Workbook.Settings.SetValue<string>(ExchangeSettings.ExchangeServerUri, serverUri);
        }
    }
}

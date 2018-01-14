using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Exchange;
using Chartreuse.Today.Exchange.Providers;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Chartreuse.Today.Exchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Tools
{
    public class TestExchangeSyncHelper
    {
        public static string Email { get; set; }
        public static string UserName { get; set; }
        public static string Domain { get; set; }
        public static string Server { get; set; }
        public static ExchangeServerVersion Version { get; set; }
        public static string Password { get; set; }

        private readonly IWorkbook workbook;
        private readonly ICryptoService cryptoService;
        private readonly IExchangeSyncService service;
        private readonly string deviceId;

        public const string Office365AccountEmail = "user@company.onmicrosoft.com";
        public const string Office365AccountPassword = "password";
        public const string Office365AccountDomain = "company.onmicrosoft.com";
        public const string Office365AccountServer = "https://outlook.office365.com/EWS/Exchange.asmx";

        public const string OutlookDotComAccountEmail = "user@outlook.com";
        public const string OutlookDotComAccountPassword = "password";
        public const string OutlookDotComAccountDomain = "outlook.com";
        public const string OutlookDotComAccountServer = "https://outlook.office365.com/EWS/Exchange.asmx";

        public TestExchangeSyncHelper(IWorkbook workbook, ICryptoService cryptoService, IExchangeSyncService service, string deviceId = null)
        {
            this.workbook = workbook;
            this.cryptoService = cryptoService;
            this.service = service;
            this.deviceId = deviceId;
        }

        public static void SetExchangeSettings(TestContext testContext, IWorkbook workbook, ICryptoService cryptoService)
        {
            string syncMode = testContext.Properties["SyncMode"] as string;

            if (syncMode == "ExchangeEwsOutlookDotCom")
            {
                Email = OutlookDotComAccountEmail;
                UserName = OutlookDotComAccountEmail;
                Domain = OutlookDotComAccountDomain;
                Server = OutlookDotComAccountServer;
                Version = ExchangeServerVersion.ExchangeOffice365;
                Password = OutlookDotComAccountPassword;
            }
            else
            {
                Email = Office365AccountEmail;
                UserName = Office365AccountEmail;
                Domain = Office365AccountDomain;
                Server = Office365AccountServer;
                Version = ExchangeServerVersion.ExchangeOffice365;
                Password = Office365AccountPassword;
            }

            if (workbook != null && cryptoService != null)
            {
                workbook.Settings.SetValue(ExchangeSettings.ExchangeEmail, Email);
                workbook.Settings.SetValue(ExchangeSettings.ExchangeUsername, UserName);
                workbook.Settings.SetValue(ExchangeSettings.ExchangeDomain, Domain);
                workbook.Settings.SetValue(ExchangeSettings.ExchangePassword, cryptoService.Encrypt(Password));
                workbook.Settings.SetValue(ExchangeSettings.ExchangeVersion, Version);
                workbook.Settings.SetValue(ExchangeSettings.ExchangeServerUri, Server);
                workbook.Settings.SetValue(ExchangeSettings.ExchangeSyncFlaggedItems, true);
            }
        }

        public ExchangeConnectionInfo GetConnectionInfo()
        {
            if (this.service is ExchangeSyncService)
            {
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Chartreuse.Today.Sync.Test.ServerPublicKey.xml");
                string publicKey = new StreamReader(stream).ReadToEnd();
                byte[] password = this.workbook.Settings.GetValue<byte[]>(ExchangeSettings.ExchangePassword);

                var connectionInfo = new ExchangeConnectionInfo
                {
                    Email = this.workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeEmail),
                    Username = this.workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeUsername),
                    Domain = this.workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeDomain),
                    EncryptedPassword = this.cryptoService.RsaEncrypt(publicKey, this.cryptoService.Decrypt(password)),
                    Password = this.cryptoService.Decrypt(password),
                    Version = this.workbook.Settings.GetValue<ExchangeServerVersion>(ExchangeSettings.ExchangeVersion),
                    DeviceId = this.deviceId
                };

                string serverUri = this.workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeServerUri);
                if (!string.IsNullOrEmpty(serverUri))
                    connectionInfo.ServerUri = SafeUri.Get(serverUri);

                return connectionInfo;
            }
            else if (this.service is EwsSyncService)
            {
                byte[] password = this.workbook.Settings.GetValue<byte[]>(ExchangeSettings.ExchangePassword);

                var connectionInfo = new ExchangeConnectionInfo
                {
                    Email = this.workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeEmail),
                    Username = this.workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeUsername),
                    Domain = this.workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeDomain),
                    EncryptedPassword = password,
                    Password = this.cryptoService.Decrypt(password),
                    Version = this.workbook.Settings.GetValue<ExchangeServerVersion>(ExchangeSettings.ExchangeVersion),
                    DeviceId = this.deviceId,
                    SyncFlaggedItems = true
                };

                string serverUri = this.workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeServerUri);
                if (!string.IsNullOrEmpty(serverUri))
                    connectionInfo.ServerUri = SafeUri.Get(serverUri);

                return connectionInfo;
            } 
            else if (this.service is ActiveSyncService)
            {
                var connectionInfo = new ExchangeConnectionInfo
                {
                    Email = this.workbook.Settings.GetValue<string>(ExchangeSettings.ActiveSyncEmail),
                    EncryptedPassword = this.workbook.Settings.GetValue<byte[]>(ExchangeSettings.ActiveSyncPassword),
                    Password = this.cryptoService.Decrypt(this.workbook.Settings.GetValue<byte[]>(ExchangeSettings.ActiveSyncPassword)),
                    DeviceId = this.deviceId,
                };

                string serverUri = this.workbook.Settings.GetValue<string>(ExchangeSettings.ActiveSyncServerUri);
                if (!string.IsNullOrEmpty(serverUri))
                    connectionInfo.ServerUri = SafeUri.Get(serverUri);

                return connectionInfo;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public Task BeforeTestCoreAsync(ISynchronizationManager manager)
        {
            manager.Metadata.ProviderDatas[ExchangeSynchronizationProviderBase<ExchangeSyncService>.CacheSyncState] = null;

            return Task.FromResult(0);
        }

        public async Task RemoteDeleteAllTasksAsync()
        {
            var connectionInfo = this.GetConnectionInfo();
            var data = await this.Sync(connectionInfo);

            if (data.ChangeSet.AddedTasks.Count > 0)
            {
                var changeset = this.GetChangeSet(deleted: data.ChangeSet.AddedTasks);
                await this.Sync(connectionInfo, changeset);
            }
        }
        
        private async Task<ExchangeSyncResult> Sync(ExchangeConnectionInfo connectionInfo, ExchangeChangeSet changeSet = null)
        {
            if (changeSet == null)
                changeSet = new ExchangeChangeSet();

            var data = await this.service.ExecuteFirstSyncAsync(connectionInfo, changeSet);

            string message = string.Format("Operation success: {0} status: {1}", data.AuthorizationResult.IsOperationSuccess, data.AuthorizationResult.AuthorizationStatus);

            Assert.IsTrue(data.AuthorizationResult.IsOperationSuccess, message);
            Assert.AreEqual(ExchangeAuthorizationStatus.OK, data.AuthorizationResult.AuthorizationStatus, message);

            return data;
        }

        private ExchangeChangeSet GetChangeSet(IEnumerable<ExchangeTask> deleted)
        {
            var changeset = new ExchangeChangeSet();

            foreach (var exchangeTask in deleted)
                changeset.DeletedTasks.Add(new ServerDeletedAsset(exchangeTask.Id));
            return changeset;
        }

        private ExchangeChangeSet GetChangeSet(ExchangeTask added = null, ExchangeTask modified = null, IEnumerable<string> deleted = null)
        {
            var changeset = new ExchangeChangeSet();

            if (added != null)
                changeset.AddedTasks.Add(added);

            if (modified != null)
                changeset.ModifiedTasks.Add(modified);

            if (deleted != null)
                deleted.ForEach(d => changeset.DeletedTasks.Add(new ServerDeletedAsset(d)));

            return changeset;
        }

        public async Task RemoteAddTask(ITask task)
        {
            var connectionInfo = this.GetConnectionInfo();
            var changeSet = this.GetChangeSet(task.ToExchangeTask(true, TaskProperties.All));
            var data = await this.Sync(connectionInfo, changeSet);
        }

        public async Task RemoteEditTask(ITask task)
        {
            var connectionInfo = this.GetConnectionInfo();
            var changeSet = this.GetChangeSet(null, task.ToExchangeTask(true, TaskProperties.All));
            var data = await this.Sync(connectionInfo, changeSet);
        }

        public async Task RemoteDeleteTask(ITask task)
        {
            var connectionInfo = this.GetConnectionInfo();
            var changeSet = this.GetChangeSet(null, null, new [] { task.SyncId });
            var data = await this.Sync(connectionInfo, changeSet);
        }

        public async Task RemoteDeleteTasks(IEnumerable<ITask> tasks)
        {
            var connectionInfo = this.GetConnectionInfo();
            var changeSet = this.GetChangeSet(null, null, tasks.Select(t => t.SyncId));
            var data = await this.Sync(connectionInfo, changeSet);
        }
    }
}

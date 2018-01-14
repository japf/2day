using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Exchange;
using Chartreuse.Today.Exchange.ActiveSync.Commands;
using Chartreuse.Today.Exchange.Providers;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Chartreuse.Today.Sync.Test.Tools;
using Chartreuse.Today.Exchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Runners
{
    public class ActiveSyncTestRunner : TestRunnerBase<ActiveSyncSynchronizationProvider>
    {
        private static int accountIndex = 0;
        private static int deviceIndex = 0;

        private static readonly List<Tuple<string, string, string>> accounts = new List<Tuple<string, string, string>>
        {
            new Tuple<string, string, string>("user@company.onmicrosoft.com", "password", "https://outlook.office365.com"),
            }; 

        private ActiveSyncService service;
        private TestExchangeSyncHelper helper;
        private static string deviceId;

        public static string DeviceId
        {
            get
            {
                if (string.IsNullOrEmpty(deviceId))
                    CreateNewDeviceId();

                return deviceId;
            }
            set { deviceId = value; }
        }

        public override string Password
        {
            get { return this.CryptoService.Decrypt(this.Workbook.Settings.GetValue<byte[]>(ExchangeSettings.ActiveSyncPassword)); }
            set { this.Workbook.Settings.SetValue(ExchangeSettings.ActiveSyncPassword, this.CryptoService.Encrypt(value)); }
        }

        public override bool SupportContext
        {
            get { return false; }
        }

        public override bool SupportFolder
        {
            get { return false; }
        }

        public override bool SupportAlarm
        {
            get { return true; }
        }

        public override bool SupportTag
        {
            get { return false; }
        }

        public TestExchangeSyncHelper Helper
        {
            get { return this.helper; }
        }

        static ActiveSyncTestRunner()
        {
            SyncCommandResult.InvalidStatusFound += (s, e) =>
            {
                Assert.Fail($"Invalid status: {e.Item}");
            };
        }

        public ActiveSyncTestRunner(string testName)
            : base(SynchronizationService.OutlookActiveSync, testName, "provider-eas-data" + accounts[accountIndex].Item1 + ".json")
        {
            this.Workbook.Settings.SetValue(ExchangeSettings.ActiveSyncEmail, accounts[accountIndex].Item1);
            this.Workbook.Settings.SetValue(ExchangeSettings.ActiveSyncPassword, this.CryptoService.Encrypt(accounts[accountIndex].Item2));
            this.Workbook.Settings.SetValue(ExchangeSettings.ActiveSyncServerUri, accounts[accountIndex].Item3);

            accountIndex++;
            if (accountIndex > accounts.Count - 1)
                accountIndex = 0;
        }

        public override void BeforeTestCore()
        {
            this.service = this.Provider.ExchangeService;
            this.helper = new TestExchangeSyncHelper(this.Workbook, this.CryptoService, this.service, deviceId: DeviceId);
        }

        public override void AfterTestCore()
        {
            CreateNewDeviceId();
        }

        private static void CreateNewDeviceId()
        {
            deviceIndex++;
            if (deviceIndex > 9)
                deviceIndex = 0;
        }

        public override async Task RemoteDeleteAllTasks()
        {
            await this.helper.RemoteDeleteAllTasksAsync();
        }
        
        public override Task<List<string>> RemoteGetAllFolders()
        {
            return Task.FromResult(new List<string>());
        }

        public override Task RemoteAddFolder(string name)
        {
            return Task.FromResult(0);
        }

        public override Task RemoteDeleteFolder(string name)
        {
            return Task.FromResult(0);
        }

        public override Task RemoteEditFolder(string id, string newName)
        {
            throw new System.NotImplementedException();
        }

        public override async Task RemoteAddTask(ITask task)
        {
            await this.helper.RemoteAddTask(task);
        }

        public override async Task RemoteEditTask(ITask task)
        {
            await this.helper.RemoteEditTask(task);
        }

        public override async Task RemoteDeleteTask(ITask task)
        {
            await this.helper.RemoteDeleteTask(task);
        }

        public override async Task RemoteDeleteTasks(IEnumerable<ITask> tasks)
        {
            await this.helper.RemoteDeleteTasks(tasks);
        }

        public ExchangeConnectionInfo GetConnectionInfo()
        {
            return this.helper.GetConnectionInfo();
        }
    }
}

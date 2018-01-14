using System.Collections.Generic;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Exchange;
using Chartreuse.Today.Exchange.Ews;
using Chartreuse.Today.Exchange.Providers;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Chartreuse.Today.Sync.Test.Tools;
using Chartreuse.Today.Exchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Runners
{
    public class ExchangeEwsTestRunner : TestRunnerBase<ExchangeEwsSynchronizationProvider>
    {
        private EwsSyncService service;
        private TestExchangeSyncHelper helper;

        public override string Password
        {
            get { return this.CryptoService.Decrypt(this.Workbook.Settings.GetValue<byte[]>(ExchangeSettings.ExchangePassword)); }
            set { this.Workbook.Settings.SetValue(ExchangeSettings.ExchangePassword, this.CryptoService.Encrypt(value)); }
        }

        public override bool SupportContext
        {
            get { return false; }
        }

        public override bool SupportAlarm
        {
            get { return true; }
        }

        public override bool SupportFolder
        {
            get { return false; }
        }

        public override bool SupportTag
        {
            get { return false; }
        }

        public static bool FailOnAnyInvalidResponseMessage { get; set; }

        static ExchangeEwsTestRunner()
        {
            FailOnAnyInvalidResponseMessage = true;
            EwsResponseParserBase.OnInvalidResponse += (s, e) =>
            {
                if (FailOnAnyInvalidResponseMessage)
                    Assert.Fail("Invalid response: " + e.Item);
            };
        }

        public ExchangeEwsTestRunner(TestContext testContext, string testName)
            : base(SynchronizationService.ExchangeEws, testName, "provider-exchange-ews-data.json")
        {
            TestExchangeSyncHelper.SetExchangeSettings(testContext, this.Workbook, this.CryptoService);
        }

        public override void BeforeTestCore()
        {
            this.service = this.Provider.ExchangeService; 
            this.helper = new TestExchangeSyncHelper(this.Workbook, this.CryptoService, this.service);
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
            throw new System.NotImplementedException();
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

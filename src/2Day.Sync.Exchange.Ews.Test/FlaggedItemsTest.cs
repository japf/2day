using System;
using System.Threading.Tasks;
using Chartreuse.Today.Exchange;
using Chartreuse.Today.Exchange.Ews;
using Chartreuse.Today.Exchange.Providers;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Chartreuse.Today.Sync.Test.Ews;
using Chartreuse.Today.Sync.Test.Runners;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Exchange.Ews.Test
{
    [TestClass]
    public class FlaggedItemsTest : TestCaseBase<ExchangeEwsSynchronizationProvider, ExchangeEwsTestRunner>
    {
        private EwsSyncServer server;
        private EwsSyncService syncService;

        protected override void OnTestInitialize()
        {
            var settings = new EwsRequestSettings(
                TestExchangeSyncHelper.Email,
                TestExchangeSyncHelper.UserName,
                TestExchangeSyncHelper.Password,
                TestExchangeSyncHelper.Server);

            this.server = new EwsSyncServer(settings);
            this.syncService = new EwsSyncService(this.Workbook);
        }

        [TestMethod]
        public async Task Fetch_flagged_item()
        {
            // setup
            var helper = new FlagedItemsHelper(this.server, this.syncService);
            var subject = "email " + DateTime.Now.ToString("g");
            var ewsTask = await helper.CreateFlaggedItem(subject);

            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangeSyncFlaggedItems, true);

            // act
            await this.SyncDelta();

            // check
            Assert.AreEqual(1, this.Workbook.Tasks.Count);
            Assert.AreEqual(subject, this.Workbook.Tasks[0].Title);
            Assert.AreEqual(ewsTask.DueDate, this.Workbook.Tasks[0].Due);
            Assert.AreEqual(ewsTask.Categories[0], this.Workbook.Tasks[0].Folder.Name);

            // act - one more time because first iteration had a bug where item where removed after second sync :-)
            await this.SyncDelta();

            // check
            Assert.AreEqual(1, this.Workbook.Tasks.Count);
            Assert.AreEqual(subject, this.Workbook.Tasks[0].Title);
            Assert.AreEqual(ewsTask.DueDate, this.Workbook.Tasks[0].Due);
            Assert.AreEqual(ewsTask.Categories[0], this.Workbook.Tasks[0].Folder.Name);
        }

        [TestMethod]
        public async Task Update_flagged_item()
        {
            // setup
            var helper = new FlagedItemsHelper(this.server, this.syncService);
            var subject = "email " + DateTime.Now.ToString("g");
            await helper.CreateFlaggedItem(subject);

            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangeSyncFlaggedItems, true);

            await this.SyncDelta();
            Assert.AreEqual(1, this.Workbook.Tasks.Count);

            // act
            var task = this.Workbook.Tasks[0];
            task.Title = "new title";
            task.Due = task.Due.Value.AddDays(1);
            task.Start = task.Due;

            await this.SyncFull();

            Assert.AreEqual(1, this.Workbook.Tasks.Count);
            Assert.AreEqual(task.Title, this.Workbook.Tasks[0].Title);
            AssertEx.DateAreEquals(task.Due, this.Workbook.Tasks[0].Due);
            AssertEx.DateAreEquals(task.Start, this.Workbook.Tasks[0].Start);
        }

        [TestMethod]
        public async Task Complete_flagged_item()
        {
            // setup
            var helper = new FlagedItemsHelper(this.server, this.syncService);
            var subject = "email " + DateTime.Now.ToString("g");
            await helper.CreateFlaggedItem(subject);

            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangeSyncFlaggedItems, true);

            await this.SyncDelta();
            Assert.AreEqual(1, this.Workbook.Tasks.Count);

            // act
            var task = this.Workbook.Tasks[0];
            task.IsCompleted = true;

            await this.SyncFull();

            Assert.AreEqual(0, this.Workbook.Tasks.Count);
        }

        [TestMethod]
        public async Task Delete_flagged_item()
        {
            // setup
            var helper = new FlagedItemsHelper(this.server, this.syncService);
            var subject = "email " + DateTime.Now.ToString("g");
            await helper.CreateFlaggedItem(subject);

            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangeSyncFlaggedItems, true);

            await this.SyncDelta();
            Assert.AreEqual(1, this.Workbook.Tasks.Count);

            // act
            this.Workbook.Tasks[0].Delete();

            // check
            await this.SyncFull();
            Assert.AreEqual(0, this.Workbook.Tasks.Count);
        }
    }
}

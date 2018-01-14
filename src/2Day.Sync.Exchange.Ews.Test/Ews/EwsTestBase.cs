using System;
using System.Threading.Tasks;
using Chartreuse.Today.Exchange.Ews;
using Chartreuse.Today.Exchange.Ews.Commands;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Ews
{
    [TestClass]
    public abstract class EwsTestBase : TestCaseBase
    {
        protected EwsRequestSettings settings;
        protected EwsSyncServer server;
        protected EwsSyncService syncService;

        protected GetFolderIdentifiersResult folderIdentifiers;
        
        [TestInitialize]
        public override void Initialize()
        {
            var testRunner = this.CreateTestRunner(this.TestContext.TestName);
            var workbook = testRunner.Workbook;

            TestExchangeSyncHelper.SetExchangeSettings(this.TestContext, null, null);

            this.settings = new EwsRequestSettings(
                TestExchangeSyncHelper.Email,
                TestExchangeSyncHelper.UserName,
                TestExchangeSyncHelper.Password,
                TestExchangeSyncHelper.Server);
            this.server = new EwsSyncServer(this.settings);
            this.syncService = new EwsSyncService(workbook);

            this.server.GetRootFolderIdentifiersAsync().ContinueWith(r => this.folderIdentifiers = r.Result).Wait();

            TestLogHandler.Initialize($"ews-trace-{this.TestContext.TestName}");

            this.DeleteAllTasksAsync().Wait();
            this.DeleteAllInboxEmailsAsync().Wait();
            this.DeleteAllDeletedItemsAsync().Wait();
        }

        protected async Task DeleteAllTasksAsync()
        {
            var itemIdentifiers = await this.server.EnumerateFolderContentAsync(EwsKnownFolderIdentifiers.Tasks);
            Assert.IsNotNull(itemIdentifiers, "itemIdentifiers != null");

            if (itemIdentifiers.Count > 0)
            {
                var result = await this.server.DeleteItemsAsync(itemIdentifiers);
                Assert.IsNotNull(result, "result != null");
            }
        }

        protected async Task DeleteAllInboxEmailsAsync()
        {
            var itemIdentifiers = await this.server.EnumerateFolderContentAsync(EwsKnownFolderIdentifiers.Inbox);
            Assert.IsNotNull(itemIdentifiers, "itemIdentifiers != null");

            if (itemIdentifiers.Count > 0)
            {
                var result = await this.server.DeleteItemsAsync(itemIdentifiers);
                Assert.IsNotNull(result, "result != null");
            }
        }

        protected async Task DeleteAllDeletedItemsAsync()
        {
            var itemIdentifiers = await this.server.EnumerateFolderContentAsync(EwsKnownFolderIdentifiers.DeletedItems);
            Assert.IsNotNull(itemIdentifiers, "itemIdentifiers != null");

            if (itemIdentifiers.Count > 0)
            {
                await this.server.HardDeleteItemsAsync(itemIdentifiers);
            }
        }

        protected static EwsTask CreateSampleEwsTask(bool titleOnly = false)
        {
            var title = $"task-{DateTime.Now.ToString("T")}-{Guid.NewGuid()}";
            var due = DateTime.Now.AddDays(5);
            var reminder = due.AddDays(-3);
            var start = due.AddDays(-2);
            var ordinal = due.AddDays(-1);
            var complete = due.AddDays(-0.5);

            var ewsTask = new EwsTask
            {
                Subject = title,
            };

            if (!titleOnly)
            {
                ewsTask.Importance = EwsImportance.High;
                ewsTask.BodyType = EwsBodyType.Text;
                ewsTask.Body = "body content";
                ewsTask.DueDate = due;
                ewsTask.ReminderDate = reminder;
                ewsTask.ReminderIsSet = true;
                ewsTask.StartDate = start;
                ewsTask.OrdinalDate = ordinal;
                ewsTask.Categories = new[] {"category1", "category2"};
                ewsTask.Complete = true;
                ewsTask.CompleteDate = complete;
            }

            return ewsTask;
        }
    }
}

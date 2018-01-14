#if SYNC_EXCHANGEEWS || SYNC_EXCHANGEWEBAPI
using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Exchange;
using Chartreuse.Today.Exchange.Ews;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Exchange.Providers;
using Chartreuse.Today.Exchange.Providers.SyncService;
using Chartreuse.Today.Sync.Test.Runners;
using Chartreuse.Today.Sync.Test.Tools;
using Chartreuse.Today.Exchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.TestCase.Exchange
{
    [TestClass]
#if SYNC_EXCHANGEWEBAPI
    public class ExchangeTest : TestCaseBase<ExchangeSynchronizationProvider, ExchangeTestRunner>
#else
    public class ExchangeTest : TestCaseBase<ExchangeEwsSynchronizationProvider, ExchangeEwsTestRunner>
#endif
    {
        public async Task Get_tasks_from_multiple_Exchange_folders()
        {
            if (this.Service != SynchronizationService.ExchangeEws)
            {
                Assert.Inconclusive();
                return;
            }

            var server = new EwsSyncServer(this.Runner.GetConnectionInfo().CreateEwsSettings());

            var result = await server.GetSubFoldersAsync(EwsKnownFolderIdentifiers.Tasks);
            if (result.Folders.Count > 0)
                await server.DeleteFoldersAsync(result.Folders);

            var folders = await server.CreateFoldersAsync(new[] {"folder1", "folder2 "}, EwsKnownFolderIdentifiers.Tasks);
            var folder1 = folders.Identifiers[0];
            var folder2 = folders.Identifiers[1];

            await server.CreateItemAsync(new EwsTask {Subject = "task1"}, folder1);
            await server.CreateItemAsync(new EwsTask {Subject = "task2"}, folder2);

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task1", "task2");
        }

        [TestMethod]
        public async Task Task_in_default_exchange_folder_have_no_category_in_exchange()
        {
            // add a single task without any category
            var changeSet = new ExchangeChangeSet();
            changeSet.AddedTasks.Add(new ExchangeTask {Subject = "task", Properties = ExchangeTaskProperties.Title});
            var result1 = await this.Provider.ExchangeService.ExecuteFirstSyncAsync(this.Runner.GetConnectionInfo(), changeSet);

            Assert.IsTrue(result1.AuthorizationResult.IsOperationSuccess);

            // perform a full sync, that will create the default Exchange folder
            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, this.Manager.ActiveProvider.DefaultFolderName);
            AssertEx.ContainsTasks(this.Workbook, "task");

            // create a new task in the default Exchange folder
            this.CreateTask("task 2", this.Workbook.Folders[0]);

            // send this new task in Exchange
            await this.SyncDelta();

            this.Workbook.RemoveAll();

            // fetch all tasks from Exchange, make sure both have no categories set
            var result2 = await this.Provider.ExchangeService.ExecuteFirstSyncAsync(this.Runner.GetConnectionInfo(), new ExchangeChangeSet());

            Assert.IsTrue(result2.AuthorizationResult.IsOperationSuccess);
            Assert.AreEqual(2, result2.ChangeSet.AddedTasks.Count);
            Assert.IsTrue(string.IsNullOrEmpty(result2.ChangeSet.AddedTasks[0].Category));
            Assert.IsTrue(string.IsNullOrEmpty(result2.ChangeSet.AddedTasks[1].Category));
        }

        [TestMethod]
        public async Task Task_renamed_in_exchange()
        {
            // add a single task without any category
            var changeSet = new ExchangeChangeSet();
            changeSet.AddedTasks.Add(new ExchangeTask {Subject = "task", Properties = ExchangeTaskProperties.Title});
            var result1 = await this.Provider.ExchangeService.ExecuteFirstSyncAsync(this.Runner.GetConnectionInfo(), changeSet);

            Assert.IsTrue(result1.AuthorizationResult.IsOperationSuccess);

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            var task = this.Workbook.Tasks[0];
            Assert.IsNotNull(task.SyncId);

            // rename the task in Exchange
            changeSet = new ExchangeChangeSet();
            changeSet.ModifiedTasks.Add(new ExchangeTask {Subject = "new subject", Id = task.SyncId, Properties = ExchangeTaskProperties.Title});
            await this.Provider.ExchangeService.ExecuteFirstSyncAsync(this.Runner.GetConnectionInfo(), changeSet);

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "new subject");
        }

        [TestMethod]
        public async Task Task_skip_note_update()
        {
            await this.SyncFull();

            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);

            task.Note = "this is a note";

            await this.SyncDelta();

            // note update should not be pushed because the TaskProperties does not mention it
            task.Note = "new note";
            task.Title = "new task";
            this.Manager.Metadata.EditedTasks[task.Id] = TaskProperties.Title;

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "new task");
            AssertEx.IsWithNote(this.Workbook, 0, "this is a note");
        }

        [TestMethod]
        public async Task Task_skip_due_time()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);

            // March 10, 2015 at 02:23:30PM
            task.Due = new DateTime(2015, 3, 10, 14, 23, 30);

            await this.SyncFull();

            var newTask = AssertEx.ContainsTasks(this.Workbook, "task")[0];

            Assert.IsTrue(newTask.Due.HasValue);
            Assert.AreEqual(0, newTask.Due.Value.Hour);
            Assert.AreEqual(0, newTask.Due.Value.Minute);
            Assert.AreEqual(0, newTask.Due.Value.Second);
        }
        
        [TestMethod]
        public async Task Login_failed_wrong_credential()
        {
            var oldPassword = this.Workbook.Settings.GetValue<byte[]>(ExchangeSettings.ExchangePassword);
            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangePassword, this.CryptoService.Encrypt("wrongPassword"));

            string errorMessage = null;
            this.Provider.OperationFailed += (s, e) =>
            {
                errorMessage = e.Item;
            };

            var result = await this.Provider.CheckLoginAsync();

            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangePassword, oldPassword);

            Assert.IsFalse(result);
            Assert.IsNotNull(errorMessage);
            Assert.IsTrue(errorMessage.Contains(ExchangeAuthorizationStatus.UserCredentialsInvalid.ToReadableString()));
        }

        [TestMethod]
        public async Task Autodiscover_can_detect_server_uri()
        {
            if (TestExchangeSyncHelper.Version != ExchangeServerVersion.ExchangeOffice365)
            {
                Assert.Inconclusive("Supported only with Office 365");
            }

            // remove server uri settings
            string serverUri = this.Workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeServerUri);
            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangeServerUri, string.Empty);

            var result = await this.Provider.ExchangeService.ExecuteFirstSyncAsync(this.Runner.GetConnectionInfo(), new ExchangeChangeSet());

            Assert.AreEqual(true, result.AuthorizationResult.IsOperationSuccess);
            Assert.IsNotNull(result.AuthorizationResult.ServerUri);

            // restore original server uri settings
            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangeServerUri, serverUri);
        }

        [TestMethod]
        public async Task Missing_asmx_in_server_uri()
        {
            if (TestExchangeSyncHelper.Version != ExchangeServerVersion.ExchangeOffice365)
            {
                Assert.Inconclusive("Supported only with Office 365");
            }

            // remove server uri settings
            string originalServerUri = this.Workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeServerUri);
            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangeServerUri, originalServerUri.ToLower().Replace("ews/exchange.asmx", ""));

            var result = await this.Provider.ExchangeService.LoginAsync(this.Runner.GetConnectionInfo());

            Assert.AreEqual(ExchangeAuthorizationStatus.OK, result.AuthorizationStatus);
            Assert.IsNotNull(result.ServerUri);
            Assert.AreEqual(originalServerUri.ToLower(), result.ServerUri.ToString().ToLower());

            // restore original server uri settings
            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangeServerUri, originalServerUri);
        }

        [TestMethod]
        public async Task Bad_email_but_valid_username()
        {
            // check that we can login is the username is correct and the email is not
            var connectionInfo = this.Runner.GetConnectionInfo();
            string username = connectionInfo.Username;
            connectionInfo.Username = connectionInfo.Email;
            connectionInfo.Email = username;

            var result = await this.Provider.ExchangeService.LoginAsync(connectionInfo);

            Assert.IsTrue(result.IsOperationSuccess);
            Assert.AreEqual(ExchangeAuthorizationStatus.OK, result.AuthorizationStatus);
        }

        [TestMethod]
        public async Task Bad_username_but_valid_email()
        {
            // check that we can login is the username is correct and the email is not
            var connectionInfo = this.Runner.GetConnectionInfo();
            connectionInfo.Email = connectionInfo.Username;
            connectionInfo.Username = "invalid";

            var result = await this.Provider.ExchangeService.LoginAsync(connectionInfo);

            Assert.IsTrue(result.IsOperationSuccess);
            Assert.AreEqual(ExchangeAuthorizationStatus.OK, result.AuthorizationStatus);
        }
    }
}
#endif
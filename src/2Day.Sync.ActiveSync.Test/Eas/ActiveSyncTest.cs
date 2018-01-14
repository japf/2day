using System.Threading.Tasks;
using Chartreuse.Today.Exchange;
using Chartreuse.Today.Exchange.Providers;
using Chartreuse.Today.Sync.Test.Runners;
using Chartreuse.Today.Sync.Test.Tools;
using Chartreuse.Today.Exchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.TestCase.Eas
{
    [TestClass]
    public class ActiveSyncTest : TestCaseBase<ActiveSyncSynchronizationProvider, ActiveSyncTestRunner>
    {
        [TestMethod]
        public async Task Autodiscover_can_detect_server_uri()
        {
            // remove server uri settings
            var serverUri = this.Workbook.Settings.GetValue<string>(ExchangeSettings.ActiveSyncServerUri);
            this.Workbook.Settings.SetValue(ExchangeSettings.ActiveSyncServerUri, string.Empty);

            var result = await this.Provider.ExchangeService.ExecuteFirstSyncAsync(this.Runner.GetConnectionInfo(), new ExchangeChangeSet());

            Assert.AreEqual(true, result.AuthorizationResult.IsOperationSuccess);
            Assert.IsNotNull(result.AuthorizationResult.ServerUri);

            // restore original server uri settings
            this.Workbook.Settings.SetValue(ExchangeSettings.ActiveSyncServerUri, serverUri);
        }

        public async Task Change_folder()
        {
            var f1 = this.CreateFolder("f1");
            var task = this.CreateTask("task", f1);

            await this.SyncDelta();

            var f2 = this.CreateFolder("f2");
            task.Folder = f2;

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "f1", "f2");
            Assert.AreEqual("f2", task.Folder.Name);
        }
    }
}

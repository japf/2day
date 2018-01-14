using System.Threading.Tasks;
using Chartreuse.Today.Exchange.Providers;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.ActiveSync.Test.MultiDevices
{
    [TestClass]
    public class Office365ActiveSyncMultiDevicesTest : ActiveSyncMultiDevicesTestBase
    {
        public Office365ActiveSyncMultiDevicesTest()
            : base(TestExchangeSyncHelper.Office365AccountEmail, TestExchangeSyncHelper.Office365AccountPassword, "https://outlook.office365.com")
        {
        }

        [TestMethod]
        public async Task SyncWithTwoDevices()
        {
            // login first device
            var workbook1 = this.CreateWorkbook();
            var manager1 = this.BuildSynchronizationManager("12345678", workbook1);
            var activeSyncSyncProvider1 = (ActiveSyncSynchronizationProvider) manager1.ActiveProvider;
            await activeSyncSyncProvider1.CheckLoginAsync();

            // login second device
            var workbook2 = this.CreateWorkbook();
            var manager2 = this.BuildSynchronizationManager("12345679", workbook2);
            var activeSyncSyncProvider2 = (ActiveSyncSynchronizationProvider) manager1.ActiveProvider;
            await activeSyncSyncProvider2.CheckLoginAsync();

            // sync first device
            await manager1.Sync();

            // sync second device
            await manager2.Sync();
        }        
    }
}
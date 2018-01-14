using System;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Exchange.Providers;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.ActiveSync.Test.MultiDevices
{
    [TestClass]
    public class OutlookActiveSyncMultiDevicesTest : ActiveSyncMultiDevicesTestBase
    {
        public OutlookActiveSyncMultiDevicesTest()
            : base("app2day-test2@outlook.com", "DevWlove14", "https://outlook.office365.com")
        {
        }

        [TestMethod]
        public async Task SyncWithTwoDevices()
        {                        
            // login first device
            var workbook1 = this.CreateWorkbook();
            var manager1 = this.BuildSynchronizationManager("12345678", workbook1);
            var activeSyncSyncProvider1 = (ActiveSyncSynchronizationProvider)manager1.ActiveProvider;
            await activeSyncSyncProvider1.CheckLoginAsync();

            // login second device
            var workbook2 = this.CreateWorkbook();
            var manager2 = this.BuildSynchronizationManager("12345679", workbook2);
            var activeSyncSyncProvider2 = (ActiveSyncSynchronizationProvider)manager1.ActiveProvider;
            await activeSyncSyncProvider2.CheckLoginAsync();

            // sync first device
            await manager1.Sync();

            // delete all tasks
            foreach (var task in workbook1.Tasks.ToList())
                task.Delete();

            // sync first device
            await manager1.Sync();

            var f1d1 = workbook1.AddFolder("f1");
            var task1d1 = new Core.Shared.Model.Impl.Task { Title = "added on device 1", Added = DateTime.Now, Folder = f1d1 };

            // sync first device
            await manager1.Sync();

            // sync second device
            await manager2.Sync();

            // check on second device
            AssertEx.ContainsTasks(workbook2, "added on device 1");

            var task2d2 = new Core.Shared.Model.Impl.Task { Title = "added on device 2", Added = DateTime.Now, Folder = workbook2.Folders.FirstOrDefault(f => f.Name == "f1") };
            await manager2.Sync();

            await manager1.Sync();

            // check on first device
            AssertEx.ContainsTasks(workbook1, "added on device 1", "added on device 2");
        }
    }
}
using System.Threading.Tasks;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.TestCase.Vercors
{
    [TestClass]
    public class VercorsTest : TestCaseBase
    {
        [TestMethod]
        public async Task Folder_color()
        {
            var folder = this.CreateFolder("folder");
            folder.Color = "#da4452";

            await this.SyncFull();

            Assert.AreEqual(1, this.Workbook.Folders.Count);
            Assert.AreEqual(folder.Name, this.Workbook.Folders[0].Name);
            Assert.AreEqual(folder.Color, this.Workbook.Folders[0].Color);
        }

        [TestMethod]
        public async Task Edit_removed_task()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);

            await this.SyncDelta();

            await this.Runner.RemoteDeleteTask(task);

            await this.SyncDelta();

            Assert.AreEqual(0, folder.TaskCount);
        }        
    }
}

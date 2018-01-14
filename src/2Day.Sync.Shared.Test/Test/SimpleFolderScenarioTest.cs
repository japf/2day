using System.Threading.Tasks;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.TestCase
{
#if SYNC_TOODLEDO || SYNC_VERCORS
    [TestClass]
    public class SimpleFolderScenarioTest : TestCaseBase
    {
        [TestMethod]
        public async Task Remote_delete()
        {
            var folder = this.CreateFolder("folder");
            var task1 = this.CreateTask("title1", folder);
            var task2 = this.CreateTask("title2", folder);

            await this.SyncDelta();

            await this.Runner.RemoteDeleteFolder("folder");
            await this.Runner.RemoteDeleteTask(task1);
            await this.Runner.RemoteDeleteTask(task2);

            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook);
            AssertEx.ContainsTasks(this.Workbook);
        }
    }
#endif
}
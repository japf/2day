using System;
#if SYNC_TOODLEDO || SYNC_VERCORS
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.TestCase
{
    [TestClass]
    public class FolderTest : TestCaseBase
    {
        [TestMethod]
        public async Task Folder_rename()
        {
            if (!this.Runner.SupportFolder)
            {
                Assert.Inconclusive();
                return;
            }

            var folder = this.CreateFolder("folder");

            await this.SyncDelta();

            folder.Name = "new name";

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "new name");
        }

        [TestMethod]
        public async Task Folder_delete()
        {
            if (!this.Runner.SupportFolder)
            {
                Assert.Inconclusive();
                return;
            }

            var folder = this.CreateFolder("folder");

            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook, "folder");

            this.RemoveFolder(folder);

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook);
        }

        [TestMethod]
        public async Task Folder_duplicate()
        {
            var folder = this.CreateFolder("folder");

            await this.SyncFull();

            // remove and re-create the same folder
            // that will cause an UpdateFolder request while this folder already exists
            this.Workbook.RemoveFolder(folder.Name);
            folder = this.CreateFolder("folder");

            await this.SyncDelta();

            folder.Name = "new name";

            await this.SyncDelta();
            await this.SyncFull();

            Assert.AreEqual(1, this.Workbook.Folders.Count);
            Assert.AreEqual(folder.Name, this.Workbook.Folders[0].Name);
        }

        [TestMethod]
        public async Task Folder_scenario()
        {
            if (!this.Runner.SupportFolder)
            {
                Assert.Inconclusive();
                return;
            }

            var folder1 = this.CreateFolder("folder1");

            await this.SyncDelta();

            // check remote has folder1
            var folders = await this.Runner.RemoteGetAllFolders();
            AssertEx.ContainsFolders(folders, "folder1");

            // add folder2 in remote
            await this.Runner.RemoteAddFolder("folder2");

            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook, "folder1", "folder2");

            this.RemoveFolder(folder1);
            var folder3 = this.CreateFolder("folder3");

            // remove folder2 in remote
            await this.Runner.RemoteDeleteFolder("folder2");
            // add folder4 in remote
            await this.Runner.RemoteAddFolder("folder4");

            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook, "folder3", "folder4");

            folder3.Name = "folder3_renamed";

            await this.SyncDelta();

            // make sure folder3 has been renamed remotely
            var remoteFolders = await this.Runner.RemoteGetAllFolders();
            Assert.IsTrue(remoteFolders.Contains("folder3_renamed"));

            // for testing purpose
            // make several edits "too fast" breaks test on CI because the timestamp is unchanged
            await Task.Delay(TimeSpan.FromSeconds(10));
                
            // rename folder 4 remotely
            await this.Runner.RemoteEditFolder(this.Workbook.Folders.First(f => f.Name == "folder4").SyncId, "folder4_renamed");

            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook, "folder3_renamed", "folder4_renamed");

            this.RemoveFolder(folder3);

            await this.SyncDelta();

            // check remote has only folder4_renamed
            remoteFolders = await this.Runner.RemoteGetAllFolders();
            Assert.AreEqual(1, remoteFolders.Count);
            Assert.IsTrue(remoteFolders.Contains("folder4_renamed"));

            // delete remote folder4_renamed
            await this.Runner.RemoteDeleteFolder("folder4_renamed");

            // for testing purpose
            // make several edits "too fast" breaks test on CI because the timestamp is unchanged
            await Task.Delay(TimeSpan.FromSeconds(5));

            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook);
        }
    }
}
#endif
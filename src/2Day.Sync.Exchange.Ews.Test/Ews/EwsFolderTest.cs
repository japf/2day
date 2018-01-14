using System;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Exchange.Ews.Commands;
using Chartreuse.Today.Exchange.Ews.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Ews
{
    [TestClass]
    public class EwsFolderTest : EwsTestBase
    {
        [TestMethod]
        public async Task Scenario()
        {
            DeleteFolderResult deleteResult;

            // get all existing folders
            var getResult = await this.server.GetSubFoldersAsync(this.folderIdentifiers.TaskFolderIdentifier);
            if (getResult.Folders.Count > 0)
            {
                // delete them
                deleteResult = await this.server.DeleteFoldersAsync(getResult.Folders);
                Assert.IsTrue(deleteResult.Classes.All(c => c == EwsResponseClass.Success));
            }

            // create folders
            var createResult = await this.server.CreateFoldersAsync(new[] { "folder1", "folder2" }, this.folderIdentifiers.TaskFolderIdentifier);

            // check folder were created
            getResult = await this.server.GetSubFoldersAsync(this.folderIdentifiers.TaskFolderIdentifier);

            Assert.AreEqual(2, getResult.Folders.Count);
            Assert.IsTrue(getResult.Folders.Any(f => f.DisplayName.Equals("folder1", StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(getResult.Folders.Any(f => f.DisplayName.Equals("folder2", StringComparison.OrdinalIgnoreCase)));

            // delete created folder
            deleteResult = await this.server.DeleteFoldersAsync(getResult.Folders);
            Assert.IsTrue(deleteResult.Classes.All(c => c == EwsResponseClass.Success));
        }
    }
}

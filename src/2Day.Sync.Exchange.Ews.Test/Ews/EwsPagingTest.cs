using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Exchange.Ews.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Ews
{
    [TestClass]
    public class EwsPagingTest : EwsTestBase
    {
        [TestMethod]
        public async Task Paging_is_supported()
        {
            const int taskCount = 105;

            // delete all existing tasks
            var itemIdentifiers = await this.server.EnumerateFolderContentAsync(this.folderIdentifiers.TaskFolderIdentifier);
            await this.server.DeleteItemsAsync(itemIdentifiers);

            var tasks = new List<EwsTask>();
            for (int i = 0; i < taskCount; i++)
                tasks.Add(new EwsTask {Subject = "task " + i.ToString("D3") });

            // create 105 tasks
            var result = await this.server.CreateItemAsync(tasks, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.IsNotNull(result.Identifiers);
            Assert.AreEqual(taskCount, result.Identifiers.Count);

            // enumerate folder content
            var identifiers = await this.server.EnumerateFolderContentAsync(this.folderIdentifiers.TaskFolderIdentifier);

            Assert.IsNotNull(identifiers);
            Assert.AreEqual(taskCount, identifiers.Count);

            // download folder content
            var downloadedTasks = await this.server.DownloadFolderContentAsync(identifiers, EwsItemType.Task);

            Assert.IsNotNull(downloadedTasks);
            Assert.AreEqual(taskCount, downloadedTasks.Count);
            for (int i = 0; i < taskCount; i++)
            {
                Assert.IsTrue(downloadedTasks.Any(t => t.Subject == tasks[i].Subject));
            }
        }
    }
}

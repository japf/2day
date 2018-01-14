using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Tests;
using Chartreuse.Today.Exchange.Ews.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Ews
{
    [TestClass]
    public class EwsFlaggedItemsTest : EwsTestBase
    {
        [TestMethod]
        public async Task CreateEmail()
        {
            // act
            var result = await this.server.CreateEmailAsync(EwsKnownFolderIdentifiers.Inbox, "hello", "world", "me@2day-app.com");

            // verify
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Identifiers.Count);
            Assert.IsNotNull(result.Identifiers[0]);
            Assert.IsNotNull(result.Identifiers[0].Id);
            Assert.IsNotNull(result.Identifiers[0].ChangeKey);
        }

        [TestMethod]
        public async Task FlagEmail()
        {
            // setup
            var helper = new FlagedItemsHelper(this.server, this.syncService);
            var subject = "email " + DateTime.Now.ToString("g");
            var ewsTask = await helper.CreateFlaggedItem(subject);

            // act
            var inboxIdentifiers = await this.server.EnumerateFolderContentAsync(helper.SearchFolderIdentifier);
            var tasks = await this.server.DownloadFolderContentAsync(inboxIdentifiers, EwsItemType.Item);

            // check flagged item
            Assert.IsNotNull(tasks);
            Assert.AreEqual(1, tasks.Count);
            Assert.AreEqual(subject, tasks[0].Subject);
            Assert.AreEqual(ewsTask.DueDate.Value.Date, tasks[0].DueDate.Value.Date);
            AssertHelper.AssertArrayAreEqual(ewsTask.Categories, tasks[0].Categories);
        }

        [TestMethod]
        public async Task UnflagEmail()
        {
            // setup
            var helper = new FlagedItemsHelper(this.server, this.syncService);
            var subject = "email " + DateTime.Now.ToString("g");
            var ewsTask = await helper.CreateFlaggedItem(subject);

            // act
            ewsTask.Complete = true;
            ewsTask.CompleteDate = DateTime.Now;
            ewsTask.Changes = EwsFields.CompleteDate;
            // complete flagged item
            var updateResult = await this.server.UpdateItemAsync(ewsTask);

            Assert.IsNotNull(updateResult);
            Assert.AreEqual(1, updateResult.Identifiers.Count);
            Assert.AreEqual(ewsTask.Id, updateResult.Identifiers[0].Id);

            // no more flagged item
            var inboxIdentifiers = await this.server.EnumerateFolderContentAsync(helper.SearchFolderIdentifier);
            var tasks = await this.server.DownloadFolderContentAsync(inboxIdentifiers, EwsItemType.Item);
            Assert.IsNotNull(tasks);
            Assert.AreEqual(0, tasks.Count);
        }

        [TestMethod]
        public async Task Delete_FlagEmail()
        {
            // setup
            var helper = new FlagedItemsHelper(this.server, this.syncService);
            var subject = "email " + DateTime.Now.ToString("g");
            var ewsTask = await helper.CreateFlaggedItem(subject);

            // act
            var inboxIdentifiers = await this.server.EnumerateFolderContentAsync(helper.SearchFolderIdentifier);
            await this.server.HardDeleteItemsAsync(inboxIdentifiers);

            inboxIdentifiers = await this.server.EnumerateFolderContentAsync(helper.SearchFolderIdentifier);
            var tasks = await this.server.DownloadFolderContentAsync(inboxIdentifiers, EwsItemType.Item);

            // check flagged item has been deleted
            Assert.IsNotNull(tasks);
            Assert.AreEqual(0, tasks.Count);
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Ews
{
    [TestClass]
    public class EwsBasicTest : EwsTestBase
    {
        [TestMethod]
        public async Task Create_task()
        {
            var ewsTask = CreateSampleEwsTask();

            await this.server.CreateItemAsync(ewsTask);

            var itemIdentifiers = await this.server.EnumerateFolderContentAsync(this.folderIdentifiers.TaskFolderIdentifier);
            var tasks = await this.server.DownloadFolderContentAsync(itemIdentifiers, EwsItemType.Task);
            var task = tasks.FirstOrDefault(t => t.Subject == ewsTask.Subject);

            Assert.IsNotNull(task);
            Assert.AreEqual(ewsTask.Importance, task.Importance);
            AssertEx.ArraysAreEquals(ewsTask.Categories, task.Categories);
            Assert.AreEqual(ewsTask.BodyType, task.BodyType);
            Assert.AreEqual(ewsTask.Body, task.Body);
            Assert.AreEqual(ewsTask.Complete, task.Complete);
            Assert.AreEqual(ewsTask.ReminderIsSet, task.ReminderIsSet);
            AssertEx.DateAreEquals(ewsTask.DueDate, task.DueDate);
            AssertEx.DateAreEquals(ewsTask.ReminderDate, task.ReminderDate);
            AssertEx.DateAreEquals(ewsTask.StartDate, task.StartDate);
            AssertEx.DateAreEquals(ewsTask.OrdinalDate, task.OrdinalDate);
            AssertEx.DateAreEquals(ewsTask.CompleteDate, task.CompleteDate);
        }
       
        [TestMethod]
        public async Task Create_task_no_alarm()
        {
            var ewsTask = CreateSampleEwsTask(titleOnly: true);

            await this.server.CreateItemAsync(ewsTask);

            var itemIdentifiers = await this.server.EnumerateFolderContentAsync(this.folderIdentifiers.TaskFolderIdentifier);
            var tasks = await this.server.DownloadFolderContentAsync(itemIdentifiers, EwsItemType.Task);
            var task = tasks.FirstOrDefault(t => t.Subject == ewsTask.Subject);

            Assert.IsNotNull(task);
            Assert.AreEqual(ewsTask.ReminderIsSet, false);           
        }

        [TestMethod]
        public async Task Delete_task()
        {
            var ewsTask = CreateSampleEwsTask();

            await this.server.CreateItemAsync(ewsTask);

            var itemIdentifiers = await this.server.EnumerateFolderContentAsync(this.folderIdentifiers.TaskFolderIdentifier);
            Assert.IsNotNull(itemIdentifiers, "itemIdentifiers != null");

            var tasks = await this.server.DownloadFolderContentAsync(itemIdentifiers, EwsItemType.Task);
            Assert.IsNotNull(tasks, "tasks != null");

            var task = tasks.FirstOrDefault(t => t.Subject == ewsTask.Subject);
            Assert.IsNotNull(task, "task != null");

            await this.server.DeleteItemsAsync(new[] {itemIdentifiers.First(i => i.Id == task.Id)});

            EwsTask value = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);
            Assert.IsNull(value, "value != null");
        }

        [TestMethod]
        public async Task Create_timezone_handling()
        {
            var due = DateTime.Now.Date;
            due = due.AddHours(23).AddMinutes(59); // 11:59PM

            var ewsTask = new EwsTask
            {
                Subject = "task" + DateTime.Now.ToString("T"),
                DueDate = due
            };

            await this.server.CreateItemAsync(ewsTask);
            var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.IsTrue(task.DueDate.HasValue);
            Assert.AreEqual(ewsTask.DueDate.Value.Date, task.DueDate.Value.Date);
        }
    }
}

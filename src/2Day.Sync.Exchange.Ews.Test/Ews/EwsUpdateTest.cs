using System;
using System.Threading.Tasks;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Ews
{
    [TestClass]
    public class EwsUpdateTest : EwsTestBase
    {
        [TestMethod]
        public async Task Update_task()
        {
            var ewsTask = CreateSampleEwsTask();

            await this.server.CreateItemAsync(ewsTask);

            var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            task.Subject = "new subject";
            task.Importance = EwsImportance.Normal;
            task.BodyType = EwsBodyType.HTML;
            task.Body = "<p>new body content</p>";
            task.Categories = new string[0];
            task.Changes = EwsFields.Subject | EwsFields.Importance | EwsFields.Body | EwsFields.Categories | EwsFields.StartDate | EwsFields.DueDate | EwsFields.CompleteDate | EwsFields.Reminder;
            task.StartDate = task.StartDate.Value.AddDays(1);
            task.DueDate = task.DueDate.Value.AddDays(1);
            task.ReminderDate = task.ReminderDate.Value.AddMinutes(60);
            task.ReminderIsSet = true;
            task.CompleteDate = task.CompleteDate.Value.AddDays(1);

            await this.server.UpdateItemAsync(task);

            var newTask = await this.server.GetTask(task.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.AreEqual(task.Subject, newTask.Subject);
            Assert.AreEqual(task.Importance, newTask.Importance);
            Assert.AreEqual(task.BodyType, newTask.BodyType);
            Assert.IsTrue(newTask.Body.Contains(task.Body));
            AssertEx.DateAreEquals(task.StartDate, newTask.StartDate);
            AssertEx.DateAreEquals(task.DueDate, newTask.DueDate);
            AssertEx.DateAreEquals(task.ReminderDate, newTask.ReminderDate);
            Assert.AreEqual(task.ReminderIsSet, newTask.ReminderIsSet);
            AssertEx.DateAreEquals(task.CompleteDate.Value.Date, newTask.CompleteDate.Value.Date);
            AssertEx.ArraysAreEquals(task.Categories, newTask.Categories);
        }

        [TestMethod]
        public async Task Update_percent_complete()
        {
            var ewsTask = new EwsTask
            {
                Subject = "task" + DateTime.Now.ToString("T"),
                PercentComplete = 0.3
            };

            await this.server.CreateItemAsync(ewsTask);
            var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.AreEqual(ewsTask.PercentComplete, task.PercentComplete);

            task.PercentComplete = 0.6;
            task.Changes = EwsFields.PercentComplete;

            await this.server.UpdateItemAsync(task);

            var newTask = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.AreEqual(task.PercentComplete, newTask.PercentComplete);
        }
        
        [TestMethod]
        public async Task Update_clear_due_date()
        {
            var due = DateTime.Now.Date;

            var ewsTask = new EwsTask
            {
                Subject = "task" + DateTime.Now.ToString("T"),
                DueDate = due
            };

            await this.server.CreateItemAsync(ewsTask);
            var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            task.DueDate = null;
            task.Changes = EwsFields.DueDate;

            await this.server.UpdateItemsAsync(new[] { task });
            task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.IsNull(task.DueDate);
        }

        [TestMethod]
        public async Task Update_clear_start_date()
        {
            var start = DateTime.Now.Date;

            var ewsTask = new EwsTask
            {
                Subject = "task" + DateTime.Now.ToString("T"),
                StartDate = start
            };

            await this.server.CreateItemAsync(ewsTask);
            var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            task.StartDate = null;
            task.Changes = EwsFields.StartDate;

            await this.server.UpdateItemsAsync(new[] { task });
            task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.IsNull(task.StartDate);
        }

        [TestMethod]
        public async Task Update_clear_completed_date()
        {
            var completed = DateTime.Now.Date;

            var ewsTask = new EwsTask
            {
                Subject = "task" + DateTime.Now.ToString("T"),
                CompleteDate = completed
            };

            await this.server.CreateItemAsync(ewsTask);
            var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            task.CompleteDate = null;
            task.Changes = EwsFields.CompleteDate;

            await this.server.UpdateItemsAsync(new[] { task });
            task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.IsNull(task.CompleteDate);
        }

        [TestMethod]
        public async Task Update_clear_reminder_date()
        {
            var reminder = DateTime.Now.Date;

            var ewsTask = new EwsTask
            {
                Subject = "task" + DateTime.Now.ToString("T"),
                ReminderDate = reminder,
                ReminderIsSet = true
            };

            await this.server.CreateItemAsync(ewsTask);
            var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            task.ReminderIsSet = false;
            task.Changes = EwsFields.Reminder;

            await this.server.UpdateItemsAsync(new[] { task });
            task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.IsFalse(task.ReminderIsSet);
        }

        [TestMethod]
        public async Task Update_clear_categories()
        {
            var ewsTask = new EwsTask
            {
                Subject = "task" + DateTime.Now.ToString("T"),
                Categories = new []{ "category1", "category2"}
            };

            await this.server.CreateItemAsync(ewsTask);
            var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            task.Categories = null;
            task.Changes = EwsFields.Categories;

            await this.server.UpdateItemsAsync(new[] { task });
            task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.AreEqual(0, task.Categories.Length);
        }

        [TestMethod]
        public async Task Update_clear_body()
        {
            var ewsTask = new EwsTask
            {
                Subject = "task" + DateTime.Now.ToString("T"),
                BodyType = EwsBodyType.Text,
                Body = "test body"
            };

            await this.server.CreateItemAsync(ewsTask);
            var task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            task.Body = null;
            task.Changes = EwsFields.Body;

            await this.server.UpdateItemsAsync(new[] { task });
            task = await this.server.GetTask(ewsTask.Subject, this.folderIdentifiers.TaskFolderIdentifier);

            Assert.IsTrue(string.IsNullOrEmpty(task.Body));
        }
    }
}

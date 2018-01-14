using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.TestCase
{
    [TestClass]
    public class TaskTest : TestCaseBase
    {
        private static readonly DateTime DateSunday17Aug = new DateTime(2014, 08, 17, 0, 0, 0, DateTimeKind.Local);
        private static readonly DateTime DateSunday17AugAlarm = new DateTime(2014, 08, 17, 15, 45, 30, DateTimeKind.Local);

        [TestMethod]
        public async Task Title_special_characters_1()
        {
            const string specialChar = "title \"test\" char";
            var folder = this.CreateFolder("folder");
            this.CreateTask(specialChar, folder);

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, specialChar);
        }

        [TestMethod]
        public async Task Title_special_characters_2()
        {
            const string specialChar = "<title test char";
            var folder = this.CreateFolder("folder");
            this.CreateTask(specialChar, folder);

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, specialChar);
        }

        [TestMethod]
        public async Task Title_special_characters_3()
        {
            const string specialChar = "title test\r\nchar";
            var folder = this.CreateFolder("folder");
            this.CreateTask(specialChar, folder);

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            Assert.AreEqual(1, this.Workbook.Tasks.Count);
            bool containsExact = this.Workbook.Tasks[0].Title.Equals(specialChar);
            bool containsLineBreak = this.Workbook.Tasks[0].Title.Equals(specialChar.Replace("\r", ""));
            Assert.IsTrue(containsExact || containsLineBreak);
        }

        [TestMethod]
        public async Task Title_with_url()
        {
            const string specialChar = "https://www.google.fr";
            var folder = this.CreateFolder("folder");
            this.CreateTask(specialChar, folder);

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, specialChar);
        }

        [TestMethod]
        public async Task Date_timezone()
        {
            var folder = this.CreateFolder("folder");
            var date = DateSunday17Aug;
            var dates = new List<Tuple<string, DateTime>>();
            for (int i = 0; i < 48; i++)
            {
                string name = "task " + i.ToString("D2");
                var task = this.CreateTask(name, folder);
                task.Due = date.Date.AddHours(i);
                dates.Add(new Tuple<string, DateTime>(name, task.Due.Value));
            }

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            Assert.AreEqual(dates.Count, this.Workbook.Tasks.Count);
            var workbookDates = this.Workbook.Tasks.OrderBy(d => d.Title).ToList();
            for (int i = 0; i < dates.Count; i++)
            {
                var dateTime = dates[i];
                Assert.AreEqual(dateTime.Item1, workbookDates[i].Title);
                Assert.AreEqual(dateTime.Item2.Date, workbookDates[i].Due.Value.Date);
            }
        }

#if DEBUG
        [TestMethod]
        public async Task Date_timezone_manual_test()
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            var folder = this.CreateFolder("folder");
            var date = DateSunday17Aug;
            var dates = new List<Tuple<string, DateTime>>();
            for (int i = 0; i < 1; i++)
            {
                string name = "test";
                var task = this.CreateTask(name, folder);

                task.Due = date.Date.AddHours(i);
                task.Start = task.Due;

                // use a random title to that we're sure we're dealing with a new task each time (in case or error during save for example)
                task.Title = $"task {rand.Next(1000)} due {task.Due.Value.ToString("g")}";
                dates.Add(new Tuple<string, DateTime>(name, task.Due.Value));
            }

            await this.SyncDelta();

            // manually check in Outlook Web Access that due/start dates are correct
            // and are not 1 day behind
        }
#endif

        [TestMethod]
        public async Task Completed_update()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);

            await this.SyncDelta();

            // make sure completed task is completed sometime in the past
            task.Completed = DateTime.UtcNow.AddHours(-5);

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsCompleted(this.Workbook, 0, task.Completed.Value.Date);
        }

        [TestMethod]
        public async Task Title_update()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);

            await this.SyncDelta();

            task.Title = "new name";

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, "new name");
        }

        [TestMethod]
        public async Task Title_update_delta()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);

            await this.SyncDelta();

            task.Title = "new name";

            await this.SyncDelta();

            task.Title = "new name 2";

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, "new name 2");
        }

        [TestMethod]
        public async Task Due_update()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);
            task.Due = DateSunday17Aug;

            await this.SyncDelta();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsDue(this.Workbook, 0, task.Due);
            AssertEx.IsStarting(this.Workbook, 0, null);

            task.Due = task.Due.Value.AddDays(1.0);

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsDue(this.Workbook, 0, task.Due);
            Assert.AreEqual(DateTimeKind.Local, task.Due.Value.Kind);
            AssertEx.IsStarting(this.Workbook, 0, null);
        }

        [TestMethod]
        public async Task Start_update()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);
            task.Start = DateSunday17Aug;

            await this.SyncDelta();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsStarting(this.Workbook, 0, task.Start);

            task.Start = task.Start.Value.AddDays(1.0);

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsStarting(this.Workbook, 0, task.Start);
            Assert.AreEqual(DateTimeKind.Local, task.Start.Value.Kind);

            // with Exchange, a task cannot have a start date without a due date 
            // if we set the start date while the due date is undefined, it takes the
            // value of the due date
            if (this.Service != SynchronizationService.Exchange && this.Service != SynchronizationService.ExchangeEws)
                AssertEx.IsDue(this.Workbook, 0, null);
            else
                AssertEx.IsDue(this.Workbook, 0, task.Start);
        }

        [TestMethod]
        public async Task Start_and_due_date()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);
            task.Due = DateSunday17Aug;
            task.Start = DateSunday17Aug;

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsStarting(this.Workbook, 0, task.Start);
            AssertEx.IsDue(this.Workbook, 0, task.Due);

            // get the last instance of the task object (doing SyncFull cleared the workbook and then fetched all content from sync)
            task = this.Workbook.Tasks[0];
            task.Due = null;

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsStarting(this.Workbook, 0, task.Start);

            // with Exchange, a task cannot have a start date without a due date 
            // if we set the start date while the due date is undefined, it takes the
            // value of the due date
            if (this.Service != SynchronizationService.Exchange && this.Service != SynchronizationService.ExchangeEws)
                AssertEx.IsDue(this.Workbook, 0, null);
            else
                AssertEx.IsDue(this.Workbook, 0, task.Start);
        }

        [TestMethod]
        public async Task Alarm_update()
        {
            if (!this.Runner.SupportAlarm)
                Assert.Inconclusive();

            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);
            task.Alarm = DateSunday17AugAlarm;

            await this.SyncDelta();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsAlarming(this.Workbook, 0, task.Alarm);

            task.Alarm = task.Alarm.Value.AddHours(4.0);

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsAlarming(this.Workbook, 0, task.Alarm);
        }

        [TestMethod]
        public async Task Alarm_no_alarm()
        {
            if (!this.Runner.SupportAlarm)
                Assert.Inconclusive();

            var folder = this.CreateFolder("folder");
            this.CreateTask("task", folder);

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            var task = AssertEx.ContainsTasks(this.Workbook, "task")[0];

            Assert.IsNull(task.Alarm);
        }

        [TestMethod]
        public async Task Priority()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);

            task.Priority = TaskPriority.Low;
            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsWithPriority(this.Workbook, 0, TaskPriority.Low);
            task = this.Workbook.Tasks[0];

            task.Priority = TaskPriority.Medium;
            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsWithPriority(this.Workbook, 0, TaskPriority.Medium);
            task = this.Workbook.Tasks[0];

            task.Priority = TaskPriority.High;
            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsWithPriority(this.Workbook, 0, TaskPriority.High);
            task = this.Workbook.Tasks[0];
        }

        [TestMethod]
        public async Task Note()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);

            task.Note = "this is a note";

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsWithNote(this.Workbook, 0, "this is a note");
            task = this.Workbook.Tasks[0];

            // remove the note
            task.Note = null;

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsWithNote(this.Workbook, 0, null);
        }

        [TestMethod]
        public async Task Tags()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);

            task.Tags = "tag1";

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            task = this.Workbook.Tasks[0];
            if (this.Runner.SupportTag)
                Assert.AreEqual("tag1", task.Tags);
            else
                Assert.IsNull(task.Tags);

            task.Tags = "tag2";

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            task = this.Workbook.Tasks[0];
            if (this.Runner.SupportTag)
                Assert.AreEqual("tag2", task.Tags);
            else
                Assert.IsNull(task.Tags);
        }

        [TestMethod]
        public async Task Completed()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);

            await this.SyncDelta();

            task.IsCompleted = true;

            await this.SyncFull();

            var tasks = AssertEx.ContainsTasks(this.Workbook, "task");

            Assert.IsTrue(tasks[0].IsCompleted);
            Assert.AreEqual(DateTimeKind.Local, tasks[0].Completed.Value.Kind);
        }

        [TestMethod]
        public async Task Task_very_long_note()
        {
            const string specialChar = "title \"test\" char";
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask(specialChar, folder);

            // make sure we can overcome the "old" 2000 characters limit
            var noteBuilder = new StringBuilder();
            const int charactersLength = 2500;
            for (int i = 0; i < charactersLength; i++)
            {
                noteBuilder.Append((char) ('A' + (i%26)));
            }
            task.Note = noteBuilder.ToString();

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            Assert.AreEqual(1, this.Workbook.Tasks.Count);
            var newTask = this.Workbook.Tasks[0];
            Assert.AreEqual(2500, task.Note.Length);
            Assert.AreEqual(task.Note.Length, newTask.Note.Length);
            Assert.AreEqual(task.Note, newTask.Note);
        }

#if SYNC_TOODLEDO
        [TestMethod]
        public async Task Task_do_not_fetch_more_than_100_completed_tasks()
        {
            Assert.Inconclusive("Disabled for now at that push too many requests to ToodleDo and we hit rate limit");

            var folder = this.CreateFolder("folder");
            // 120 tasks total
            for (int i = 0; i < 120; i++)
            {
                var task = this.CreateTask("task " + i, folder);
            }

            await this.SyncDelta();

            // 110 tasks are completed
            for (int i = 0; i < 110; i++)
            {
                this.Workbook.Tasks[i].Completed = DateSunday17Aug;
            }

            await this.SyncFull();

            // we expect to have 100 completed tasks + 10 non completed tasks
            Assert.AreEqual(110, this.Workbook.Tasks.Count);
        }
#endif
    }
}

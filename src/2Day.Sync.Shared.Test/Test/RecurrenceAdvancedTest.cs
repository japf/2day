using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.TestCase
{
    [TestClass]
    public class RecurrenceAdvancedTest : TestCaseBase
    {
        private static readonly DateTime DateSunday17Aug = new DateTime(2014, 08, 17);

        [TestMethod]
        public async Task Task_complete_recurring()
        {
            var folder = this.CreateFolder("folder");

            var task = this.CreateTask("task", folder);

            var originalDue = DateSunday17Aug;
            var originalFrequency = new WeeklyFrequency();

            task.Due = originalDue;
            task.FrequencyType = FrequencyType.Weekly;
            task.CustomFrequency = originalFrequency;
            task.UseFixedDate = true;

            // add recurring task
            await this.SyncDelta();

            // complete recurring task
            task.IsCompleted = true;

            await this.SyncFull();

            Assert.AreEqual(2, this.Workbook.Tasks.Count);

            var task1 = AssertEx.ContainsTask(this.Workbook, "task", DateSunday17Aug.Day);
            AssertEx.IsDue(task1, task.Due);
            AssertEx.IsCompleted(task);
            AssertEx.IsRecurringWithFrequencyType(task, FrequencyType.Once);

            var task2 = AssertEx.ContainsTask(this.Workbook, "task", DateSunday17Aug.Day + 7);
            AssertEx.IsDue(task2, originalDue.AddDays(7));
            AssertEx.IsRecurringWithFrequency(task2, originalFrequency);
        }

        [TestMethod]
        public async Task Task_changes_note_recurring()
        {
            // reporter, Kamil Gozdek [mailto:kgozdek@hotmail.com] 
            // updating the note of a recurring task in Exchange creates a duplicate
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);
            task.Note = "task note content";

            var originalDue = DateSunday17Aug;
            var originalFrequency = new WeeklyFrequency();

            task.Due = originalDue;
            task.FrequencyType = FrequencyType.Weekly;
            task.CustomFrequency = originalFrequency;
            task.UseFixedDate = true;

            await this.SyncDelta();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsDue(this.Workbook, 0, task.Due);
            AssertEx.IsRecurringWithFrequency(task, originalFrequency);
            AssertEx.IsWithNote(this.Workbook, 0, task.Note);

            task.Note = "new note";

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook, "task");
            AssertEx.IsDue(this.Workbook, 0, task.Due);
            AssertEx.IsRecurringWithFrequency(task, originalFrequency);
            AssertEx.IsWithNote(this.Workbook, 0, task.Note);
        }

        [TestMethod]
        public async Task Task_change_due_date_of_recurring()
        {
            var folder = this.CreateFolder("folder");

            var task = this.CreateTask("task", folder);

            var originalDue = DateSunday17Aug;
            var originalFrequency = FrequencyType.Weekly;

            task.Due = originalDue;
            task.FrequencyType = originalFrequency;
            task.CustomFrequency = new WeeklyFrequency();
            task.UseFixedDate = true;

            // add recurring task
            await this.SyncDelta();

            // change due date
            DateTime newDue = task.Due.Value.AddDays(7);
            task.Due = newDue;

            await this.SyncFull();

            if (this.Service == SynchronizationService.Exchange)
            {
                // normal behavior would be to have only 1 task, but Exchange does not work this way
                // when we change the due date of a recurring task in Outlook for example, it creates a new task
                // (on top of the existing recurring task) and both have due date of the new task...
                Assert.AreEqual(2, this.Workbook.Tasks.Count);

                var task1 = AssertEx.ContainsTask(this.Workbook, "task", newDue.Day);
                AssertEx.IsDue(task1, task.Due);
                AssertEx.IsRecurringLike(task1, task);
            }
            else
            {
                Assert.AreEqual(1, this.Workbook.Tasks.Count);
            }

            var task2 = AssertEx.ContainsTask(this.Workbook, "task", newDue.Day);
            AssertEx.IsDue(task2, originalDue.AddDays(7).Date);
            AssertEx.IsRecurringLike(task2, task);
        }

        [TestMethod]
        public async Task Start_and_due_recurring()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task2", folder);
            task.Start = new DateTime(2017, 1, 23);
            task.Due = new DateTime(2017, 1, 25);
            task.FrequencyType = FrequencyType.Weekly;
            task.UseFixedDate = true;

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            Assert.AreEqual(1, this.Workbook.Tasks.Count);
            var newTask = this.Workbook.Tasks[0];
            Assert.AreEqual(task.Title, newTask.Title);
            Assert.AreEqual(task.Start, newTask.Start);
            Assert.AreEqual(task.Due, newTask.Due);
            Assert.AreEqual(task.FrequencyType, newTask.FrequencyType);
            Assert.AreEqual(task.UseFixedDate, newTask.UseFixedDate);
        }
    }
}

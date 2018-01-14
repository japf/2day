#if SYNC_TOODLEDO || SYNC_VERCORS
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Chartreuse.Today.Sync.Test.TestCase
{
    [TestClass]
    public class SubTaskTest : TestCaseBase
    {
        [TestMethod]
        public async Task Subtask_add()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);
            var subtask = this.CreateTask("subtask", folder);
            task.AddChild(subtask);

            await this.SyncFull();

            Assert.AreEqual(2, this.Workbook.Tasks.Count);
            Assert.AreEqual("task", this.Workbook.Tasks[0].Title);
            Assert.AreEqual("subtask", this.Workbook.Tasks[1].Title);
            Assert.AreEqual(1, this.Workbook.Tasks[0].Children.Count);
            Assert.AreEqual("subtask", this.Workbook.Tasks[0].Children[0].Title);
        }

        [TestMethod]
        public async Task Subtask_edit()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);
            var subtask = this.CreateTask("subtask", folder);
            task.AddChild(subtask);

            await this.SyncDelta();

            subtask.Title = "new subtask";
            subtask.IsCompleted = true;

            await this.SyncFull();

            Assert.AreEqual(2, this.Workbook.Tasks.Count);
            Assert.AreEqual("task", this.Workbook.Tasks[0].Title);
            Assert.AreEqual("new subtask", this.Workbook.Tasks[1].Title);
            Assert.AreEqual(1, this.Workbook.Tasks[0].Children.Count);
            Assert.AreEqual("new subtask", this.Workbook.Tasks[0].Children[0].Title);
            Assert.IsTrue(this.Workbook.Tasks[0].Children[0].IsCompleted);
        }

        [TestMethod]
        public async Task Subtask_change_parent()
        {
            var folder = this.CreateFolder("folder");
            var task1 = this.CreateTask("task1", folder);
            var task2 = this.CreateTask("task2", folder);
            var subtask = this.CreateTask("subtask", folder);
            task1.AddChild(subtask);

            await this.SyncDelta();

            task1.RemoveChild(subtask);
            task2.AddChild(subtask);

            await this.SyncFull();

            Assert.AreEqual(3, this.Workbook.Tasks.Count);
            var newTask1 = this.Workbook.Tasks.FirstOrDefault(t => t.Title == "task1");
            Assert.IsNotNull(newTask1);
            var newTask2 = this.Workbook.Tasks.FirstOrDefault(t => t.Title == "task2");
            Assert.IsNotNull(newTask2);
            var newSubtask = this.Workbook.Tasks.FirstOrDefault(t => t.Title == "subtask");
            Assert.IsNotNull(newSubtask);
            Assert.AreEqual(0, newTask1.Children.Count);
            Assert.AreEqual(1, newTask2.Children.Count);
            Assert.AreEqual("subtask", newTask2.Children[0].Title);            
        }

        [TestMethod]
        public async Task Subtask_delete()
        {
            var folder = this.CreateFolder("folder");
            var task = this.CreateTask("task", folder);
            var subtask = this.CreateTask("subtask", folder);
            task.AddChild(subtask);

            await this.SyncDelta();

            task.RemoveChild(subtask);
            subtask.Delete();

            await this.SyncFull();

            Assert.AreEqual(1, this.Workbook.Tasks.Count);
            Assert.AreEqual("task", this.Workbook.Tasks[0].Title);
            Assert.AreEqual(0, this.Workbook.Tasks[0].Children.Count);
        }

        [TestMethod]
        public async Task Subtask_first_sync_add_subtask_before_parent()
        {
            // that should not happen in 2Day, but run this because it indeed happened for a user
            // we first add the subtask, then the parent task
            var folder = this.CreateFolder("folder");
            var subtask = this.CreateTask("subtask", folder);
            var task = this.CreateTask("task", folder);
            task.AddChild(subtask);

            await this.SyncFull();

            Assert.AreEqual(2, this.Workbook.Tasks.Count);
            var newTask = this.Workbook.Tasks.FirstOrDefault(t => t.Title == "task");
            Assert.IsNotNull(newTask);
            var newSubTask = this.Workbook.Tasks.FirstOrDefault(t => t.Title == "subtask");
            Assert.IsNotNull(newSubTask);
            Assert.AreEqual(1, newTask.Children.Count);
            Assert.AreEqual("subtask", newTask.Children[0].Title);
        }

        [TestMethod]
        public async Task Subtask_normal_sync_add_subtask_before_parent()
        {
            await this.SyncDelta();

            // that should not happen in 2Day, but run this because it indeed happened for a user
            // we first add the subtask, then the parent task
            var folder = this.CreateFolder("folder");
            var subtask = this.CreateTask("subtask", folder);
            var task = this.CreateTask("task", folder);
            task.AddChild(subtask);

            await this.SyncFull();

            Assert.AreEqual(2, this.Workbook.Tasks.Count);
            var newTask = this.Workbook.Tasks.FirstOrDefault(t => t.Title == "task");
            Assert.IsNotNull(newTask);
            var newSubTask = this.Workbook.Tasks.FirstOrDefault(t => t.Title == "subtask");
            Assert.IsNotNull(newSubTask);
            Assert.AreEqual(1, newTask.Children.Count);
            Assert.AreEqual("subtask", newTask.Children[0].Title);
        }
    }
}
#endif
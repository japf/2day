using System;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Task = System.Threading.Tasks.Task;

namespace Chartreuse.Today.Sync.Test.TestCase
{
    [TestClass]
    public class SimpleTaskScenarioTest : TestCaseBase
    {
        private IFolder folder;

        protected override void OnTestInitialize()
        {
            this.folder = this.CreateFolder("folder");
        }

        [TestMethod]
        public async Task Local_add()
        {
            this.CreateTask("title", this.folder);

            // will cause the creation of 1 task
            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, "title");

            // must not change anything are everything is up to date
            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, "title");
        }

        [TestMethod]
        public async Task Local_edit()
        {
            var task = this.CreateTask("title", this.folder);

            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, "title");

            task.Title = "new title";

            await this.SyncFull();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, "new title");
        }

        [TestMethod]
        public async Task Local_delete()
        {
            var task = this.CreateTask("title", this.folder);

            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, "title");

            task.Delete();

            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook);
        }

        [TestMethod]
        public async Task Remote_add()
        {
            await this.SyncDelta();

            var task = this.CreateTask("title", this.folder);
            await this.Runner.RemoteAddTask(task);
            task.Delete();

            // must fetch remote task
            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, "title");
        }

        [TestMethod]
        public async Task Remote_edit()
        {
            var task = this.CreateTask("title", this.folder);

            // add task in remote service
            await this.SyncDelta();

            // perform remote edition
            await this.Runner.RemoteEditTask(new Core.Shared.Model.Impl.Task() { SyncId = task.SyncId, Title = "new title", Folder = new Folder { SyncId = this.folder.SyncId, Name = this.folder.Name } });

            // must fetch remote task
            await this.SyncDelta();

            AssertEx.ContainsFolders(this.Workbook, "folder");
            AssertEx.ContainsTasks(this.Workbook, "new title");
        }
        
        [TestMethod]
        public async Task Remote_delete()
        {
            var task = this.CreateTask("title", this.folder);

            // add task in remote service
            await this.SyncDelta();

            // perform remote deletion
            await this.Runner.RemoteDeleteTask(task);

            // must fetch remote task
            await this.SyncDelta();

            AssertEx.ContainsTasks(this.Workbook);
        }

        [TestMethod]
        public async Task Remote_delete_multiple()
        {
            var task1 = this.CreateTask("title", this.folder);
            var task2 = this.CreateTask("title", this.folder);

            // add task in remote service
            await this.SyncDelta();

            // perform remote deletion
            // put the same task twice to make sure the service can handle this
            await this.Runner.RemoteDeleteTasks(new [] { task1, task1, task2 });

            // must fetch remote task
            await this.SyncDelta();

            AssertEx.ContainsTasks(this.Workbook);
        }

        [TestMethod]
        public async Task Delete_multiple()
        {
            var task1 = this.CreateTask("title", this.folder);
            var task2 = this.CreateTask("title", this.folder);

            // add task in remote service
            await this.SyncDelta();

            task1.Delete();
            task2.Delete();

            // must fetch remote task
            await this.SyncFull();

            AssertEx.ContainsTasks(this.Workbook);
        }

        [TestMethod]
        public async Task When_a_task_change_of_folder_it_must_be_edited()
        {
            var task = this.CreateTask("title", this.folder);
            var newFolder = this.CreateFolder("new folder");

            // add task in remote service
            await this.SyncDelta();

            task.Title = "new title";
            task.Folder = newFolder;
            task.Note = "new note";

            Assert.AreEqual(1, this.Manager.Metadata.EditedTasks.Count);
            Assert.AreEqual(TaskProperties.Folder | TaskProperties.Title | TaskProperties.Note, this.Manager.Metadata.EditedTasks[task.Id]);

            Assert.AreEqual(0, this.Manager.Metadata.DeletedTasks.Count);
            Assert.AreEqual(0, this.Manager.Metadata.AddedTasks.Count);
        }

        [TestMethod]
        public async Task When_is_edited_and_removed_it_must_be_removed_only()
        {
            var task = this.CreateTask("title", this.folder);

            // add task in remote service
            await this.SyncDelta();

            task.Title = "new title";
            task.Delete();

            Assert.AreEqual(0, this.Manager.Metadata.EditedTasks.Count);
            Assert.AreEqual(1, this.Manager.Metadata.DeletedTasks.Count);
            Assert.AreEqual(0, this.Manager.Metadata.AddedTasks.Count);
        }
    }
}

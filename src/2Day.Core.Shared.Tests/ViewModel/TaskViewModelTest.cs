using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Speech;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Chartreuse.Today.Core.Shared.Tests.ViewModel
{
    [TestClass]
    public class TaskViewModelTest
    {
        private TestFactory factory;
        private Workbook workbook;

        [TestInitialize]
        public void Initialize()
        {
            this.factory = new TestFactory();
            this.workbook = this.factory.Workbook;
            this.workbook.AddFolder("f1");
        }

        private CreateTaskViewModel GetCreateTaskViewModel()
        {
            return new CreateTaskViewModel(
                this.factory.Workbook,
                new Mock<INavigationService>().Object,
                new Mock<IMessageBoxService>().Object,
                new Mock<INotificationService>().Object,
                new Mock<ISynchronizationManager>().Object,
                new Mock<ISpeechService>().Object,
                new Mock<ITrackingManager>().Object,
                new Mock<IPlatformService>().Object);
        }

        private EditTaskViewModel GetEditTaskViewModel(ITask task)
        {
            var viewModel = new EditTaskViewModel(
                this.factory.Workbook,
                new Mock<INavigationService>().Object,
                new Mock<IMessageBoxService>().Object,
                new Mock<INotificationService>().Object,
                new Mock<ITileManager>().Object,
                new Mock<ISynchronizationManager>().Object,
                new Mock<ISpeechService>().Object,
                new Mock<ITrackingManager>().Object,
                new Mock<IPlatformService>().Object);

            viewModel.LoadTask(task);

            return viewModel;
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.factory.Datacontext.CloseConnection();
        }

        [TestMethod]
        public void Add_tag()
        {
            // setup
            var viewmodel = this.GetCreateTaskViewModel();
            viewmodel.CurrentTag = "tag";

            // act
            viewmodel.AddTagCommand.Execute(null);

            // verify
            Assert.AreEqual(1, viewmodel.Tags.Count);
        }

        [TestMethod]
        public void Add_tag_empty1()
        {
            // setup
            var viewmodel = this.GetCreateTaskViewModel();

            // act
            viewmodel.AddTagCommand.Execute(null);

            // verify
            Assert.AreEqual(0, viewmodel.Tags.Count);
        }

        [TestMethod]
        public void Add_tag_empty2()
        {
            // setup
            var viewmodel = this.GetCreateTaskViewModel();
            viewmodel.CurrentTag = "";

            // act
            viewmodel.AddTagCommand.Execute(null);

            // verify
            Assert.AreEqual(0, viewmodel.Tags.Count);
        }

        [TestMethod]
        public void Add_subtask()
        {
            // setup
            var viewmodel = this.GetCreateTaskViewModel();
            viewmodel.Title = "test";
            viewmodel.SubtaskTitle = "test";
            viewmodel.AddSubtaskCommand.Execute(null);

            // act
            viewmodel.SaveCommand.Execute(null);

            // verify
            Assert.AreEqual(2, this.workbook.Tasks.Count);
        }

        [TestMethod]
        public void Edit_subtask()
        {
            // setup
            var task = new Task { Title = "task", Folder = this.workbook.Folders[0] };
            var subtask = new Task { Title = "subtask", Folder = this.workbook.Folders[0], ParentId = task.Id };
            this.workbook.Tasks[0].AddChild(subtask);

            var viewmodel = this.GetEditTaskViewModel(task);
            viewmodel.Subtasks[0].Title = "new title";
            viewmodel.Subtasks[0].IsCompleted = true;

            // act
            viewmodel.SaveCommand.Execute(null);

            // verify
            Assert.AreEqual(2, this.workbook.Tasks.Count);
            Assert.AreEqual("new title", this.workbook.Tasks[1].Title);
            Assert.IsTrue(this.workbook.Tasks[1].IsCompleted);
        }

        [TestMethod]
        public void Delete_subtask()
        {
            // setup
            var task = new Task { Title = "task", Folder = this.workbook.Folders[0] };
            var subtask = new Task { Title = "subtask", Folder = this.workbook.Folders[0], ParentId = task.Id };
            this.workbook.Tasks[0].AddChild(subtask);

            var viewmodel = this.GetEditTaskViewModel(task);
            viewmodel.DeleteSubtaskCommand.Execute(viewmodel.Subtasks[0]);

            // act
            viewmodel.SaveCommand.Execute(null);

            // verify
            Assert.AreEqual(1, this.workbook.Tasks.Count);
            Assert.AreEqual("task", this.workbook.Tasks[0].Title);
        }

        [TestMethod]
        public void When_a_task_is_completed_subtasks_are_completed()
        {
            // setup
            var task = new Task { Title = "task", Folder = this.workbook.Folders[0] };
            var subtask = new Task { Title = "subtask", Folder = this.workbook.Folders[0], ParentId = task.Id };
            this.workbook.Tasks[0].AddChild(subtask);

            var viewmodel = this.GetEditTaskViewModel(task);

            // act
            viewmodel.IsCompleted = true;

            // verify
            Assert.IsTrue(viewmodel.Subtasks[0].IsCompleted);
        }

        [TestMethod]
        public void Save_changes_to_subtask()
        {
            // setup
            var task = new Task { Title = "task", Folder = this.workbook.Folders[0] };
            var subtask = new Task { Title = "subtask", Folder = this.workbook.Folders[0] };
            this.workbook.Tasks[0].AddChild(subtask);

            var viewmodel = this.GetEditTaskViewModel(task);

            // act
            viewmodel.Subtasks[0].Title = "other";

            // verify
            Assert.AreEqual("subtask", subtask.Title);

            // act
            viewmodel.SaveCommand.Execute(null);

            // verify
            Assert.AreEqual("other", subtask.Title);
        }

        [TestMethod]
        public void IsDirty_title()
        {
            // setup
            var task = new Task { Title = "task", Folder = this.workbook.Folders[0] };
            var viewmodel = this.GetEditTaskViewModel(task);

            Assert.IsFalse(viewmodel.HasDirtyChanges());

            // act
            viewmodel.Title = "other";

            // verify
            Assert.IsTrue(viewmodel.HasDirtyChanges());
        }

        [TestMethod]
        public void IsDirty_subtask_new()
        {
            // setup
            var task = new Task { Title = "task", Folder = this.workbook.Folders[0] };
            var viewmodel = this.GetEditTaskViewModel(task);

            // act
            viewmodel.SubtaskTitle = "test";
            viewmodel.AddSubtaskCommand.Execute(null);

            // verify
            Assert.IsTrue(viewmodel.HasDirtyChanges());
        }

        [TestMethod]
        public void IsDirty_subtask_delete()
        {
            // setup
            var task = new Task { Title = "task", Folder = this.workbook.Folders[0] };
            var subtask = new Task { Title = "subtask", Folder = this.workbook.Folders[0] };
            this.workbook.Tasks[0].AddChild(subtask);

            var viewmodel = this.GetEditTaskViewModel(task);

            // act
            viewmodel.DeleteSubtaskCommand.Execute(viewmodel.Subtasks[0]);

            // verify
            Assert.IsTrue(viewmodel.HasDirtyChanges());
        }

        [TestMethod]
        public void IsDirty_subtask_edit_title()
        {
            // setup
            var task = new Task { Title = "task", Folder = this.workbook.Folders[0] };
            var subtask = new Task { Title = "subtask", Folder = this.workbook.Folders[0] };
            this.workbook.Tasks[0].AddChild(subtask);

            var viewmodel = this.GetEditTaskViewModel(task);

            // act
            viewmodel.Subtasks[0].Title = "new";

            // verify
            Assert.IsTrue(viewmodel.HasDirtyChanges());
        }

        [TestMethod]
        public void IsDirty_subtask_edit_is_completed()
        {
            // setup
            var task = new Task { Title = "task", Folder = this.workbook.Folders[0] };
            var subtask = new Task { Title = "subtask", Folder = this.workbook.Folders[0] };
            this.workbook.Tasks[0].AddChild(subtask);

            var viewmodel = this.GetEditTaskViewModel(task);

            // act
            viewmodel.Subtasks[0].IsCompleted = true;

            Assert.IsTrue(viewmodel.HasDirtyChanges());
        }
    }
}

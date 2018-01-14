using System;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.ViewModel
{
    [TestClass]
    public class FolderItemViewModelTest
    {
        private FolderItemViewModel vm;

        private IWorkbook workbook;
        private TestSettings settings;

        [TestInitialize]
        public void Initialize()
        {
            this.settings = new TestSettings();
            this.workbook = new Workbook(new TestDatabaseContext(), this.settings);

            // default settings
            this.settings.SetValue(CoreSettings.UseGroupedDates, true);
            this.settings.SetValue(CoreSettings.TaskOrderingType1, TaskOrdering.Priority);
            this.settings.SetValue(CoreSettings.TaskOrderingAscending1, false);
            this.settings.SetValue(CoreSettings.TaskOrderingType2, TaskOrdering.Folder);
            this.settings.SetValue(CoreSettings.TaskOrderingAscending2, true);
            this.settings.SetValue(CoreSettings.TaskOrderingType3, TaskOrdering.Alphabetical);
            this.settings.SetValue(CoreSettings.TaskOrderingAscending3, true);
        }

        [TestMethod]
        public void No_tasks()
        {
            // setup
            var folder = new Folder();

            // act
            this.vm = new FolderItemViewModel(this.workbook, folder);

            // verify
            Assert.AreEqual(0, this.vm.SmartCollection.Count);
        }

        [TestMethod]
        public void One_task()
        {
            // setup
            var folder = new Folder();
            var task = new Task { Title = "task1", Folder = folder };

            // act
            this.vm = new FolderItemViewModel(this.workbook, folder);

            // verify
            Assert.AreEqual(1, this.vm.SmartCollection.Count);
            Assert.AreEqual(1, this.vm.SmartCollection.Items.Count);
            Assert.AreEqual(1, this.vm.SmartCollection.Items[0].Count);
            Assert.AreEqual(task.Title, this.vm.SmartCollection.Items[0][0].Title);
        }

        [TestMethod]
        public void One_task_recurring_subtasks()
        {
            // setup
            var folder = this.workbook.AddFolder("folder");
            var task = new Task { Title = "task1", Folder = folder };
            var subtask = new Task {Title = "subtask", Folder = folder };
            task.FrequencyType = FrequencyType.Daily;
            task.AddChild(subtask);
            this.vm = new FolderItemViewModel(this.workbook, folder);

            // act
            task.IsCompleted = true;

            // verify
            Assert.AreEqual(1, this.vm.SmartCollection.Items.Count);
            Assert.AreEqual(2, this.vm.SmartCollection.Items[0].Count);
            Assert.AreEqual("task1", this.vm.SmartCollection.Items[0][0].Title);
            Assert.IsFalse(this.vm.SmartCollection.Items[0][0].IsPeriodic);
            Assert.AreEqual("task1", this.vm.SmartCollection.Items[0][1].Title);
            Assert.IsTrue(this.vm.SmartCollection.Items[0][1].IsPeriodic);
        }

        [TestMethod]
        public void Group_future()
        {
            // setup
            var folder = new Folder();
            var task1 = new Task {Title = "task1", Due = DateTime.Now.AddDays(10), Folder = folder};
            var task2 = new Task {Title = "task2", Due = DateTime.Now.AddDays(11), Folder = folder};
            var task3 = new Task {Title = "task3", Due = DateTime.Now.AddDays(12), Folder = folder};

            // act
            this.vm = new FolderItemViewModel(this.workbook, folder);

            // verify
            Assert.AreEqual(3, this.vm.SmartCollection.Count);
            Assert.AreEqual(1, this.vm.SmartCollection.Items.Count);
            Assert.AreEqual(StringResources.ConverterDate_Future, this.vm.SmartCollection.Items[0].Title);
            Assert.AreEqual(3, this.vm.SmartCollection.Items[0].Count);
            Assert.AreEqual(task1.Title, this.vm.SmartCollection.Items[0][0].Title);
            Assert.AreEqual(task2.Title, this.vm.SmartCollection.Items[0][1].Title);
            Assert.AreEqual(task3.Title, this.vm.SmartCollection.Items[0][2].Title);
        }

        [TestMethod]
        public void Group_future_different_year_alpha_order()
        {
            // setup
            var folder = new Folder();
            var task1 = new Task { Title = "task1", Due = DateTime.Now.AddDays(10), Folder = folder };
            var task2 = new Task { Title = "task2", Due = DateTime.Now.AddDays(11).AddYears(1), Folder = folder };
            var task3 = new Task { Title = "task3", Due = DateTime.Now.AddDays(12), Folder = folder };

            // act
            this.vm = new FolderItemViewModel(this.workbook, folder);

            // verify
            Assert.AreEqual(3, this.vm.SmartCollection.Count);
            Assert.AreEqual(1, this.vm.SmartCollection.Items.Count);
            Assert.AreEqual(StringResources.ConverterDate_Future, this.vm.SmartCollection.Items[0].Title);
            Assert.AreEqual(3, this.vm.SmartCollection.Items[0].Count);

            // task sort 3 setting is name, so collection is sorted back by name
            Assert.AreEqual(task1.Title, this.vm.SmartCollection.Items[0][0].Title);
            Assert.AreEqual(task2.Title, this.vm.SmartCollection.Items[0][1].Title);
            Assert.AreEqual(task3.Title, this.vm.SmartCollection.Items[0][2].Title);
        }

        [TestMethod]
        public void Group_future_different_year_due_order()
        {
            // setup
            var folder = new Folder();
            var task1 = new Task { Title = "task1", Due = DateTime.Now.AddDays(10), Folder = folder };
            var task2 = new Task { Title = "task2", Due = DateTime.Now.AddDays(11).AddYears(1), Folder = folder };
            var task3 = new Task { Title = "task3", Due = DateTime.Now.AddDays(12), Folder = folder };
            this.settings.SetValue(CoreSettings.UseGroupedDates, true);
            this.settings.SetValue(CoreSettings.TaskOrderingType1, TaskOrdering.DueDate);
            this.settings.SetValue(CoreSettings.TaskOrderingAscending1, true);
            this.settings.SetValue(CoreSettings.TaskOrderingType2, TaskOrdering.Priority);
            this.settings.SetValue(CoreSettings.TaskOrderingAscending2, false);
            this.settings.SetValue(CoreSettings.TaskOrderingType3, TaskOrdering.Alphabetical);
            this.settings.SetValue(CoreSettings.TaskOrderingAscending3, true);

            // act
            this.vm = new FolderItemViewModel(this.workbook, folder);

            // verify
            Assert.AreEqual(3, this.vm.SmartCollection.Count);
            Assert.AreEqual(1, this.vm.SmartCollection.Items.Count);
            Assert.AreEqual(StringResources.ConverterDate_Future, this.vm.SmartCollection.Items[0].Title);
            Assert.AreEqual(3, this.vm.SmartCollection.Items[0].Count);

            // task sort 3 setting is name, so collection is sorted back by name
            Assert.AreEqual(task1.Title, this.vm.SmartCollection.Items[0][0].Title);
            Assert.AreEqual(task3.Title, this.vm.SmartCollection.Items[0][1].Title);
            Assert.AreEqual(task2.Title, this.vm.SmartCollection.Items[0][2].Title);
        }
    }
}

using System;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Shared.Tests.Model
{
    [TestClass]
    public class TaskTest
    {
        [TestMethod]
        public void When_a_recurring_task_completed_subtasks_are_created()
        {
            // setup
            var parent = GetTask("parent");
            var folder = parent.Folder;
            parent.FrequencyType = FrequencyType.Daily;
            parent.Due = DateTime.Now;

            var sub1 = new Task {Title = "sub1", Folder = folder};
            parent.AddChild(sub1);

            var sub2 = new Task {Title = "sub2", Folder = folder };
            parent.AddChild(sub2);

            sub1.IsCompleted = true;

            // act
            parent.IsCompleted = true;

            // verify
            Assert.AreEqual(3 * 2, folder.Tasks.Count());
            var newParent = folder.Tasks.ElementAt(3);
            Assert.AreEqual("parent", newParent.Title);
            var newSub1 = folder.Tasks.ElementAt(4);
            var newSub2 = folder.Tasks.ElementAt(5);
            Assert.AreEqual(2, newParent.Children.Count);
            Assert.AreEqual(newSub1, newParent.Children[0]);
            Assert.AreEqual(newSub2, newParent.Children[1]);
            Assert.IsFalse(newSub1.IsCompleted);
            Assert.IsFalse(newSub2.IsCompleted);
        }

        [TestMethod]
        public void Subtask_descriptor_no_subtask()
        {
            // setup
            var task = GetTask("parent");

            // verify
            Assert.IsNull(task.ChildrenDescriptor);
        }

        [TestMethod]
        public void Subtask_change_parent()
        {
            // setup
            var folder = new Folder();
            var task1 = GetTask("parent1", folder);
            var task2 = GetTask("parent2", folder);
            var sub1 = new Task { Id = id++, Title = "sub1", Folder = task1.Folder };
            task1.AddChild(sub1);

            // act
            task2.AddChild(sub1);

            // verify
            Assert.AreEqual(0, task1.Children.Count);
            Assert.AreEqual(1, task2.Children.Count);
            Assert.AreEqual(sub1, task2.Children[0]);
        }

        [TestMethod]
        public void Subtask_descriptor_1_subtask()
        {
            // setup
            var task = GetTask("parent");
            var sub1 = new Task { Title = "sub1", Folder = task.Folder };
            task.AddChild(sub1);

            // verify
            Assert.IsNotNull(task.ChildrenDescriptor);
            Assert.AreEqual("0 / 1", task.ChildrenDescriptor);
        }

        [TestMethod]
        public void Subtask_descriptor_3_subtasks()
        {
            // setup
            var task = GetTask("parent");
            var sub1 = new Task { Title = "sub1", Folder = task.Folder };
            var sub2 = new Task { Title = "sub1", Folder = task.Folder, IsCompleted = true};
            var sub3 = new Task { Title = "sub1", Folder = task.Folder };
            task.AddChild(sub1);
            task.AddChild(sub2);
            task.AddChild(sub3);

            // verify
            Assert.IsNotNull(task.ChildrenDescriptor);
            Assert.AreEqual("1 / 3", task.ChildrenDescriptor);
        }

        [TestMethod]
        public void Subtask_descriptor_notification()
        {
            // setup
            var task = GetTask("parent");
            var sub1 = new Task { Title = "sub1", Folder = task.Folder };
            var sub2 = new Task { Title = "sub1", Folder = task.Folder, IsCompleted = true };
            var sub3 = new Task { Title = "sub1", Folder = task.Folder };
            task.AddChild(sub1);
            task.AddChild(sub2);
            task.AddChild(sub3);

            bool propertyChanged = false;
            task.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ChildrenDescriptor")
                    propertyChanged = true;
            };

            // act
            sub1.IsCompleted = true;

            // verify
            Assert.IsTrue(propertyChanged);
            Assert.IsNotNull(task.ChildrenDescriptor);
            Assert.AreEqual("2 / 3", task.ChildrenDescriptor);

            // act
            propertyChanged = false;
            var sub4 = new Task { Title = "sub1", Folder = task.Folder };
            task.AddChild(sub4);

            // verify
            Assert.IsTrue(propertyChanged);
            Assert.IsNotNull(task.ChildrenDescriptor);
            Assert.AreEqual("2 / 4", task.ChildrenDescriptor);

            // act
            propertyChanged = false;
            task.RemoveChild(sub2);

            // verify
            Assert.IsTrue(propertyChanged);
            Assert.IsNotNull(task.ChildrenDescriptor);
            Assert.AreEqual("1 / 3", task.ChildrenDescriptor);
        }

        private static int id;

        private static Task GetTask(string name)
        {
            return GetTask(name, new Folder());
        }

        private static Task GetTask(string name, IFolder folder)
        {
            var task = new Task
            {
                Id = id++,
                Title = name,
                Folder = folder,
            };
            return task;
        }
    }
}

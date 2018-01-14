using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Groups;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Chartreuse.Today.Core.Shared.Tests.Tools.Collection
{
    [TestClass]
    public class TaskComparerTest
    {
        [TestMethod]
        public void Compare_Sort_1()
        {
            var testCase = new SortTestCase(
                TaskOrdering.Priority, true, TaskOrdering.Alphabetical, true, TaskOrdering.AddedDate, false,
                "t1", "t2", "t3", "t4", "t5");

            CheckTestCase(testCase);
        }

        [TestMethod]
        public void Compare_Sort_2()
        {
            var testCase = new SortTestCase(
                TaskOrdering.Priority, true, TaskOrdering.Alphabetical, false, TaskOrdering.AddedDate, false,
                "t2", "t1", "t3", "t5", "t4");

            CheckTestCase(testCase);
        }

        [TestMethod]
        public void Compare_Sort_3()
        {
            var testCase = new SortTestCase(
                TaskOrdering.Priority, false, TaskOrdering.Alphabetical, true, TaskOrdering.AddedDate, false,
                "t4", "t5", "t3", "t1", "t2");

            CheckTestCase(testCase);
        }

        [TestMethod]
        public void Compare_Sort_4()
        {
            var testCase = new SortTestCase(
                TaskOrdering.Priority, false, TaskOrdering.Alphabetical, false, TaskOrdering.AddedDate, false,
                "t5", "t4", "t3", "t2", "t1");

            CheckTestCase(testCase);
        }

        [TestMethod]
        public void Compare_Sort_5()
        {
            var testCase = new SortTestCase(
                TaskOrdering.AddedDate, true, TaskOrdering.Alphabetical, false, TaskOrdering.Priority, false,
                "t1", "t2", "t3", "t4", "t5");

            CheckTestCase(testCase);
        }

        [TestMethod]
        public void Compare_Sort_6()
        {
            var testCase = new SortTestCase(
                TaskOrdering.AddedDate, false, TaskOrdering.Alphabetical, false, TaskOrdering.Priority, false,
                "t5", "t4", "t3", "t2", "t1");

            CheckTestCase(testCase);
        }

        [TestMethod]
        public void Compare_Sort_7()
        {
            var testCase = new SortTestCase(
                TaskOrdering.Priority, true, TaskOrdering.AddedDate, false, TaskOrdering.ModifiedDate, false,
                "t2", "t1", "t3", "t5", "t4");

            CheckTestCase(testCase);
        }

        [TestMethod]
        public void Compare_Sort_8()
        {
            var testCase = new SortTestCase(
                TaskOrdering.Priority, true, TaskOrdering.AddedDate, true, TaskOrdering.ModifiedDate, false,
                "t1", "t2", "t3", "t4", "t5");

            CheckTestCase(testCase);
        }

        [TestMethod]
        public void Compare_Completed_DoesNotChangeOrder()
        {
            var folder = new Folder();
            var settings = CreateSettings();

            var t1 = new Task() { Title = "t1", Folder = folder, Added = new DateTime(2011, 1, 1, 9, 0, 10) };
            var t2 = new Task() { Title = "t2", Folder = folder, Added = new DateTime(2011, 1, 1, 9, 0, 20) };
            var t3 = new Task() { Title = "t3", Folder = folder, Added = new DateTime(2011, 1, 1, 9, 0, 30) };

            var c = new TaskComparer(settings);
            var l = new List<ITask> { t1, t2, t3 };

            l.Sort(c);

            l.AssertOrderIs(t3, t2, t1);

            Assert.AreEqual(t3, l[0]);
            Assert.AreEqual(t2, l[1]);
            Assert.AreEqual(t1, l[2]);

            l.Sort(c);

            l.AssertOrderIs(t3, t2, t1);

            Assert.AreEqual(t3, l[0]);
            Assert.AreEqual(t2, l[1]);
            Assert.AreEqual(t1, l[2]);

            t1.IsCompleted = true;
            l.Sort(c);

            l.AssertOrderIs(t3, t2, t1);

            Assert.AreEqual(t3, l[0]);
            Assert.AreEqual(t2, l[1]);
            Assert.AreEqual(t1, l[2]);

            t1.IsCompleted = false;
            l.Sort(c);

            l.AssertOrderIs(t3, t2, t1);

            Assert.AreEqual(t3, l[0]);
            Assert.AreEqual(t2, l[1]);
            Assert.AreEqual(t1, l[2]);

            t3.IsCompleted = true;
            t2.IsCompleted = true;
            l.Sort(c);

            l.AssertOrderIs(t3, t2, t1);

            Assert.AreEqual(t3, l[0]);
            Assert.AreEqual(t2, l[1]);
            Assert.AreEqual(t1, l[2]);
        }

        private static void CheckTestCase(SortTestCase testCase)
        {
            var folder = new Folder();
            var settings = CreateSettings(testCase);

            var t1 = new Task() { Title = "t1", Priority = TaskPriority.Low, Folder = folder, Added = new DateTime(2011, 1, 1, 9, 0, 10) };
            var t2 = new Task() { Title = "t2", Priority = TaskPriority.Low, Folder = folder, Added = new DateTime(2011, 1, 1, 9, 0, 20) };
            var t3 = new Task() { Title = "t3", Priority = TaskPriority.Medium, Folder = folder, Added = new DateTime(2011, 1, 1, 9, 0, 30) };
            var t4 = new Task () { Title = "t4", Priority = TaskPriority.High, Folder = folder, Added = new DateTime(2011, 1, 1, 9, 0, 40) };
            var t5 = new Task() { Title = "t5", Priority = TaskPriority.High, Folder = folder, Added = new DateTime(2011, 1, 1, 9, 0, 50) };

            var tasks = new List<ITask> { t1, t2, t3, t4, t5 };
            var comparer = new TaskComparer(settings);

            tasks.Sort(comparer);
            tasks.AssertOrderIs(testCase.Titles);

            // sort 2 times to make sure sort is stable
            tasks.Sort(comparer);
            tasks.AssertOrderIs(testCase.Titles);
        }

        private static ISettings CreateSettings(SortTestCase sortTestCase)
        {
            var settings = new Mock<ISettings>();

            settings.Setup(s => s.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType1)).Returns(sortTestCase.Ordering1);
            settings.Setup(s => s.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType2)).Returns(sortTestCase.Ordering2);
            settings.Setup(s => s.GetValue<TaskOrdering>(CoreSettings.TaskOrderingType3)).Returns(sortTestCase.Ordering3);
            settings.Setup(s => s.GetValue<bool>(CoreSettings.TaskOrderingAscending1)).Returns(sortTestCase.Ascending1);
            settings.Setup(s => s.GetValue<bool>(CoreSettings.TaskOrderingAscending2)).Returns(sortTestCase.Ascending2);
            settings.Setup(s => s.GetValue<bool>(CoreSettings.TaskOrderingAscending3)).Returns(sortTestCase.Ascending3);

            return settings.Object;
        }

        private static ISettings CreateSettings()
        {
            return CreateSettings(new SortTestCase(TaskOrdering.Priority, false, TaskOrdering.AddedDate, false, TaskOrdering.Alphabetical, true));
        }

        public struct SortTestCase
        {
            public string[] Titles { get; set; }

            public TaskOrdering Ordering1 { get; set; }
            public TaskOrdering Ordering2 { get; set; }
            public TaskOrdering Ordering3 { get; set; }
            public bool Ascending1 { get; set; }
            public bool Ascending2 { get; set; }
            public bool Ascending3 { get; set; }

            public SortTestCase(TaskOrdering ordering1, bool ascending1, TaskOrdering ordering2, bool ascending2, TaskOrdering ordering3, bool ascending3, params string[] titles)
                : this()
            {
                this.Titles = titles;

                this.Ordering1 = ordering1;
                this.Ordering2 = ordering2;
                this.Ordering3 = ordering3;

                this.Ascending1 = ascending1;
                this.Ascending2 = ascending2;
                this.Ascending3 = ascending3;
            }
        }
    }
}

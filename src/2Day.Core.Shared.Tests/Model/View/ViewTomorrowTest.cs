using System;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Model.View;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Model.View
{
    [TestClass]
    public class ViewTomorrowTest : ViewTestBase
    {
        public ViewTomorrowTest()
            : base(ViewKind.Tomorrow)
        {
        }

        [TestMethod]
        public void Due_yesterday() {
            var task = new Task { Folder = this.Folder, Due = DateTime.Now.AddDays(-1) };

            Assert.AreEqual(0, this.View.TaskCount);
        }

        [TestMethod]
        public void Due_today()
        {
            var task = new Task {Folder = this.Folder, Due = DateTime.Now };

            Assert.AreEqual(0, this.View.TaskCount);
        }

        [TestMethod]
        public void Due_tomorrow()
        {
            var task = new Task { Folder = this.Folder, Due = DateTime.Now.AddDays(1) };

            Assert.AreEqual(1, this.View.TaskCount);
        }

        [TestMethod]
        public void Due_later()
        {
            var task = new Task { Folder = this.Folder, Due = DateTime.Now.AddDays(2) };

            Assert.AreEqual(0, this.View.TaskCount);
        }

        [TestMethod]
        public void Special_feb_28()
        {
            var feb26 = new DateTime(2015, 2, 28);
            ViewTomorrow.Now = feb26;

            var task = new Task { Folder = this.Folder, Due = feb26 };

            Assert.AreEqual(0, this.View.TaskCount);

            // make the task recurring and complete it to create a new task
            task.FrequencyType = FrequencyType.Daily;
            task.UseFixedDate = true;
            task.IsCompleted = true;

            Assert.AreEqual(2, this.Workbook.Tasks.Count);
            Assert.AreEqual(1, this.View.TaskCount);
        }
    }
}
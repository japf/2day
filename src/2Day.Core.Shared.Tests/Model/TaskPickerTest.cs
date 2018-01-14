using System;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Model
{
    [TestClass]
    public class TaskPickerTest
    {
        private readonly IWorkbook workbook;
        private readonly IFolder folder;

        public TaskPickerTest()
        {
            this.workbook = new Workbook(new TestDatabaseContext(), new TestSettings());
            this.folder = this.workbook.AddFolder("f1");
        }

        [TestMethod]
        public void Start_date_tomorrow()
        {
            var task = new Task { Folder = this.folder, Start = DateTime.Now.AddDays(1) };

            var tasks = TaskPicker.SelectTasks(this.folder, new TestSettings());

            Assert.AreEqual(0, tasks.Count);
        }

        [TestMethod]
        public void Start_date_past()
        {
            var task = new Task { Folder = this.folder, Start = DateTime.Now.AddDays(-1) };

            var tasks = TaskPicker.SelectTasks(this.folder, new TestSettings());

            Assert.AreEqual(1, tasks.Count);
        }

        [TestMethod]
        public void Start_date_earlier_today()
        {
            var task = new Task { Folder = this.folder, Start = DateTime.Now.AddHours(-1) };

            var tasks = TaskPicker.SelectTasks(this.folder, new TestSettings());

            Assert.AreEqual(1, tasks.Count);
        }

        [TestMethod]
        public void Start_date_later_today()
        {
            var task = new Task { Folder = this.folder, Start = DateTime.Now.AddHours(1) };

            var tasks = TaskPicker.SelectTasks(this.folder, new TestSettings());

            Assert.AreEqual(0, tasks.Count);
        }
    }
}

using System;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Model.View
{
    [TestClass]
    public class ViewStartDateTest : ViewTestBase
    {
        public ViewStartDateTest() 
            : base(ViewKind.StartDate)
        {
        }

        [TestMethod]
        public void Start_date_tomorrow()
        {
            var task = new Task { Folder = this.Folder, Start = DateTime.Now.AddDays(1) };

            Assert.AreEqual(1, this.View.TaskCount);
        }

        [TestMethod]
        public void Start_date_past()
        {
            var task = new Task { Folder = this.Folder, Start = DateTime.Now.AddDays(-1) };

            Assert.AreEqual(0, this.View.TaskCount);
        }

        [TestMethod]
        public void Start_date_earlier_today()
        {
            var task = new Task { Folder = this.Folder, Start = DateTime.Now.AddHours(-1) };

            Assert.AreEqual(0, this.View.TaskCount);
        }

        [TestMethod]
        public void Start_date_later_today()
        {
            var task = new Task { Folder = this.Folder, Start = DateTime.Now.AddHours(1) };

            Assert.AreEqual(1, this.View.TaskCount);
        }
    }
}

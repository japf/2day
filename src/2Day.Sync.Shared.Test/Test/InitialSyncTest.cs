using System;
using System.Threading.Tasks;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.TestCase
{
    [TestClass]
    public class InitialSyncTest : TestCaseBase
    {
        [TestMethod]
        public async Task Initial_sync_should_not_create_duplicates_no_local_changes()
        {
            // setup
            var folder = this.CreateFolder("folder1");
            var folder2 = this.CreateFolder("folder2");
            
            var task1 = this.CreateTask("title1", folder);
            task1.Due = DateTime.Now.Date;

            var task2 = this.CreateTask("title2", folder);

            var task3 = this.CreateTask("title1", folder);
            task3.Due = DateTime.Now.Date.AddDays(1);

            var task4 = this.CreateTask("title1", folder);
            var task5 = this.CreateTask("title1", folder);

            // act
            await this.SyncDelta();

            var currentService = this.Manager.ActiveService;
            this.Manager.Reset(clearSettings: false);
            this.Manager.ActiveService = currentService;

            await this.SyncDelta();

            // check
            AssertEx.ContainsFolders(this.Workbook, "folder1", "folder2");
            var tasks = AssertEx.ContainsTasks(this.Workbook, "title1", "title2", "title1", "title1", "title1");
            AssertEx.CheckSyncId(this.Workbook);
        }

        [TestMethod]
        public async Task Initial_sync_should_not_create_duplicates_with_local_changes()
        {
            // setup
            var folder = this.CreateFolder("folder1");

            var task1 = this.CreateTask("title1", folder);
            var task2 = this.CreateTask("title1", folder);

            // act
            await this.SyncDelta();

            var currentService = this.Manager.ActiveService;
            this.Manager.Reset(clearSettings: false);
            this.Manager.ActiveService = currentService;

            var task3 = this.CreateTask("title1", folder);
            var task4 = this.CreateTask("title2", folder);

            await this.SyncDelta();

            // check
            AssertEx.ContainsFolders(this.Workbook, "folder1");
            var tasks = AssertEx.ContainsTasks(this.Workbook, "title1", "title1", "title1", "title2");
            AssertEx.CheckSyncId(this.Workbook);
        }
    }
}

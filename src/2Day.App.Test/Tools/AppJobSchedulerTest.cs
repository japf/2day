using System;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Test.Impl;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Universal.IO;
using Chartreuse.Today.Core.Universal.Model;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Chartreuse.Today.App.Test.Tools
{
    [TestClass]
    public class AppJobSchedulerTest
    {
        private Workbook workbook;
        private IFolder folder;
        private FolderItemViewModel folderItemViewModel;
        private AppJobScheduler scheduler;
        private TestSynchronizationManager syncManager;

        [TestInitialize]
        public void Initialize()
        {
            var context = new WinDatabaseContext(string.Format("db-{0}.out", Guid.NewGuid()), false);
            context.InitializeDatabase();

            this.workbook = new Workbook(context, WinSettings.Instance);
            this.workbook.Settings.SetValue(CoreSettings.ShowFutureStartDates, false);

            this.folder = this.workbook.AddFolder("f1");
            StaticTestOverrides.Now = DateTime.Now;
        }

        [TestMethod]
        public void When_timer_ticks_collection_is_rebuilt()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                // setup
                this.Setup(null);
                Assert.IsTrue(this.folderItemViewModel.SmartCollection.Items[0].Title.StartsWith("Today"));

                // act
                StaticTestOverrides.Now = DateTime.Now.AddDays(1);
                this.scheduler.OnUpdateTasksTimerTick();

                // verify
                Assert.IsFalse(this.folderItemViewModel.SmartCollection.Items[0].Title.StartsWith("Today"));
            });
        }

        [TestMethod]
        public void When_timer_ticks_start_date_is_checked()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                // setup
                this.Setup(DateTime.Now.AddDays(1));
                Assert.AreEqual(0, this.folderItemViewModel.SmartCollection.Count);

                // act
                StaticTestOverrides.Now = DateTime.Now.AddDays(1);
                this.scheduler.OnUpdateTasksTimerTick();

                // verify
                Assert.AreEqual(1, this.folderItemViewModel.SmartCollection.Count);
            });
        }

        [TestMethod]
        public void When_timer_ticks_sync_running_no_rebuilt()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                // setup
                this.Setup(null);
                this.syncManager.IsSyncRunning = true;
                Assert.IsTrue(this.folderItemViewModel.SmartCollection.Items[0].Title.StartsWith("Today"));

                // act
                StaticTestOverrides.Now = DateTime.Now.AddDays(1);
                this.scheduler.OnUpdateTasksTimerTick();

                // verify
                Assert.IsTrue(this.folderItemViewModel.SmartCollection.Items[0].Title.StartsWith("Today"));
            });
        }

        [TestMethod]
        public void When_timer_ticks_sync_running_no_start_date()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                // setup
                this.Setup(DateTime.Now.AddDays(1));
                this.syncManager.IsSyncRunning = true;
                Assert.AreEqual(0, this.folderItemViewModel.SmartCollection.Count);

                // act
                StaticTestOverrides.Now = DateTime.Now.AddDays(1);
                this.scheduler.OnUpdateTasksTimerTick();

                // verify
                Assert.AreEqual(0, this.folderItemViewModel.SmartCollection.Count);
            });
        }

        public void Setup(DateTime? startDate)
        {
            var task = new Core.Shared.Model.Impl.Task { Folder = this.folder, Due = DateTime.Now, Start = startDate };

            this.folderItemViewModel = new FolderItemViewModel(this.workbook, this.folder);

            this.syncManager = new TestSynchronizationManager();
            this.scheduler = new AppJobScheduler(this.workbook, this.syncManager, () => new[] { this.folderItemViewModel });

            this.scheduler.OnUpdateTasksTimerTick();
        }
    }
}

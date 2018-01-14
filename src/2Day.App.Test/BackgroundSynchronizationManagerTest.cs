using System;
using System.Collections.Generic;
using System.Diagnostics;
using Chartreuse.Today.App.Background;
using Chartreuse.Today.App.Test.Impl;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Universal.IO;
using Chartreuse.Today.Core.Universal.Model;
using Chartreuse.Today.Core.Universal.Tools;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Task = System.Threading.Tasks.Task;

namespace Chartreuse.Today.App.Test
{
    [TestClass]
    public class BackgroundSynchronizationManagerTest
    {
        private IWorkbook workbook;
        private IWorkbook backgroundWorkbook;

        private IPersistenceLayer persistence;
        private IPersistenceLayer backgroundPersistence;
        private BackgroundSynchronizationManager manager;

        [TestInitialize]
        public async Task BeforeTest()
        {
            await WinIsolatedStorage.DeleteAsync("2day.db");

            CreateWorkbook(out this.persistence, out this.workbook);
            this.workbook.RemoveAll();
            this.persistence.Save();

            this.workbook.AddFolder("f1");
            this.workbook.AddContext("c1");
            this.persistence.Save();

            CreateWorkbook(out this.backgroundPersistence, out this.backgroundWorkbook);
            if (Ioc.HasType<ISynchronizationManager>())
                Ioc.RemoveInstance<ISynchronizationManager>();

            var trackingManager = new TestTrackingManager();
            var syncManager = new SynchronizationManager(new TestPlatformService(), trackingManager, "test", false);
            Ioc.RegisterInstance<ISynchronizationManager, SynchronizationManager>(syncManager);

            this.manager = new BackgroundSynchronizationManager(this.workbook, trackingManager, (s) => { });
        }

        [TestCleanup]
        public void AfterTest()
        {
            this.persistence.CloseDatabase();
            this.backgroundPersistence.CloseDatabase();
        }

        [TestMethod]
        public async Task Folder_added_in_background()
        {
            // setup
            var oldFolder = this.workbook.Folders[0];
            var newFolder = this.backgroundWorkbook.AddFolder("f2");
            this.backgroundPersistence.Save();
            await SaveMetadataAsync(new SynchronizationMetadata { AfterSyncEditedFolders = new List<string> { newFolder.Name } });

            // act
            await this.manager.TryUpdateWorkbookAsync();

            // check
            Assert.AreEqual(oldFolder, this.workbook.Folders[0]);
            this.Check(() => AssertEx.ContainsFolders(this.workbook, "f1", "f2"));
        }

        [TestMethod]
        public async Task Folder_edited_in_background()
        {
            // setup
            this.backgroundWorkbook.Folders[0].Color = "FFEEDD";
            this.backgroundWorkbook.Folders[0].Order = 1;
            this.backgroundWorkbook.Folders[0].IconId = 2;
            this.backgroundPersistence.Save();
            await SaveMetadataAsync(new SynchronizationMetadata { AfterSyncEditedFolders = new List<string> { "f1" } });

            // act
            await this.manager.TryUpdateWorkbookAsync();

            // check
            Assert.AreEqual("FFEEDD", this.workbook.Folders[0].Color);
            Assert.AreEqual(1, this.workbook.Folders[0].Order);
            Assert.AreEqual(2, this.workbook.Folders[0].IconId);
        }

        [TestMethod]
        public async Task Folder_renamed_in_background()
        {
            // setup
            this.backgroundWorkbook.Folders[0].Name = "new name";
            this.backgroundPersistence.Save();
            await SaveMetadataAsync(new SynchronizationMetadata { AfterSyncEditedFolders = new List<string> { "new name" } });

            // act
            await this.manager.TryUpdateWorkbookAsync();

            // check
            Assert.AreEqual("new name", this.workbook.Folders[0].Name);
        }

        [TestMethod]
        public async Task Folder_removed_in_background()
        {
            // setup
            this.backgroundWorkbook.RemoveFolder("f1");
            this.backgroundPersistence.Save();
            await SaveMetadataAsync(new SynchronizationMetadata { AfterSyncEditedFolders = new List<string> { "f1" } });

            // act
            await this.manager.TryUpdateWorkbookAsync();

            // check
            this.Check(() => AssertEx.ContainsFolders(this.workbook));
        }

        [TestMethod]
        public async Task Context_added_in_background()
        {
            // setup
            var oldContext = this.workbook.Contexts[0];
            var newContext = this.backgroundWorkbook.AddContext("c2");
            this.backgroundPersistence.Save();
            await SaveMetadataAsync(new SynchronizationMetadata { AfterSyncEditedContexts = new List<string> { newContext.Name } });

            // act
            await this.manager.TryUpdateWorkbookAsync();

            // check
            Assert.AreEqual(oldContext, this.workbook.Contexts[0]);
            this.Check(() => AssertEx.ContainsContexts(this.workbook, "c1", "c2"));
        }

        [TestMethod]
        public async Task Context_edited_in_background()
        {
            // setup
            this.backgroundWorkbook.Contexts[0].Order = 1;
            this.backgroundPersistence.Save();
            await SaveMetadataAsync(new SynchronizationMetadata { AfterSyncEditedContexts = new List<string> { "c1" } });

            // act
            await this.manager.TryUpdateWorkbookAsync();

            // check
            Assert.AreEqual(1, this.workbook.Contexts[0].Order);
        }

        [TestMethod]
        public async Task Context_renamed_in_background()
        {
            // setup
            this.backgroundWorkbook.Contexts[0].Name = "new name";
            this.backgroundPersistence.Save();
            await SaveMetadataAsync(new SynchronizationMetadata { AfterSyncEditedContexts = new List<string> { "new name" } });

            // act
            await this.manager.TryUpdateWorkbookAsync();

            // check
            Assert.AreEqual("new name", this.workbook.Contexts[0].Name);
        }

        [TestMethod]
        public async Task Context_removed_in_background()
        {
            // setup
            this.backgroundWorkbook.RemoveContext("c1");
            this.backgroundPersistence.Save();
            await SaveMetadataAsync(new SynchronizationMetadata { AfterSyncEditedContexts = new List<string> { "c1" } });

            // act
            await this.manager.TryUpdateWorkbookAsync();

            // check
            this.Check(() => AssertEx.ContainsContexts(this.workbook));
        }

        [TestMethod]
        public async Task Task_added_in_background()
        {
            // setup
            var task = new Core.Shared.Model.Impl.Task() { Added = DateTime.Now, Title = "t1", Folder = this.backgroundWorkbook.Folders[0] };
            this.backgroundPersistence.Save();
            await SaveMetadataAsync(new SynchronizationMetadata { AfterSyncEditedTasks = new List<int> { task.Id } });

            // act
            await this.manager.TryUpdateWorkbookAsync();

            // check
            this.Check(() => AssertEx.ContainsTasks(this.workbook, "t1"));
        }

        [TestMethod]
        public async Task Task_edited_in_background()
        {
            // setup
            ITask task = new Core.Shared.Model.Impl.Task() { Added = DateTime.Now, Title = "t1", Folder = this.workbook.Folders[0] };
            this.persistence.Save();
            CreateWorkbook(out this.backgroundPersistence, out this.backgroundWorkbook);
            task = this.backgroundWorkbook.Tasks[0];

            task.Title = "renamed";
            task.Priority = TaskPriority.Medium;
            task.Action = TaskAction.Call;
            task.ActionName = "call";
            task.ActionValue = "dad";
            task.Alarm = DateTime.Now;
            task.Completed = DateTime.Now;
            task.CustomFrequency = new EveryXPeriodFrequency { Scale = CustomFrequencyScale.Month, Rate = 2 };
            task.Due = DateTime.Now;
            task.Note = "note";
            task.UseFixedDate = true;
            task.Tags = "tag";
            task.Start = DateTime.Now;
            task.Progress = 0.5;
            task.SyncId = "2";

            this.backgroundPersistence.Save();

            await SaveMetadataAsync(new SynchronizationMetadata { AfterSyncEditedTasks = new List<int> { task.Id } });

            // act
            await this.manager.TryUpdateWorkbookAsync();

            // check
            var newTask = this.workbook.Tasks[0];
            Assert.AreEqual(task.Title, newTask.Title);
            Assert.AreEqual(task.Priority, newTask.Priority);
            Assert.AreEqual(task.Action, newTask.Action);
            Assert.AreEqual(task.ActionName, newTask.ActionName);
            Assert.AreEqual(task.ActionValue, newTask.ActionValue);

            Assert.IsTrue(Math.Abs((task.Alarm.Value - newTask.Alarm.Value).TotalSeconds) < 1);
            Assert.IsTrue(Math.Abs((task.Completed.Value - newTask.Completed.Value).TotalSeconds) < 1);
            Assert.IsTrue(Math.Abs((task.Due.Value - newTask.Due.Value).TotalSeconds) < 1);
            Assert.IsTrue(Math.Abs((task.Start.Value - newTask.Start.Value).TotalSeconds) < 1);

            Assert.IsTrue(task.CustomFrequency.Equals(newTask.CustomFrequency));
            Assert.AreEqual(task.Note, newTask.Note);
            Assert.AreEqual(task.UseFixedDate, newTask.UseFixedDate);
            Assert.AreEqual(task.Tags, newTask.Tags);
            Assert.AreEqual(task.Progress, newTask.Progress);
            Assert.AreEqual(task.SyncId, newTask.SyncId);
        }

        [TestMethod]
        public async Task Task_and_folder_added_in_background()
        {
            // setup
            var oldFolder = this.workbook.Folders[0];
            var newFolder = this.backgroundWorkbook.AddFolder("f2");
            var task = new Core.Shared.Model.Impl.Task() { Added = DateTime.Now, Title = "t1", Folder = this.backgroundWorkbook.Folders[1] };
            this.backgroundPersistence.Save();

            await SaveMetadataAsync(new SynchronizationMetadata
            {
                AfterSyncEditedFolders = new List<string> { newFolder.Name },
                AfterSyncEditedTasks = new List<int> { task.Id }
            });

            // act
            await this.manager.TryUpdateWorkbookAsync();

            // check
            Assert.AreEqual(oldFolder, this.workbook.Folders[0]);

            AssertEx.ContainsFolders(this.workbook, "f1", "f2");
            AssertEx.ContainsTasks(this.workbook, "t1");

            this.persistence.Save();
            this.persistence.CloseDatabase();
            CreateWorkbook(out this.persistence, out this.workbook);

            AssertEx.ContainsFolders(this.workbook, "f1", "f2");
            AssertEx.ContainsTasks(this.workbook, "t1");
        }

        private void Check(Action assertion)
        {
            assertion();
            this.persistence.Save();
            this.persistence.CloseDatabase();
            CreateWorkbook(out this.persistence, out this.workbook);
            assertion();

            this.persistence.CloseDatabase();
        }

        private static async Task SaveMetadataAsync(SynchronizationMetadata metadata)
        {
            await WinIsolatedStorage.SaveAsync(metadata, SynchronizationMetadata.Filename);
        }

        private static void CreateWorkbook(out IPersistenceLayer persistence, out IWorkbook workbook)
        {
            persistence = new WinPersistenceLayer(false);
            IWorkbook currentWorkbook = null;
            if (persistence.HasSave)
            {
                try
                {
                    currentWorkbook = persistence.Open() as Workbook;
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.ToString());
                }
            }

            if (currentWorkbook == null)
            {
                persistence.Initialize();
                persistence.Save();
                currentWorkbook = new Workbook(persistence.Context, WinSettings.Instance);
            }

            workbook = currentWorkbook;
            workbook.Initialize();
        }
    }
}

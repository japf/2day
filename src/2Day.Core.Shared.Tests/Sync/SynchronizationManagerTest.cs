using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tests.Impl;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Chartreuse.Today.Core.Shared.Tests.Sync
{
    [TestClass]
    public class SynchronizationManagerTest
    {
        private SynchronizationManager manager;

        [TestInitialize]
        public void Initialize()
        {
            // setup

            this.manager = new SynchronizationManager(new TestPlatformService(), new Mock<ITrackingManager>().Object, "test", false);
            this.manager.InitializeAsync().Wait();
            this.manager.Metadata.Reset();
        }

        [TestMethod]
        public async Task Metadata_save()
        {
            // act
            this.manager.Metadata.Backups.Add("backup");

            this.manager.Metadata.AddedFolders.Add(10);
            this.manager.Metadata.DeletedFolders.Add(new DeletedEntry(11, "12"));
            this.manager.Metadata.EditedFolders.Add(13);
            this.manager.Metadata.AfterSyncEditedFolders.Add("14");

            this.manager.Metadata.AddedContexts.Add(20);
            this.manager.Metadata.DeletedContexts.Add(new DeletedEntry(21, "22"));
            this.manager.Metadata.EditedContexts.Add(23);
            this.manager.Metadata.AfterSyncEditedContexts.Add("24");

            this.manager.Metadata.AddedTasks.Add(30);
            this.manager.Metadata.DeletedTasks.Add(new DeletedEntry(31, "32"));
            this.manager.Metadata.EditedTasks.Add(33, TaskProperties.Alarm | TaskProperties.Context);
            this.manager.Metadata.AfterSyncEditedTasks.Add(34);

            await this.manager.SaveAsync();
            await this.manager.InitializeAsync();

            // verify
            Assert.AreEqual(1, this.manager.Metadata.AddedFolders.Count);
            Assert.AreEqual(10, this.manager.Metadata.AddedFolders[0]);
            Assert.AreEqual(1, this.manager.Metadata.DeletedFolders.Count);
            Assert.AreEqual(11, this.manager.Metadata.DeletedFolders[0].FolderId);
            Assert.AreEqual("12", this.manager.Metadata.DeletedFolders[0].SyncId);
            Assert.AreEqual(1, this.manager.Metadata.EditedFolders.Count);
            Assert.AreEqual(13, this.manager.Metadata.EditedFolders[0]);
            Assert.AreEqual(1, this.manager.Metadata.AfterSyncEditedFolders.Count);
            Assert.AreEqual("14", this.manager.Metadata.AfterSyncEditedFolders[0]);

            Assert.AreEqual(1, this.manager.Metadata.AddedContexts.Count);
            Assert.AreEqual(20, this.manager.Metadata.AddedContexts[0]);
            Assert.AreEqual(1, this.manager.Metadata.DeletedContexts.Count);
            Assert.AreEqual(21, this.manager.Metadata.DeletedContexts[0].FolderId);
            Assert.AreEqual("22", this.manager.Metadata.DeletedContexts[0].SyncId);
            Assert.AreEqual(1, this.manager.Metadata.EditedContexts.Count);
            Assert.AreEqual(23, this.manager.Metadata.EditedContexts[0]);
            Assert.AreEqual(1, this.manager.Metadata.AfterSyncEditedContexts.Count);
            Assert.AreEqual("24", this.manager.Metadata.AfterSyncEditedContexts[0]);

            Assert.AreEqual(1, this.manager.Metadata.AddedTasks.Count);
            Assert.AreEqual(30, this.manager.Metadata.AddedTasks[0]);
            Assert.AreEqual(1, this.manager.Metadata.DeletedTasks.Count);
            Assert.AreEqual(31, this.manager.Metadata.DeletedTasks[0].FolderId);
            Assert.AreEqual("32", this.manager.Metadata.DeletedTasks[0].SyncId);
            Assert.AreEqual(1, this.manager.Metadata.EditedTasks.Count);
            Assert.AreEqual(TaskProperties.Alarm | TaskProperties.Context, this.manager.Metadata.EditedTasks[33]);
            Assert.AreEqual(1, this.manager.Metadata.AfterSyncEditedTasks.Count);
            Assert.AreEqual(34, this.manager.Metadata.AfterSyncEditedTasks[0]);
        }
    }
}

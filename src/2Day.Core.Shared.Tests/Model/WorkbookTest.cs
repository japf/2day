using System.IO;
using System.Linq;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLite.Net.Platform.Generic;
using SQLite.Net.Platform.Win32;

namespace Chartreuse.Today.Core.Shared.Tests.Model
{
    [TestClass]
    public class WorkbookTest
    {
        private Workbook workbook;
        private DatabaseContext datacontext;

        [TestInitialize]
        public void Initialize()
        {
            if (File.Exists("test.db"))
                File.Delete("test.db");

            this.datacontext = new DatabaseContext("./test.db", false, new SQLitePlatformWin32(), new TestTrackingManager());
            this.datacontext.InitializeDatabase();

            this.workbook = new Workbook(this.datacontext, new TestSettings());
            this.workbook.Settings.SetValue(CoreSettings.AutoDeleteTags, true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.datacontext.CloseConnection();
        }

        private void SaveAndReload()
        {
            this.datacontext.SendChanges();
            this.datacontext.CloseConnection();

            this.datacontext = new DatabaseContext("./test.db", false, new SQLitePlatformWin32(), new TestTrackingManager());
            this.datacontext.InitializeDatabase();

            this.workbook = new Workbook(this.datacontext, new TestSettings());
        }

        [TestMethod]
        public void SaveFolder()
        {
            // setup
            var folder = this.workbook.AddFolder("f1");

            // act
            this.SaveAndReload();

            // verify
            Assert.AreEqual(1, this.workbook.Folders.Count);
            Assert.AreEqual("f1", this.workbook.Folders[0].Name);
        }

        [TestMethod]
        public void RemoveContext()
        {
            // setup
            var folder = this.workbook.AddFolder("f1");
            var context = this.workbook.AddContext("c1");
            var task = new Task { Folder = folder, Context = context, Title = "t1" };

            // act
            this.SaveAndReload();

            // verify
            Assert.AreEqual(1, this.workbook.Tasks.Count);
            Assert.IsNotNull(this.workbook.Tasks[0].Context);
            Assert.AreEqual("c1", this.workbook.Tasks[0].Context.Name);

            // act
            this.workbook.Tasks[0].Context = null;

            // act
            this.SaveAndReload();

            // verify
            Assert.AreEqual(1, this.workbook.Tasks.Count);
            Assert.IsNull(this.workbook.Tasks[0].Context);
        }

        [TestMethod]
        public void SaveSubTask()
        {
            // setup
            var folder = this.workbook.AddFolder("f1");
            var task = new Task {Folder = folder, Title = "t1"};
            task.Children.Add(new Task {Title = "t1.1", ParentId = task.Id, Folder = folder });

            // act
            this.SaveAndReload();

            // verify
            Assert.AreEqual(1, this.workbook.Folders.Count);
            Assert.AreEqual("f1", this.workbook.Folders[0].Name);

            Assert.AreEqual(2, this.workbook.Tasks.Count);

            Assert.AreEqual("t1", this.workbook.Tasks[0].Title);
            Assert.IsNull(this.workbook.Tasks[0].ParentId);

            Assert.AreEqual("t1.1", this.workbook.Tasks[1].Title);
            Assert.IsNotNull(this.workbook.Tasks[1].ParentId);

            Assert.AreEqual(1, this.workbook.Tasks[0].Children.Count);
            Assert.AreEqual("t1.1", this.workbook.Tasks[0].Children[0].Title);
        }

        [TestMethod]
        public void Add_smartview_setup_order()
        {
            var sv1 = this.workbook.AddSmartView("sv1", "(priority is star)");
            var sv2 = this.workbook.AddSmartView("sv2", "(priority is star)");

            Assert.AreEqual(0, sv1.Order);
            Assert.AreEqual(1, sv2.Order);
        }

        [TestMethod]
        public void Load_views_does_not_create_duplicates()
        {
            // setup
            this.datacontext.AddSystemView(new SystemView { ViewKind = ViewKind.Today });
            this.datacontext.AddSystemView(new SystemView { ViewKind = ViewKind.Tomorrow });

            // act
            this.workbook.LoadViews();
            Assert.AreEqual(2, this.workbook.Views.Count());

            this.workbook.LoadViews();

            // check
            Assert.AreEqual(2, this.workbook.Views.Count());
        }

        [TestMethod]
        public void Remove_non_existing_tag()
        {
            // setup
            bool notification = false;
            this.workbook.TagRemoved += (s, e) => notification = true;
            var folder = this.workbook.AddFolder("f1");
            var task = new Task {Folder = folder, Tags = "tag1,tag3" };

            // act
            this.workbook.RemoveTag("tag2");

            // check
            Assert.AreEqual("tag1,tag3", task.Tags);
            Assert.IsFalse(notification);
            Assert.AreEqual(2, this.workbook.Tags.Count());
            Assert.AreEqual("tag1", this.workbook.Tags.ToList()[0].Name);
            Assert.AreEqual("tag3", this.workbook.Tags.ToList()[1].Name);
        }

        [TestMethod]
        public void Remove_existing_tag()
        {
            // setup
            bool notification = false;
            this.workbook.TagRemoved += (s, e) => notification = true; 
            var folder = this.workbook.AddFolder("f1");
            var task = new Task { Folder = folder, Tags = "tag1,tag2,tag3" };

            // act
            this.workbook.RemoveTag("tag2");

            // check
            Assert.AreEqual("tag1,tag3", task.Tags);
            Assert.IsTrue(notification);
            Assert.AreEqual(2, this.workbook.Tags.Count());
            Assert.AreEqual(2, this.workbook.Tags.Count());
            Assert.AreEqual("tag1", this.workbook.Tags.ToList()[0].Name);
            Assert.AreEqual("tag3", this.workbook.Tags.ToList()[1].Name);
        }

        [TestMethod]
        public void Remove_all()
        {
            // setup
            this.AddSampleTasks();

            // act
            this.workbook.RemoveAll();

            // check
            Assert.AreEqual(0, this.workbook.Tasks.Count);
            Assert.AreEqual(0, this.workbook.Folders.Count);
        }

        [TestMethod]
        public void Remove_tasks()
        {
            // setup
            this.AddSampleTasks();

            // act
            this.workbook.RemoveAllTasks();

            // check
            Assert.AreEqual(0, this.workbook.Tasks.Count);
            Assert.AreEqual(1, this.workbook.Folders.Count);
        }

        [TestMethod]
        public void Remove_completed()
        {
            // setup
            this.AddSampleTasks();

            // act
            this.workbook.RemoveCompletedTasks();

            // check
            Assert.AreEqual(2, this.workbook.Tasks.Count);
            Assert.AreEqual(1, this.workbook.Folders.Count);
        }

        [TestMethod]
        public void Complete_all()
        {
            // setup
            this.AddSampleTasks();

            // act
            this.workbook.CompleteTasks();

            // check
            Assert.IsTrue(this.workbook.Tasks.All(t => t.IsCompleted));
            Assert.AreEqual(1, this.workbook.Folders.Count);
        }

        [TestMethod]
        public void Rename_context()
        {
            // setup
            var context = this.workbook.AddContext("context1");

            // act
            context.TryRename(this.workbook, "Context1");

            // check
            Assert.AreEqual("Context1", context.Name);
        }

        [TestMethod]
        public void Add_tag_when_task_is_added()
        {
            // setup
            var folder = this.workbook.AddFolder("f1");
            var task1 = new Task { Folder = folder };

            // act
            task1.Tags = "tag1";

            // check
            var tags = this.workbook.Tags.ToList();
            Assert.AreEqual(1, tags.Count);
            Assert.AreEqual("tag1", tags[0].Name);
        }

        [TestMethod]
        public void Add_tags_when_task_is_added()
        {
            // setup
            var folder = this.workbook.AddFolder("f1");
            var task1 = new Task { Folder = folder };

            // act
            task1.Tags = "tag1, tag2";

            // check
            var tags = this.workbook.Tags.ToList();
            Assert.AreEqual(2, tags.Count);
            Assert.AreEqual("tag1", tags[0].Name);
            Assert.AreEqual("tag2", tags[1].Name);
        }

        [TestMethod]
        public void Update_tag_when_task_is_updated()
        {
            // setup
            var folder = this.workbook.AddFolder("f1");
            var task1 = new Task { Tags = "tag1", Folder = folder };

            // act
            task1.Tags = "tag2";

            // check
            var tags = this.workbook.Tags.ToList();
            Assert.AreEqual(1, tags.Count);
            Assert.AreEqual("tag2", tags[0].Name);
        }

        [TestMethod]
        public void Delete_tag_when_task_is_updated()
        {
            // setup
            var folder = this.workbook.AddFolder("f1");
            var task1 = new Task { Tags = "tag1", Folder = folder };

            // act
            task1.Tags = null;

            // check
            var tags = this.workbook.Tags.ToList();
            Assert.AreEqual(0, tags.Count);
        }

        [TestMethod]
        public void Keep_tag_when_task_is_updated()
        {
            // setup
            var folder = this.workbook.AddFolder("f1");
            var task1 = new Task { Tags = "tag1", Folder = folder };
            this.workbook.Settings.SetValue(CoreSettings.AutoDeleteTags, false);

            // act
            task1.Tags = null;

            // check
            var tags = this.workbook.Tags.ToList();
            Assert.AreEqual(1, tags.Count);
            Assert.AreEqual("tag1", tags[0].Name);
        }

        [TestMethod]
        public void Add_subtask()
        {
            // setup
            var folder = this.workbook.AddFolder("f1");
            var task1 = new Task { Tags = "tag1", Folder = folder };

        }

        private void AddSampleTasks()
        {
            var folder = this.workbook.AddFolder("f1");

            var task1 = new Task { Folder = folder };
            var task2 = new Task { Folder = folder };
            var task3 = new Task { Folder = folder, IsCompleted = true };
        }
    }
}

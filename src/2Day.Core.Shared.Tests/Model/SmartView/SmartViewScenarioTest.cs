using System;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Model.SmartView
{
    [TestClass]
    public class SmartViewScenarioTest
    {
        private Workbook workbook;
        private IFolder folder;
        private ISmartView smartview;
        private IContext context;

        [TestInitialize]
        public void Initialize()
        {
            this.workbook = new Workbook(new TestDatabaseContext(), new TestSettings());
            this.folder = this.workbook.AddFolder("f1");
            this.context = this.workbook.AddContext("@context");
            this.smartview = this.workbook.AddSmartView("SV1", "(Priority is Low)");
        }

        [TestMethod]
        public void Priority()
        {
            // setup
            this.smartview.Rules = "(Priority is Low)";
            new Task { Title = "t1", Folder = this.folder, Priority = TaskPriority.Low };
            new Task { Title = "t2", Folder = this.folder, Priority = TaskPriority.Medium };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Priority_is_not()
        {
            // setup
            this.smartview.Rules = "(Priority isnot Low)";
            new Task { Title = "t1", Folder = this.folder, Priority = TaskPriority.Low };
            new Task { Title = "t2", Folder = this.folder, Priority = TaskPriority.Medium };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t2", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Progress_is()
        {
            // setup
            var f1 = this.folder;
            this.smartview.Rules = "(Progress is 10)";
            new Task { Title = "t1", Folder = f1 };
            new Task { Title = "t2", Folder = f1, Progress = 0.1 };
            // act
            this.smartview.Rebuild();
            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t2", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Progress_is_not()
        {
            // setup
            var f1 = this.folder;
            this.smartview.Rules = "(Progress isnot 10)";
            new Task { Title = "t1", Folder = f1 };
            new Task { Title = "t2", Folder = f1, Progress = 0.1 };
            // act
            this.smartview.Rebuild();
            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Progress_is_more_than()
        {
            // setup
            this.smartview.Rules = "(Progress IsMoreThan 50)";
            new Task { Title = "t1", Folder = this.folder };
            new Task { Title = "t2", Folder = this.folder, Progress = 0 };
            new Task { Title = "t3", Folder = this.folder, Progress = 0.25 };
            new Task { Title = "t4", Folder = this.folder, Progress = 0.5 };
            new Task { Title = "t5", Folder = this.folder, Progress = 0.51 };
            new Task { Title = "t6", Folder = this.folder, Progress = 0.75 };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(2, this.smartview.TaskCount);
            Assert.AreEqual("t5", this.smartview.Tasks.ElementAt(0).Title);
            Assert.AreEqual("t6", this.smartview.Tasks.ElementAt(1).Title);
        }

        [TestMethod]
        public void Progress_is_less_than()
        {
            // setup
            this.smartview.Rules = "(Progress IsLessThan 50)";
            new Task { Title = "t1", Folder = this.folder };
            new Task { Title = "t2", Folder = this.folder, Progress = 0 };
            new Task { Title = "t3", Folder = this.folder, Progress = 0.25 };
            new Task { Title = "t4", Folder = this.folder, Progress = 0.49 };
            new Task { Title = "t5", Folder = this.folder, Progress = 0.5 };
            new Task { Title = "t6", Folder = this.folder, Progress = 0.75 };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(4, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
            Assert.AreEqual("t2", this.smartview.Tasks.ElementAt(1).Title);
            Assert.AreEqual("t3", this.smartview.Tasks.ElementAt(2).Title);
            Assert.AreEqual("t4", this.smartview.Tasks.ElementAt(3).Title);
        }

        [TestMethod]
        public void Due_and_context()
        {
            // setup
            this.smartview.Rules = "(Due DoesNotExist 0) AND (Context IsNot @context)";
            new Task { Title = "t1", Folder = this.folder };
            new Task { Title = "t2", Folder = this.folder, Due = DateTime.Now };        // ignored, because of due date
            new Task { Title = "t3", Folder = this.folder, Context = this.context };    // ignored, because of context

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Not_tag()
        {
            // setup
            this.smartview.Rules = "(Tags IsNot tag1)";
            new Task { Title = "t1", Folder = this.folder };
            new Task { Title = "t2", Folder = this.folder, Tags = "tag1" };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Tag_does_not_contain()
        {
            // setup
            this.smartview.Rules = "(Tags DoesNotContains tag1)";
            new Task { Title = "t1", Folder = this.folder, Tags = "tag3" };
            new Task { Title = "t2", Folder = this.folder, Tags = "tag1, tag2" };
            new Task { Title = "t3", Folder = this.folder };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(2, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
            Assert.AreEqual("t3", this.smartview.Tasks.ElementAt(1).Title);
        }

        [TestMethod]
        public void Start_does_not_exist_and_tag_does_not_contain_1()
        {
            // setup
            this.smartview.Rules = "(Start DoesNotExist 0) AND (Tags DoesNotContains tag1)";
            new Task { Title = "t1", Folder = this.folder, Tags = "tag3" };
            new Task { Title = "t2", Folder = this.folder, Start = DateTime.Now };      // excluded because start date
            new Task { Title = "t3", Folder = this.folder, Tags = "tag2, tag4, tag1" }; // excluded because tag1

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Start_does_not_exist_and_tag_does_not_contain_2()
        {
            // setup
            this.smartview.Rules = "(Start DoesNotExist 0 AND Tags DoesNotContains tag1)";
            new Task { Title = "t1", Folder = this.folder, Tags = "tag3" };
            new Task { Title = "t2", Folder = this.folder, Start = DateTime.Now };      // excluded because start date
            new Task { Title = "t3", Folder = this.folder, Tags = "tag2, tag4, tag1" }; // excluded because tag1

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Folder_is()
        {
            // setup
            var f1 = this.folder;
            var f2 = this.workbook.AddFolder("f2");
            this.smartview.Rules = "(Folder is f1)";
            new Task { Title = "t1", Folder = f1 };
            new Task { Title = "t2", Folder = f2 };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Folder_is_not()
        {
            // setup
            var f1 = this.folder;
            var f2 = this.workbook.AddFolder("f2");
            this.smartview.Rules = "(Folder isnot f1)";
            new Task { Title = "t1", Folder = f1 };
            new Task { Title = "t2", Folder = f2 };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t2", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Context_is()
        {
            // setup
            var c1 = this.workbook.AddContext("c1");
            var c2 = this.workbook.AddContext("c2");
            this.smartview.Rules = "(Context is c1)";
            new Task { Title = "t1", Folder = this.folder, Context = c1 };
            new Task { Title = "t2", Folder = this.folder, Context = c2 };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Context_is_not()
        {
            // setup
            var c1 = this.workbook.AddContext("c1");
            var c2 = this.workbook.AddContext("c2");
            this.smartview.Rules = "(Context isnot c1)";
            new Task { Title = "t1", Folder = this.folder, Context = c1 };
            new Task { Title = "t2", Folder = this.folder, Context = c2 };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t2", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Context_exist()
        {
            // setup
            var c1 = this.workbook.AddContext("c1");
            this.smartview.Rules = "(Context exists 0)";
            new Task { Title = "t1", Folder = this.folder, Context = c1 };
            new Task { Title = "t2", Folder = this.folder };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Context_does_not_exist()
        {
            // setup
            var c1 = this.workbook.AddContext("c1");
            this.smartview.Rules = "(Context doesnotexist 0)";
            new Task { Title = "t1", Folder = this.folder, Context = c1 };
            new Task { Title = "t2", Folder = this.folder };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t2", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Has_alarm()
        {
            // setup
            var f1 = this.folder;
            this.smartview.Rules = "(HasAlarm Yes False)";
            new Task { Title = "t1", Folder = f1 };
            new Task { Title = "t2", Folder = f1, Alarm = DateTime.Now };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t2", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Has_recurrence()
        {
            // setup
            var f1 = this.folder;
            this.smartview.Rules = "(HasRecurrence Yes False)";
            new Task { Title = "t1", Folder = f1 };
            new Task { Title = "t2", Folder = f1, FrequencyType = FrequencyType.Weekly };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t2", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Has_no_subtasks()
        {
            // setup
            var f1 = this.folder;
            this.smartview.Rules = "(HasSubtasks No False)";
            var t1 = new Task { Title = "t1", Folder = f1 };
            var t2 = new Task { Title = "t2", Folder = f1 };
            var subt2 = new Task { Title = "subt2", Folder = f1 };
            t2.AddChild(subt2);

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(2, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
            Assert.AreEqual("subt2", this.smartview.Tasks.ElementAt(1).Title);
        }

        [TestMethod]
        public void Has_subtasks()
        {
            // setup
            var f1 = this.folder;
            this.smartview.Rules = "(HasSubtasks Yes True)";
            var t1 = new Task { Title = "t1", Folder = f1 };
            var t2 = new Task { Title = "t2", Folder = f1 };
            var subt2 = new Task { Title = "subt2", Folder = f1 };
            t2.AddChild(subt2);

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t2", this.smartview.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void Due_date_is_today()
        {
            // setup
            var f1 = this.folder;
            this.smartview.Rules = "(Due IsToday 0)";
            new Task { Title = "t1", Folder = f1, Due = DateTime.Now };
            new Task { Title = "t2", Folder = f1, Due = DateTime.Now.AddDays(-1) };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
            
        }

        [TestMethod]
        public void Modified_is_today()
        {
            // setup
            var f1 = this.folder;
            this.smartview.Rules = "(Modified IsToday 0)";
            new Task { Title = "t1", Folder = f1, Modified = DateTime.Now };
            new Task { Title = "t2", Folder = f1, Modified = DateTime.Now.AddDays(-1) };

            // act
            this.smartview.Rebuild();

            // verify
            Assert.AreEqual(1, this.smartview.TaskCount);
            Assert.AreEqual("t1", this.smartview.Tasks.ElementAt(0).Title);
            
        }
    }
}
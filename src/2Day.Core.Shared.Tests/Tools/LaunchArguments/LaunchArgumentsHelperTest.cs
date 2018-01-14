using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Model.View;
using Chartreuse.Today.Core.Shared.Tests.Impl;
using Chartreuse.Today.Core.Shared.Tools.LaunchArguments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Tools.LaunchArguments
{
    [TestClass]
    public class LaunchArgumentsHelperTest
    {
        private IWorkbook workbook;

        [TestInitialize]
        public void Initialize()
        {
            this.workbook = new Workbook(new TestDatabaseContext(), new TestSettings());

            this.workbook.AddView(new SystemView { Id = 1 });
            this.workbook.AddView(new SystemView { Id = 2 });

            this.workbook.Folders.Add(new Folder { Id = 3, Color = "blue" });
            this.workbook.Folders.Add(new Folder { Id = 4, Color = "red" });

            this.workbook.AddSmartView("sv1", "(priority is 0)", 5);
            this.workbook.AddSmartView("sv1", "(priority is 0)", 6);

            this.workbook.AddContext("ctx1", null, 7);
            this.workbook.AddContext("ctx2", null, 8);

            this.workbook.Tasks.Add(new Task { Id = 9 });
            this.workbook.Tasks.Add(new Task { Id = 10 });
            this.workbook.Tasks.Add(new Task { Id = 11 });

            ((IList<ViewTag>)this.workbook.Tags).Add(new ViewTag(this.workbook, new Tag()) { Id = 12 });
            ((IList<ViewTag>)this.workbook.Tags).Add(new ViewTag(this.workbook, new Tag()) { Id = 13 });

            this.workbook.LoadViews();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HandleException()
        {
            LaunchArgumentsHelper.GetDescriptorFromArgument(null, "test");
        }

        [TestMethod]
        public void NullArgs()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, null);

            // verify
            Assert.IsNotNull(descriptor);
            Assert.IsNull(descriptor.Task);
            Assert.IsNull(descriptor.Folder);
            Assert.AreEqual(LaunchArgumentType.Unknown, descriptor.Type);
        }

        [TestMethod]
        public void EmptyArgs()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "");

            // verify
            Assert.IsNotNull(descriptor);
            Assert.IsNull(descriptor.Task);
            Assert.IsNull(descriptor.Folder);
            Assert.AreEqual(LaunchArgumentType.Unknown, descriptor.Type);
        }

        [TestMethod]
        public void WhitespaceArgs()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, " ");

            // verify
            Assert.IsNotNull(descriptor);
            Assert.IsNull(descriptor.Task);
            Assert.IsNull(descriptor.Folder);
            Assert.AreEqual(LaunchArgumentType.Unknown, descriptor.Type);
        }

        [TestMethod]
        public void NoAction_Task()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "task-9");

            // verify
            Assert.AreEqual(this.workbook.Tasks[0], descriptor.Task);
            Assert.AreEqual(LaunchArgumentType.EditTask, descriptor.Type);
            Assert.IsNull(descriptor.Folder);
        }

        [TestMethod]
        public void Edit_Task_WithPrefix()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "edit/task-9");

            // verify
            Assert.AreEqual(this.workbook.Tasks[0], descriptor.Task);
            Assert.AreEqual(LaunchArgumentType.EditTask, descriptor.Type);
            Assert.IsNull(descriptor.Folder);
        }

        [TestMethod]
        public void Edit_Task_NoPrefix()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "task-9");

            // verify
            Assert.AreEqual(this.workbook.Tasks[0], descriptor.Task);
            Assert.AreEqual(LaunchArgumentType.EditTask, descriptor.Type);
            Assert.IsNull(descriptor.Folder);
        }

        [TestMethod]
        public void Complete_Task()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "complete/task-9");

            // verify
            Assert.AreEqual(this.workbook.Tasks[0], descriptor.Task);
            Assert.AreEqual(LaunchArgumentType.CompleteTask, descriptor.Type);
            Assert.IsNull(descriptor.Folder);
        }

        [TestMethod]
        public void Folder()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "folder-3");

            // verify
            Assert.AreEqual(this.workbook.Folders[0], descriptor.Folder);
            Assert.AreEqual(LaunchArgumentType.Select, descriptor.Type);
            Assert.IsNull(descriptor.Task);
        }

        [TestMethod]
        public void UnkownFolder()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "folder-100");

            // verify
            Assert.AreEqual(LaunchArgumentType.Unknown, descriptor.Type);
            Assert.IsNull(descriptor.Task);
            Assert.IsNull(descriptor.Folder);
        }

        [TestMethod]
        public void UnkownTask()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "task-100");

            // verify
            Assert.AreEqual(LaunchArgumentType.Unknown, descriptor.Type);
            Assert.IsNull(descriptor.Task);
            Assert.IsNull(descriptor.Folder);
        }

        [TestMethod]
        public void View()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "view-1");

            // verify
            Assert.AreEqual(this.workbook.Views.ElementAt(0), descriptor.Folder);
            Assert.AreEqual(LaunchArgumentType.Select, descriptor.Type);
            Assert.IsNull(descriptor.Task);
        }

        [TestMethod]
        public void Context()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "context-7");

            // verify
            Assert.AreEqual(this.workbook.Contexts[0], descriptor.Folder);
            Assert.AreEqual(LaunchArgumentType.Select, descriptor.Type);
            Assert.IsNull(descriptor.Task);
        }

        [TestMethod]
        public void Tag()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "tag-12");

            // verify
            Assert.AreEqual(this.workbook.Tags.ElementAt(0), descriptor.Folder);
            Assert.AreEqual(LaunchArgumentType.Select, descriptor.Type);
            Assert.IsNull(descriptor.Task);
        }

        [TestMethod]
        public void SmartView()
        {
            // act
            var descriptor = LaunchArgumentsHelper.GetDescriptorFromArgument(this.workbook, "smartview-5");

            // verify
            Assert.AreEqual(this.workbook.SmartViews[0], descriptor.Folder);
            Assert.AreEqual(LaunchArgumentType.Select, descriptor.Type);
            Assert.IsNull(descriptor.Task);
        }

        [TestMethod]
        public void GetArgEditTask()
        {
            Assert.AreEqual("task-9", LaunchArgumentsHelper.GetArgEditTask(this.workbook.Tasks[0]));
        }

        [TestMethod]
        public void GetArgCompleteTask()
        {
            Assert.AreEqual("complete/task-9", LaunchArgumentsHelper.GetArgCompleteTask(this.workbook.Tasks[0]));
        }

        [TestMethod]
        public void GetArgSelectView()
        {
            Assert.AreEqual("view-" + this.workbook.Views.ElementAt(0).Id, LaunchArgumentsHelper.GetArgSelectFolder(this.workbook.Views.ElementAt(0)));
        }

        [TestMethod]
        public void GetArgSelectFolder()
        {
            Assert.AreEqual("folder-" + this.workbook.Folders.ElementAt(0).Id, LaunchArgumentsHelper.GetArgSelectFolder(this.workbook.Folders.ElementAt(0)));
        }

        [TestMethod]
        public void GetArgSelectSmartView()
        {
            Assert.AreEqual("smartview-" + this.workbook.SmartViews.ElementAt(0).Id, LaunchArgumentsHelper.GetArgSelectFolder(this.workbook.SmartViews.ElementAt(0)));
        }

        [TestMethod]
        public void GetArgSelectContext()
        {
            Assert.AreEqual("context-" + this.workbook.Contexts.ElementAt(0).Id, LaunchArgumentsHelper.GetArgSelectFolder(this.workbook.Contexts.ElementAt(0)));
        }

        [TestMethod]
        public void GetArgSelectTag()
        {
            Assert.AreEqual("tag-" + this.workbook.Tags.ElementAt(0).Id, LaunchArgumentsHelper.GetArgSelectFolder(this.workbook.Tags.ElementAt(0)));
        }
    }
}

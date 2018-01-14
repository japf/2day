using System;
using System.Linq;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLite.Net.Platform.Generic;
using SQLite.Net.Platform.Win32;

namespace Chartreuse.Today.Core.Shared.Tests.IO
{
    [TestClass]
    public class DatabaseContextText
    {
        [TestMethod]
        public void AddTask()
        {
            DatabaseContextTestCase<Task>.RunTest(
                (c, i) => c.AddTask(i),
                (c, i) => c.RemoveTask(i),
                c => c.Tasks.Count(),
                (i, v) => i.Title = v,
                c => c.Tasks.ElementAt(0).Title);
        }

        [TestMethod]
        public void AddFolder()
        {
            DatabaseContextTestCase<Folder>.RunTest(
                (c, i) => c.AddFolder(i),
                (c, i) => c.RemoveFolder(i),
                c => c.Folders.Count(),
                (i, v) => i.Name = v,
                c => c.Folders.ElementAt(0).Name);
        }

        [TestMethod]
        public void AddContext()
        {
            DatabaseContextTestCase<Context>.RunTest(
                (c, i) => c.AddContext(i),
                (c, i) => c.RemoveContext(i),
                c => c.Contexts.Count(),
                (i, v) => i.Name = v,
                c => c.Contexts.ElementAt(0).Name);
        }

        [TestMethod]
        public void AddTag()
        {
            DatabaseContextTestCase<Tag>.RunTest(
                (c, i) => c.AddTag(i),
                (c, i) => c.RemoveTag(i),
                c => c.Tags.Count(),
                (i, v) => i.Name = v,
                c => c.Tags.ElementAt(0).Name);
        }

        [TestMethod]
        public void AddSmartView()
        {
            DatabaseContextTestCase<SmartView>.RunTest(
                (c, i) => c.AddSmartView(i),
                (c, i) => c.RemoveSmartView(i),
                c => c.SmartViews.Count(),
                (i, v) => i.Name = v,
                c => c.SmartViews.ElementAt(0).Name);
        }

        [TestMethod]
        public void AddView()
        {
            // setup
            var filename = Guid.NewGuid().ToString();
            var view = new SystemView { Name = "view", ViewKind = ViewKind.Completed };
            var context = DatabaseContextText.CreateDatabaseContext(filename);

            // act
            context.AddSystemView(view);
            context.SendChanges();

            // assert
            var context2 = DatabaseContextText.CreateDatabaseContext(filename);
            Assert.AreEqual(1, context2.Views.Count());
            Assert.AreEqual("view", context2.Views.ElementAt(0).Name);
            Assert.AreEqual(ViewKind.Completed, context2.Views.ElementAt(0).ViewKind);
        }

        private class DatabaseContextTestCase<TItem> where TItem : new()
        {
            private readonly Action<IDatabaseContext, TItem> addItem;
            private readonly Action<IDatabaseContext, TItem> removeItem;
            private readonly Func<IDatabaseContext, int> getItemCount;
            private readonly Action<TItem, string> setItemProperty;
            private readonly Func<IDatabaseContext, string> getItemProperty;

            private string filename;
            private DatabaseContext context;

            private DatabaseContextTestCase(Action<IDatabaseContext, TItem> addItem, Action<IDatabaseContext, TItem> removeItem, Func<IDatabaseContext, int> getItemCount, Action<TItem, string> setItemProperty, Func<IDatabaseContext, string> getItemProperty)
            {
                this.addItem = addItem;
                this.removeItem = removeItem;
                this.getItemCount = getItemCount;
                this.setItemProperty = setItemProperty;
                this.getItemProperty = getItemProperty;
            }

            public static void RunTest(Action<IDatabaseContext, TItem> addItem, Action<IDatabaseContext, TItem> removeItem, Func<IDatabaseContext, int> getItemCount, Action<TItem, string> setItemProperty, Func<IDatabaseContext, string> getItemProperty)
            {
                var instance = new DatabaseContextTestCase<TItem>(addItem, removeItem, getItemCount, setItemProperty, getItemProperty);
                instance.RunTestCore();
            }

            private void RunTestCore()
            {
                this.ResetDatebaseContextFilename();
                this.Add();

                this.ResetDatebaseContextFilename();
                this.AddMultiple();

                this.ResetDatebaseContextFilename();
                this.Remove();
            }

            private void ResetDatebaseContextFilename()
            {
                this.filename = string.Format("test-{0}.db", Guid.NewGuid());
                this.context = DatabaseContextText.CreateDatabaseContext(this.filename);
            }

            private void Add()
            {
                // setup
                var item = new TItem();
                this.setItemProperty(item, "new value");

                // act
                this.addItem(this.context, item);
                this.context.SendChanges();

                // assert
                var context2 = DatabaseContextText.CreateDatabaseContext(this.filename);
                Assert.AreEqual(1, this.getItemCount(context2));
                Assert.AreEqual("new value", this.getItemProperty(context2));
            }

            private void Remove()
            {
                // setup
                var item = new TItem();
                this.setItemProperty(item, "new value");
                this.addItem(this.context, item);
                this.context.SendChanges();

                // act
                this.removeItem(this.context, item);

                // assert
                var context2 = DatabaseContextText.CreateDatabaseContext(this.filename);
                Assert.AreEqual(0, this.getItemCount(context2));
            }

            private void AddMultiple()
            {
                // setup
                var context2 = DatabaseContextText.CreateDatabaseContext(this.filename);
                var item = new TItem();
                this.setItemProperty(item, "new value");

                // act
                this.addItem(context2, item);
                context2.SendChanges();

                this.addItem(this.context, item);           // add to original context while task is already persisted on disk            
                this.setItemProperty(item, "new title");    // make sure we keep track of changes properly
                this.context.SendChanges();

                // assert
                var context3 = CreateDatabaseContext(this.filename);
                Assert.AreEqual(2, this.getItemCount(context3));
            }            
        }

        private static DatabaseContext CreateDatabaseContext(string filename)
        {
            var newContext = new DatabaseContext(filename, false, new SQLitePlatformWin32(), new TestTrackingManager());
            newContext.InitializeDatabase();

            return newContext;
        }
    }
}

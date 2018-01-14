using System;
using System.Collections.Generic;
using Chartreuse.Today.App.Manager.UI;
using Chartreuse.Today.App.Test.Impl;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Universal.Model;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Chartreuse.Today.App.Test.Tools
{
    [TestClass]
    public class NavigationMenuManagerTest
    {
        private const string SmartViewRule = "(Title BeginsWith e)";

        private Workbook workbook;
        private MainPageViewModel viewmodel;
        private List<ISystemView> views;
        private IDatabaseContext dbContext;

        [TestInitialize]
        public void Initialize()
        {
            this.dbContext = new FakeDatabaseContext();
            this.workbook = new Workbook(this.dbContext, WinSettings.Instance);

            this.views = new List<ISystemView>
            {
                new SystemView { ViewKind = ViewKind.Today },
                new SystemView { ViewKind = ViewKind.Tomorrow },
                new SystemView { ViewKind = ViewKind.Starred },
                new SystemView { ViewKind = ViewKind.Late }
            };

            foreach (var view in this.views)
                this.dbContext.AddSystemView(view);

            this.workbook.Initialize();

            UITestHelper.ExecuteOnUIThread(() =>
            {
                this.viewmodel = this.CreateMainPageViewModel();
                var manager = new NavigationMenuManager(this.workbook, new TestSynchronizationManager(), this.viewmodel);
            });
        }

        [TestMethod]
        public void It_must_be_empty_when_workbook_is_empty()
        {
            this.AssertMenu();
        }

        [TestMethod]
        public void It_must_track_views()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                this.views[0].IsEnabled = true;
                this.AssertMenu("today");

                this.views[0].IsEnabled = false;
                this.AssertMenu();

                this.views[0].IsEnabled = true;
                this.views[1].IsEnabled = true;
                this.AssertMenu("today", "tomorrow");
            });
        }

        [TestMethod]
        public void It_must_track_smart_views()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                this.workbook.AddSmartView("SV1", SmartViewRule);

                this.AssertMenu("SV1");

                this.workbook.RemoveSmartView("SV1");

                this.AssertMenu();
            });
        }

        [TestMethod]
        public void It_must_track_folders()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                this.workbook.AddFolder("1");
                this.AssertMenu("1");

                this.workbook.RemoveFolder("1");
                this.AssertMenu();

                this.workbook.AddFolder("2");
                this.workbook.AddFolder("3");
                this.AssertMenu("2", "3");
            });
        }

        [TestMethod]
        public void It_must_track_contexts()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                this.workbook.AddContext("1");
                this.AssertMenu("1");

                this.workbook.RemoveContext("1");
                this.AssertMenu();

                this.workbook.AddContext("2");
                this.workbook.AddContext("3");
                this.AssertMenu("2", "3");
            });
        }

        [TestMethod]
        public void It_must_track_tags()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                this.workbook.AddTag("1");
                this.AssertMenu("1");

                this.workbook.RemoveTag("1");
                this.AssertMenu();

                this.workbook.AddTag("2");
                this.workbook.AddTag("3");
                this.AssertMenu("2", "3");
            });
        }

        [TestMethod]
        public void It_must_manage_separators()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                this.AssertMenu();

                this.views[0].IsEnabled = true;
                this.AssertMenu("today");

                this.workbook.AddFolder("f1");
                this.AssertMenu("today", "", "f1");

                this.views[1].IsEnabled = true;
                this.AssertMenu("today", "tomorrow", "", "f1");

                this.workbook.AddContext("c2");
                this.AssertMenu("today", "tomorrow", "", "f1", "", "c2");

                this.workbook.AddTag("t3");
                this.AssertMenu("today", "tomorrow", "", "f1", "", "c2", "", "t3");

                this.workbook.AddSmartView("sv1", SmartViewRule);
                this.AssertMenu("today", "tomorrow", "", "sv1", "", "f1", "", "c2", "", "t3");

                this.views[1].IsEnabled = false;
                this.AssertMenu("today", "", "sv1", "", "f1", "", "c2", "", "t3");

                this.views[0].IsEnabled = false;
                this.AssertMenu("sv1", "", "f1", "", "c2", "", "t3");

                this.workbook.RemoveFolder("f1");
                this.AssertMenu("sv1", "", "c2", "", "t3");

                this.workbook.RemoveTag("t3");
                this.AssertMenu("sv1", "", "c2");

                this.workbook.RemoveContext("c2");
                this.AssertMenu("sv1");

                this.workbook.RemoveSmartView("sv1");
                this.AssertMenu();
            });
        }

        [TestMethod]
        public void It_must_manage_separators_add_context()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                this.AssertMenu();

                this.views[0].IsEnabled = true;
                this.workbook.AddTag("t2");
                this.workbook.AddFolder("f1");

                this.AssertMenu("today", "", "f1", "", "t2");

                this.workbook.AddContext("c3");

                this.AssertMenu("today", "", "f1", "", "c3", "", "t2");
            });
        }

        [TestMethod]
        public void Starts_with_2_smart_views()
        {
            UITestHelper.ExecuteOnUIThread(() =>
            {
                this.viewmodel = this.CreateMainPageViewModel();

                var manager = new NavigationMenuManager(this.workbook, new TestSynchronizationManager(), this.viewmodel);

                this.AssertMenu();

                this.views[0].IsEnabled = true;
                this.workbook.AddSmartView("sv1", SmartViewRule);
                this.workbook.AddSmartView("sv2", SmartViewRule);


                this.AssertMenu("today", "", "sv1", "sv2");
            });
        }

        private MainPageViewModel CreateMainPageViewModel()
        {
            return new MainPageViewModel(
                this.workbook,
                new TestSynchronizationManager(),
                new TestStartupManager(),
                new TestMessageBoxService(),
                new TestNotificationService(),
                new TestNavigationService(),
                new TestPlatformService(),
                new TestTileManager(),
                new TestTrackingManager(),
                new TestSpeechService());
        }


        private void AssertMenu(params string[] content)
        {
            Assert.AreEqual(content.Length, this.viewmodel.MenuItems.Count);
            for (int i = 0; i < content.Length; i++)
            {
                if (!string.IsNullOrEmpty(content[i]))
                    Assert.AreEqual(content[i], this.viewmodel.MenuItems[i].Name);
            }
        }

        private class FakeDatabaseContext : IDatabaseContext
        {
            private readonly List<ISystemView> views;
            private readonly List<IFolder> folders;
            private readonly List<IContext> contexts;
            private readonly List<ITag> tags;
            private readonly List<ITask> tasks;
            private readonly List<ISmartView> smartViews;

            public FakeDatabaseContext()
            {
                this.views = new List<ISystemView>();
                this.folders = new List<IFolder>();
                this.contexts = new List<IContext>();
                this.tags = new List<ITag>();
                this.tasks = new List<ITask>();
                this.smartViews = new List<ISmartView>();
            }

            public string FullPathFile
            {
                get { return string.Empty; }
            }

            public string Filename
            {
                get { return string.Empty; }
            }

            public IEnumerable<ISystemView> Views
            {
                get { return this.views; }
            }

            public void AddSystemView(ISystemView view)
            {
                this.views.Add(view);
            }

            public void RemoveSystemView(ISystemView view)
            {
                this.views.Remove(view);
            }

            public IEnumerable<ISmartView> SmartViews
            {
                get { return this.smartViews; }
            }

            public void AddSmartView(ISmartView view)
            {
                this.smartViews.Add(view);
            }

            public void RemoveSmartView(ISmartView view)
            {
                this.smartViews.Remove(view);
            }

            public IEnumerable<IFolder> Folders
            {
                get { return this.folders; }
            }

            public void AddFolder(IFolder folder)
            {
                this.folders.Add(folder);
            }

            public void RemoveFolder(IFolder folder)
            {
                this.folders.Remove(folder);
            }

            public IEnumerable<IContext> Contexts
            {
                get { return this.contexts; }
            }

            public void AddContext(IContext context)
            {
                this.contexts.Add(context);
            }

            public void RemoveContext(IContext context)
            {
                this.contexts.Remove(context);
            }

            public IEnumerable<ITag> Tags
            {
                get { return this.tags; }
            }

            public void AddTag(ITag tag)
            {
                this.tags.Add(tag);
            }

            public void RemoveTag(ITag tag)
            {
                this.tags.Remove(tag);
            }

            public IEnumerable<ITask> Tasks
            {
                get { return this.tasks; }
            }

            public void AddTask(ITask task)
            {
                this.tasks.Add(task);
            }

            public void RemoveTask(ITask task)
            {
                this.tasks.Remove(task);
            }

            public void SendChanges()
            {
            }

            public IDisposable WithTransaction()
            {
                return new DisposableAction(() => { });
            }

            public IDisposable WithDuplicateProtection()
            {
                return new DisposableAction(() => { });
            }
        }
    }
}

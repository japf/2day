using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.Core.Shared.Tests.Impl
{
    public class TestDatabaseContext : IDatabaseContext
    {
        private readonly List<ISystemView> views;
        private readonly List<IFolder> folders;
        private readonly List<IContext> contexts;
        private readonly List<ITag> tags;
        private readonly List<ITask> tasks;
        private readonly List<ISmartView> smartViews;

        private static int viewId;
        private static int folderId;
        private static int tagId;
        private static int taskId;
        private static int contextId;
        private static int smartviewId;

        public TestDatabaseContext()
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
            if (view.Id == 0)
                view.Id = viewId++;

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
            if (view.Id == 0)
                view.Id = smartviewId++;

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
            if (folder.Id == 0)
                folder.Id = folderId++;

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
            if (context.Id == 0)
                context.Id = contextId++;

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
            if (tag.Id == 0)
                tag.Id = tagId++;

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
            task.Id = taskId++;
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
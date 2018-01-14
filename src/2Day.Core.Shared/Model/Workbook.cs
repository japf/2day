using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Chartreuse.Today.Core.Shared.Icons;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Model.View;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using SmartView = Chartreuse.Today.Core.Shared.Model.Smart.SmartView;

namespace Chartreuse.Today.Core.Shared.Model
{
    public class Workbook : NotifyPropertyChanged, IWorkbook
    {
        private readonly IDatabaseContext databaseContext;
        private readonly ISettings settings;

        private readonly List<ITask> tasks;
        private readonly List<IFolder> trackedFolders;
        private readonly List<ISystemView> views;
        private readonly List<IContext> trackedContexts;
        private readonly List<ISmartView> trackedSmartViews; 
        private List<ViewTag> tags;

        private List<ISmartView> smartViews;
        private List<IFolder> folders;
        private List<IContext> contexts;

        public event EventHandler<EventArgs> ViewsReordered;

        public event EventHandler<EventArgs<IFolder>> FolderRemoved;
        public event EventHandler<EventArgs<IFolder>> FolderAdded;
        public event EventHandler<EventArgs> FoldersReordered;
        public event EventHandler<PropertyChangedEventArgs> FolderChanged;

        public event EventHandler<EventArgs<ISmartView>> SmartViewRemoved;
        public event EventHandler<EventArgs<ISmartView>> SmartViewAdded;
        public event EventHandler<EventArgs> SmartViewsReordered;
        public event EventHandler<PropertyChangedEventArgs> SmartViewChanged;

        public event EventHandler<EventArgs<IContext>> ContextRemoved;
        public event EventHandler<EventArgs<IContext>> ContextAdded;
        public event EventHandler<EventArgs> ContextsReordered;
        public event EventHandler<PropertyChangedEventArgs> ContextChanged;

        public event EventHandler<EventArgs<ITask>> TaskRemoved;
        public event EventHandler<EventArgs<ITask>> TaskAdded;
        public event EventHandler<PropertyChangedEventArgs> TaskChanged;

        public event EventHandler<EventArgs<ITag>> TagRemoved;
        public event EventHandler<EventArgs<ITag>> TagAdded;
        public event EventHandler<EventArgs> TagsReordered;

        public bool IgnoreFolderChanges { get; set; }

        public ISettings Settings
        {
            get
            {
                return this.settings;
            }
        }

        public IList<IFolder> Folders
        {
            get { return this.folders; }
        }

        public IList<IContext> Contexts
        {
            get { return this.contexts; }
        }

        public IEnumerable<ISystemView> Views
        {
            get { return this.views; }
        }

        public IList<ISmartView> SmartViews
        {
            get { return this.smartViews; }
        }

        public IEnumerable<ITag> Tags
        {
            get { return this.tags; }
        }

        public IList<ITask> Tasks
        {
            get { return this.tasks; }
        }

        public IFolder this[int folderId]
        {
            get { return this.folders.FirstOrDefault(f => f.Id == folderId); }
        }

        protected int DefaultFolderIconId
        {
            get { return FontIconHelper.IconIdFolder; }
        }

        public int DefaultSyncIconId
        {
            get { return FontIconHelper.IconIdCloud; }
        }

        protected string DefaultFolderColor
        {
            get { return ColorChooser.Blue; }
        }

        public Workbook(IDatabaseContext databaseContext, ISettings settings)
        {
            if (databaseContext == null)
                throw new ArgumentNullException(nameof(databaseContext));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            this.databaseContext = databaseContext;
            this.settings = settings;
            this.trackedFolders = new List<IFolder>();
            this.trackedContexts = new List<IContext>();
            this.trackedSmartViews = new List<ISmartView>();

            this.tasks = this.databaseContext.Tasks.ToList();
            foreach (var subTask in this.databaseContext.Tasks.Where(t => t.ParentId != null))
            {
                var parentTask = this.tasks.FirstOrDefault(t => t.Id == subTask.ParentId.Value);
                if (parentTask != null)
                {
                    parentTask.AddChild(subTask);
                }
                else
                {
                    LogService.Log(LogLevel.Warning, "Workbook", $"Could not find parent task with id {subTask.ParentId}");
                }
            }

            this.folders = this.databaseContext.Folders.OrderBy(f => f.Order).ToList();
            this.contexts = this.databaseContext.Contexts.OrderBy(c => c.Order).ToList();

            this.views = new List<ISystemView>();
            this.smartViews = new List<ISmartView>();
            this.tags = new List<ViewTag>();
        }

        public ITask CreateTask(int? id = null)
        {
            var task = new Task { Added = DateTime.Now, Modified = DateTime.Now };
            if (id.HasValue)
                task.Id = id.Value;

            return task;
        }
        
        protected IFolder InstanciateFolder(string name, string syncId)
        {
            var folder = new Folder
            {
                Name = name,
                SyncId = syncId,
                ShowInViews = true,
                Color = this.DefaultFolderColor,
                IconId = this.DefaultFolderIconId
            };

            return folder;
        }

        protected IContext InstanciateContext(string name, string syncId)
        {
            return new Context { Name = name, SyncId = syncId };
        }

        protected ITag InstanciateTag(string name)
        {
            return new Tag { Name = name };
        }    
        
        private List<string> GetTagsFromTasks()
        {
            return this.Tasks
                .SelectMany(t => t.Tags != null ? t.Tags.Split(Constants.TagSeparator) : Enumerable.Empty<string>())
                .Distinct()
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .ToList();
        }

        public void CommitEditedChanges()
        {
            this.databaseContext.SendChanges();
        }

        public IDisposable WithTransaction()
        {
            return this.databaseContext.WithTransaction();
        }

        public IDisposable WithDuplicateProtection()
        {
            return this.databaseContext.WithDuplicateProtection();
        }

        public void Initialize()
        {
            foreach (var task in this.Tasks.Where(t => t.Folder == null || string.IsNullOrEmpty(t.Title)).ToList())
            {
                this.databaseContext.RemoveTask(task);
                this.databaseContext.SendChanges();

                this.tasks.Remove(task);
            }

            foreach (var folder in this.Folders)
                this.StartTrackingFolder(folder);

            foreach (var context in this.Contexts)
                this.StartTrackingContext(context);

            this.LoadViews();

            foreach (var view in this.databaseContext.SmartViews.OrderBy(f => f.Order))
            {
                if (!string.IsNullOrEmpty(view.Rules))
                {
                    var smartView = new SmartView(this, view);
                    this.smartViews.Add(smartView);
                    this.StartTrackingSmartView(smartView);
                }
            }

            foreach (var tag in this.databaseContext.Tags.OrderBy(f => f.Order))
            {
                this.tags.Add(new ViewTag(this, tag));
            }
        }

        public void LoadViews()
        {
            foreach (var view in this.databaseContext.Views.OrderBy(f => f.Order))
            {
                if (!this.views.Any(v => v.ViewKind == view.ViewKind))
                {
                    ISystemView systemView = ViewFactory.BuildView(this, view);
                    systemView.PropertyChanged += this.OnFolderPropertyChanged;

                    this.views.Add(systemView);
                }
            }
        }

        public void AddView(ISystemView view)
        {
            this.databaseContext.AddSystemView(view);
            this.databaseContext.SendChanges();
        }

        public ISmartView AddSmartView(string name, string rules, int? id = null)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (rules == null)
                throw new ArgumentNullException("rules");

            // make sure no smart view has this name
            if (this.SmartViews.Any(f => f.Name == name))
                return null;

            var view = new Impl.SmartView { Name = name, Rules = rules };
            if (id.HasValue)
                view.Id = id.Value;

            ISmartView smartView = new SmartView(this, view);
            smartView.GroupAscending = true;
            smartView.TaskGroup = TaskGroup.DueDate;
            smartView.Order = this.SmartViews.Count;

            this.databaseContext.AddSmartView(view);
            this.databaseContext.SendChanges();

            this.smartViews.Add(smartView);

            this.StartTrackingSmartView(smartView);

            this.SmartViewAdded.Raise(this, new EventArgs<ISmartView>(smartView));

            return smartView;

        }

        public IFolder AddFolder(string name, string syncId = null, int? id = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // make sure no folder has this name
            if (this.Folders.Any(f => f.Name == name))
                return null;

            var folder = this.InstanciateFolder(name, syncId);
            if (id.HasValue)
                folder.Id = id.Value;

            folder.Order = this.Folders.Count;

            LogService.Log("Workbook", string.Format("Adding folder {0}", folder.Name));

            this.databaseContext.AddFolder(folder);
            this.databaseContext.SendChanges();

            this.folders = this.databaseContext.Folders.OfType<IFolder>().ToList();

            this.StartTrackingFolder(folder);

            this.FolderAdded.Raise(this, new EventArgs<IFolder>(folder));

            return folder;
        }

        public bool RemoveFolder(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // make sure a folder has this name
            var folder = this.Folders.FirstOrDefault(f => f.Name == name);
            if (folder == null)
                return false;

            LogService.Log("Workbook", string.Format("Removing folder {0}", folder.Name));

            // remove all its tasks
            foreach (var task in folder.Tasks.ToList())
                task.Delete();

            this.databaseContext.RemoveFolder(folder);
            this.databaseContext.SendChanges();

            this.folders.Remove(folder);

            this.StopTrackingFolder(folder);

            this.FolderRemoved.Raise(this, new EventArgs<IFolder>(folder));

            return true;
        }

        public bool RemoveSmartView(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // make sure a smart view has this name
            var smartview = this.SmartViews.FirstOrDefault(f => f.Name == name) as SmartView;
            if (smartview == null)
                return false;

            LogService.Log("Workbook", string.Format("Removing smartView {0}", smartview.Name));

            this.databaseContext.RemoveSmartView(smartview.Owner);
            this.databaseContext.SendChanges();

            this.smartViews.Remove(smartview);

            this.StopTrackingSmartView(smartview);

            this.SmartViewRemoved.Raise(this, new EventArgs<ISmartView>(smartview));

            return true;
        }

        public IContext AddContext(string name, string syncId = null, int? id = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            name = name.TryTrim();
            if (string.IsNullOrEmpty(name))
                return null;

            // make sure no context has this name
            if (this.Contexts.Any(f => f.Name == name))
                return null;

            var context = this.InstanciateContext(name, syncId);
            if (id.HasValue)
                context.Id = id.Value;

            context.Order = this.Contexts.Count - 1;

            LogService.Log("Workbook", string.Format("Adding context {0}", context.Name));

            this.databaseContext.AddContext(context);
            this.databaseContext.SendChanges();

            this.contexts = this.databaseContext.Contexts.OfType<IContext>().ToList();

            this.StartTrackingContext(context);

            this.ContextAdded.Raise(this, new EventArgs<IContext>(context));

            return context;
        }

        public bool RemoveContext(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // make sure a context has this name
            var context = this.Contexts.FirstOrDefault(c => c.Name == name);
            if (context == null)
                return false;

            LogService.Log("Workbook", string.Format("Removing context {0}", context.Name));

            // set no context to all the associated tasks
            foreach (var task in context.Tasks.ToList())
                task.Context = null;

            this.databaseContext.RemoveContext(context);
            this.databaseContext.SendChanges();

            this.contexts.Remove(context);

            this.StopTrackingContext(context);

            this.ContextRemoved.Raise(this, new EventArgs<IContext>(context));

            return true;
        }

        public void CompleteTasks()
        {
            using (this.WithTransaction())
            {
                foreach (var folder in this.folders)
                {
                    foreach (var task in folder.Tasks.ToList())
                        task.IsCompleted = true;
                }                
            }
        }

        public void RemoveCompletedTasks()
        {
            using (this.WithTransaction())
            {
                foreach (var task in this.Tasks.Where(t => t.IsCompleted).ToList())
                    task.Delete();   
            }
        }

        public void RemoveAllTasks()
        {
            using (this.WithTransaction())
            {
                foreach (var folder in this.folders)
                {
                    foreach (var task in folder.Tasks.ToList())
                        folder.RemoveTask(task);
                }
            }
        }

        public void RemoveAll()
        {
            using (this.WithTransaction())
            {
                LogService.Log("Workbook", "Clearing");
                foreach (var smartview in this.SmartViews.ToList())
                {
                    this.RemoveSmartView(smartview.Name);
                }

                foreach (var folder in this.Folders.ToList())
                {
                    foreach (var task in folder.Tasks.ToList())
                    {
                        folder.RemoveTask(task);
                    }

                    this.RemoveFolder(folder.Name);
                }
                foreach (var context in this.Contexts.ToList())
                {
                    this.RemoveContext(context.Name);
                }
            }
        }

        public int RemoveOldTasks()
        {
            int count = 0;
            int minDays = (int)this.Settings.GetValue<AutoDeleteFrequency>(CoreSettings.AutoDeleteFrequency);
            if (minDays == 0)
                return 0;

            DateTime date = DateTime.Now.Date;
            // iterate over the completed tasks, skipping child tasks
            foreach (var task in this.Tasks.Where(t => t.IsCompleted && t.ParentId == null).ToList())
            {
                // compute the delay between now and when the task gets completed
                var timespanCompleted = date - task.Completed.Value.Date;

                // if the delay is more than the one configured, delete the task
                if (timespanCompleted.TotalDays >= minDays)
                {
                    task.Delete();
                    count++;
                }
            }

            if (count > 0)
                LogService.Log("Workbook", string.Format("Auto-delete completed removed {0} tasks", count));

            return count;
        }

        public void ApplySmartViewOrder(IList<ISmartView> newOrder)
        {
            // make sure the list is valid: the content is the same but the order can be different
            if (this.smartViews.Count != newOrder.Count || this.smartViews.Any(sv => !newOrder.Contains(sv)))
                return;

            // check of the order has actually changed
            bool hasChanges = false;
            foreach (ISmartView smartView in this.smartViews)
            {
                if (smartView.Order != newOrder.IndexOf(smartView))
                {
                    hasChanges = true;
                    break;
                }
            }

            if (!hasChanges)
                return;

            foreach (var smartView in this.smartViews)
                smartView.Order = newOrder.IndexOf(smartView);

            this.databaseContext.SendChanges();

            // refreh the list of folder and sort by order
            this.smartViews = this.smartViews.OrderBy(f => f.Order).ToList();

            this.SmartViewsReordered.Raise(this, EventArgs.Empty); 
        }

        public void ApplyTagOrder(IList<ITag> newOrder)
        {
            // make sure the list is valid: the content is the same but the order can be different
            if (this.tags.Count != newOrder.Count || this.tags.Any(t => !newOrder.Contains(t)))
                return;

            // check of the order has actually changed
            bool hasChanges = false;
            foreach (ViewTag tag in this.tags)
            {
                if (tag.Order != newOrder.IndexOf(tag))
                {
                    hasChanges = true;
                    break;
                }
            }

            if (!hasChanges)
                return;

            foreach (var tag in this.tags)
                tag.Order = newOrder.IndexOf(tag);

            this.databaseContext.SendChanges();

            // refreh the list of folder and sort by order
            this.tags = this.tags.OrderBy(f => f.Order).ToList();

            this.TagsReordered.Raise(this, EventArgs.Empty);
        }

        public void ApplyFolderOrder(IList<IFolder> newOrder)
        {
            // make sure the list is valid: the content is the same but the order can be different
            if (this.Folders.Count != newOrder.Count || this.Folders.Any(f => !newOrder.Contains(f)))
                return;

            // check of the order has actually changed
            bool hasChanges = false;
            foreach (IFolder folder in this.folders)
            {
                if (folder.Order != newOrder.IndexOf(folder))
                {
                    hasChanges = true;
                    break;
                }
            }

            if (!hasChanges)
                return;

            foreach (var folder in this.Folders)
                folder.Order = newOrder.IndexOf(folder);

            this.databaseContext.SendChanges();

            // refreh the list of folder and sort by order
            this.folders = this.databaseContext.Folders
                .OfType<IFolder>()
                .OrderBy(f => f.Order)
                .ToList();

            this.FoldersReordered.Raise(this, EventArgs.Empty);
        }

        public void ApplyContextOrder(IList<IContext> newOrder)
        {
            // make sure the list is valid: the content is the same but the order can be different
            if (this.Contexts.Count != newOrder.Count || this.Contexts.Any(c => !newOrder.Contains(c)))
                return;

            // check of the order has actually changed
            bool hasChanges = false;
            foreach (var context in this.contexts)
            {
                if (context.Order != newOrder.IndexOf(context))
                {
                    hasChanges = true;
                    break;
                }
            }

            if (!hasChanges)
                return;

            foreach (var context in this.Contexts)
                context.Order = newOrder.IndexOf(context);

            this.databaseContext.SendChanges();

            // refreh the list of context and sort by order
            this.contexts = this.databaseContext.Contexts
                .OfType<IContext>()
                .OrderBy(f => f.Order)
                .ToList();

            this.ContextsReordered.Raise(this, EventArgs.Empty);
        }

        public void ApplyViewOrder(IList<ISystemView> newOrder)
        {
            // make sure the list is valid: the content is the same but the order can be different
            if (this.Views.Count() != newOrder.Count || this.Views.Any(p => !newOrder.Contains(p)))
                return;

            // check of the order has actually changed
            bool hasChanges = false;
            foreach (var view in this.views)
            {
                if (view.Order != newOrder.IndexOf(view))
                {
                    hasChanges = true;
                    break;
                }
            }

            if (!hasChanges)
                return;

            foreach (var view in this.Views)
                view.Order = newOrder.IndexOf(view);

            this.databaseContext.SendChanges();

            this.ViewsReordered.Raise(this, EventArgs.Empty);
        }

        public void SetupRemoteSyncFolder(IFolder folder)
        {
            folder.Color = this.DefaultFolderColor;
            folder.IconId = this.DefaultSyncIconId;
        }
        
        private void OnFolderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.FolderChanged.Raise(sender, e);
        }

        private void OnContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.ContextChanged.Raise(sender, e);
        }

        private void OnSmartViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.SmartViewChanged.Raise(sender, e);
        }

        private void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateTagsCollection((ITask)sender, ItemChange.Removed);

            this.TaskChanged.Raise(sender, e);
        }
        
        private void StartTrackingFolder(IFolder folder)
        {
            if (this.trackedFolders.Contains(folder))
                return;

            folder.TaskAdded += this.OnFolderTaskAdded;
            folder.TaskRemoved += this.OnFolderTaskRemoved;

            foreach (var task in folder.Tasks)
            {
                this.StartTrackingTask(task);
            }

            folder.PropertyChanged += this.OnFolderPropertyChanged;

            this.trackedFolders.Add(folder);
        }

        protected void StopTrackingFolder(IFolder folder)
        {
            folder.TaskAdded -= this.OnFolderTaskAdded;
            folder.TaskRemoved -= this.OnFolderTaskRemoved;

            foreach (var task in folder.Tasks)
            {
                this.StopTrackingTask(task);
            }

            folder.PropertyChanged -= this.OnFolderPropertyChanged;

            this.trackedFolders.Remove(folder);
        }

        private void StartTrackingContext(IContext context)
        {
            if (this.trackedContexts.Contains(context))
                return;

            context.PropertyChanged += this.OnContextPropertyChanged;

            this.trackedContexts.Add(context);
        }

        protected void StopTrackingContext(IContext context)
        {
            context.PropertyChanged -= this.OnContextPropertyChanged;

            this.trackedContexts.Remove(context);
        }

        private void StartTrackingSmartView(ISmartView smartview)
        {
            if (this.trackedSmartViews.Contains(smartview))
                return;

            smartview.PropertyChanged += this.OnSmartViewPropertyChanged;

            this.trackedSmartViews.Add(smartview);
        }

        protected void StopTrackingSmartView(ISmartView smartview)
        {
            smartview.PropertyChanged -= this.OnSmartViewPropertyChanged;

            this.trackedSmartViews.Remove(smartview);
        }

        private void OnFolderTaskRemoved(object sender, EventArgs<ITask> e)
        {
            if (!this.databaseContext.Tasks.Contains(e.Item) || this.IgnoreFolderChanges)
                return;

            this.TaskRemoved.Raise(this, e);
            this.StopTrackingTask(e.Item);

            this.databaseContext.RemoveTask(e.Item);
            this.databaseContext.SendChanges();

            this.tasks.Remove(e.Item);

            this.UpdateTagsCollection(e.Item, ItemChange.Removed);
        }

        private void OnFolderTaskAdded(object sender, EventArgs<ITask> e)
        {
            if (this.databaseContext.Tasks.Contains(e.Item) || this.IgnoreFolderChanges)
                return;

            // make sure the Modified property is set to a correct value otherwise
            // it causes an exception in the SQL database
            if (e.Item.Modified == DateTime.MinValue)
                e.Item.Modified = DateTime.Now;

            this.databaseContext.AddTask(e.Item);
            this.databaseContext.SendChanges();

            // warning: perform the insert in the DB before sending the TaskAdded event
            // because otherwise, the ID of the task is undefined
            this.TaskAdded.Raise(this, e);
            this.StartTrackingTask(e.Item);

            this.tasks.Add(e.Item);

            this.UpdateTagsCollection(e.Item, ItemChange.Added);
        }

        private void StartTrackingTask(ITask task)
        {
            task.PropertyChanged += this.OnTaskPropertyChanged;
        }

        protected void StopTrackingTask(ITask task)
        {
            task.PropertyChanged -= this.OnTaskPropertyChanged;
        }

        private void UpdateTagsCollection(ITask task, ItemChange change)
        {
            // task added: check if its tags must be added to the collection
            if ((change == ItemChange.Added || change == ItemChange.Changed) && task.HasTags)
            {
                foreach (var tag in task.Tags.Split(Constants.TagSeparator))
                {
                    if (!this.tags.Any(t => t.Name.Equals(tag.Trim(), StringComparison.OrdinalIgnoreCase)))
                        this.AddTag(tag);
                }
            }
            
            // task changed/removed: check if tags are still in other tasks
            if (change == ItemChange.Changed || change == ItemChange.Removed)
            {
                var newTags = this.GetTagsFromTasks();

                if (this.settings.GetValue<bool>(CoreSettings.AutoDeleteTags))
                {
                    foreach (var tag in this.tags.ToList())
                    {
                        if (!newTags.Contains(tag.Name))
                            this.RemoveTagCore(tag.Name);
                    }
                }

                foreach (var newTag in newTags)
                {
                    if (!this.tags.Any(t => t.Name.Equals(newTag, StringComparison.OrdinalIgnoreCase)))
                        this.AddTag(newTag);
                }
            }
        }

        public void AddTag(string tagName)
        {
            var tag = this.InstanciateTag(tagName);
            tag.GroupAscending = true;
            tag.TaskGroup = TaskGroup.DueDate;

            var viewTag = new ViewTag(this, tag);

            this.databaseContext.AddTag(tag);
            this.tags.Add(viewTag);

            this.TagAdded.Raise(this, new EventArgs<ITag>(viewTag));
        }

        internal void RemoveTagCore(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return;

            var target = this.tags.FirstOrDefault(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                this.databaseContext.RemoveTag((ITag)target.Owner);
                this.tags.Remove(target);

                this.TagRemoved.Raise(this, new EventArgs<ITag>(target));
            }
        }

        public void RemoveTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return;

            foreach (var task in this.tasks.Where(t => !string.IsNullOrWhiteSpace(t.Tags)))
            {
                var newTags = new List<string>();
                foreach (string tag in task.Tags.Split(Constants.TagSeparator))
                {
                    if (!tag.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                        newTags.Add(tag);
                }
                task.WriteTags(newTags);
            }

            this.RemoveTagCore(tagName);
        }

        private enum ItemChange
        {
            Added,
            Removed,
            Changed
        }
    }
}

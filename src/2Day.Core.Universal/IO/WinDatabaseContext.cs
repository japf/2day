using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.UI.Xaml;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Manager;
using SQLite.Net;
using SQLite.Net.Interop;
using SQLite.Net.Platform.WinRT;
using Task = Chartreuse.Today.Core.Shared.Model.Impl.Task;

namespace Chartreuse.Today.Core.Universal.IO
{
    public class WinDatabaseContext : IDatabaseContext
    {
        private readonly ObjectTracker objectTracker;
        private readonly string dbFileName;

        private SQLiteConnection connection;
        private readonly string fullPathFile;

        private List<IFolder> folders;
        private List<ISystemView> views;
        private List<ISmartView> smartViews;
        private List<IContext> contexts;
        private List<ITask> tasks;
        private List<ITag> tags;

        private bool preventDuplicates;

        public IEnumerable<ISystemView> Views { get { return this.views; } }
        public IEnumerable<ISmartView> SmartViews { get { return this.smartViews; } }
        public IEnumerable<IContext> Contexts { get { return this.contexts; } }
        public IEnumerable<IFolder> Folders { get { return this.folders; } }
        public IEnumerable<ITask> Tasks { get { return this.tasks; } }
        public IEnumerable<ITag> Tags { get { return this.tags; } }

        public string Filename
        {
            get { return this.dbFileName; }
        }

        public string FullPathFile
        {
            get { return this.fullPathFile; }
        }

        public WinDatabaseContext(string dbFileName, bool automaticSave)
        {
            if (string.IsNullOrEmpty(dbFileName))
                throw new ArgumentNullException(nameof(dbFileName));

            this.dbFileName = dbFileName;

            this.fullPathFile = Path.Combine(ApplicationData.Current.LocalFolder.Path, this.dbFileName);
            this.connection = CreateSQLiteConnection(this.fullPathFile);

            this.objectTracker = new ObjectTracker(this.connection, automaticSave);
        }

        public void AddSystemView(ISystemView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var target = this.views.FirstOrDefault(t => t.ViewKind == view.ViewKind);
            if (target != null)
                return;

            this.objectTracker.EnsureIsTracked(view);
            this.connection.Insert(view);

            this.views.Add(view);
        }

        public void RemoveSystemView(ISystemView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            this.Delete(view);
            this.views.Remove(view);
        }

        public void AddSmartView(ISmartView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var target = this.smartViews.FirstOrDefault(t => t.Id == view.Id);
            if (target != null)
                return;

            this.objectTracker.EnsureIsTracked(view);

            if (this.preventDuplicates)
                target = this.connection.Table<SmartView>().FirstOrDefault(i => i.Id == view.Id);

            if (target == null)
                this.connection.Insert(view);

            this.smartViews.Add(view);
        }

        public void RemoveSmartView(ISmartView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            this.Delete(view);
            this.smartViews.Remove(view);
        }

        public void AddFolder(IFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            var target = this.folders.FirstOrDefault(i => i.Id == folder.Id);
            if (target != null)
                return;

            this.objectTracker.EnsureIsTracked(folder);

            if (this.preventDuplicates)
                target = this.connection.Table<Folder>().FirstOrDefault(i => i.Id == folder.Id);

            if (target == null)
                this.connection.Insert(folder);

            this.folders.Add(folder);
        }

        public void RemoveFolder(IFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            this.Delete(folder);
            this.folders.Remove(folder);
        }

        public void AddTask(ITask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var target = this.tasks.FirstOrDefault(i => i.Id == task.Id);
            if (target != null)
                return;

            this.objectTracker.EnsureIsTracked(task);

            if (this.preventDuplicates)
                target = this.connection.Table<Task>().FirstOrDefault(i => i.Id == task.Id);

            if (target == null)
                this.connection.Insert(task);

            this.tasks.Add(task);
        }

        public void RemoveTask(ITask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            this.Delete(task);
            this.tasks.Remove(task);
        }

        public void AddContext(IContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var target = this.contexts.FirstOrDefault(t => t.Id == context.Id);
            if (target != null)
                return;

            this.objectTracker.EnsureIsTracked(context);

            if (this.preventDuplicates)
                target = this.connection.Table<Context>().FirstOrDefault(i => i.Id == context.Id);

            if (target == null)
                this.connection.Insert(context);

            this.contexts.Add(context);
        }

        public void RemoveContext(IContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            this.Delete(context);
            this.contexts.Remove(context);
        }

        public void AddTag(ITag tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            var target = this.tags.FirstOrDefault(t => t.Id == tag.Id);
            if (target != null)
                return;

            this.objectTracker.EnsureIsTracked(tag);

            if (this.preventDuplicates)
                target = this.connection.Table<Tag>().FirstOrDefault(i => i.Id == tag.Id);

            if (target == null)
                this.connection.Insert(tag);

            this.tags.Add(tag);
        }

        public void RemoveTag(ITag tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            this.Delete(tag);
            this.tags.Remove(tag);
        }

        public void SendChanges()
        {
            this.objectTracker.SaveInDatabaseUnsavedEditedItems();
        }

        public IDisposable WithTransaction()
        {
            // nested transactions are not supported with SQLite
            if (this.connection.IsInTransaction)
                return new DisposableAction(() => { });

            this.connection.BeginTransaction();
            return new DisposableAction(this.connection.Commit);
        }

        public IDisposable WithDuplicateProtection()
        {
            this.preventDuplicates = true;
            return new DisposableAction(() => this.preventDuplicates = false);
        }

        public bool DatabaseExists()
        {
            return this.connection.GetTableInfo("Contexts").Any();
        }

        public void InitializeDatabase()
        {
            if (!this.DatabaseExists())
            {
                this.connection.CreateTable<Context>();
                this.connection.CreateTable<Task>();
                this.connection.CreateTable<Folder>();
                this.connection.CreateTable<SystemView>();
                this.connection.CreateTable<SmartView>();
                this.connection.CreateTable<Tag>();

                this.folders = new List<IFolder>();
                this.contexts = new List<IContext>();
                this.tasks = new List<ITask>();
                this.views = new List<ISystemView>();
                this.smartViews = new List<ISmartView>();
                this.tags = new List<ITag>();
            }
            else
            {
                try
                {
                    this.folders = new List<IFolder>(this.connection.Table<Folder>());
                }
                catch (FormatException)
                {
                    DeviceFamily deviceFamily = DeviceFamily.Unkown;
                    if (Ioc.HasType<IPlatformService>())
                        deviceFamily = Ioc.Resolve<IPlatformService>().DeviceFamily;

                    var trackingManager = new TrackingManager(true, deviceFamily);
                    DatebaseDateTimeFixer.Run(trackingManager, this.connection);

                    this.folders = new List<IFolder>(this.connection.Table<Folder>());
                    trackingManager.Event(TrackingSource.Storage, "FormatException", "Fixed");
                }
                
                this.contexts = new List<IContext>(this.connection.Table<Context>());

                // check if the Task table contains the ParentId column
                if (!this.connection.GetTableInfo("Tasks").Any(c => c.Name == "ParentId"))
                    this.connection.MigrateTable<Task>();

                this.tasks = new List<ITask>(this.connection.Table<Task>());

                // check duplicate sync id
                var syncIds = new HashSet<string>();
                foreach (var task in this.tasks.Where(t => t.SyncId != null).ToList())
                {
                    if (!syncIds.Contains(task.SyncId))
                    {
                        syncIds.Add(task.SyncId);
                    }
                    else
                    {
                        LogService.Log("Workbook", $"Removing duplicate task {task.Title}");
                        this.tasks.Remove(task);
                        this.connection.Delete(task);
                    }
                }

                this.views = new List<ISystemView>();
                bool hasBeganDeleteTransaction = false;
                foreach (var view in this.connection.Table<SystemView>().ToList())
                {
                    // make sure we don't add twice the same view kind in the list
                    // this should never happens, but... it already did for a single user !
                    if (this.views.FirstOrDefault(v => v.ViewKind == view.ViewKind) == null)
                    {
                        this.views.Add(view);
                    }
                    else
                    {
                        if (!hasBeganDeleteTransaction)
                        {
                            this.connection.BeginTransaction();
                            hasBeganDeleteTransaction = true;
                        }

                        this.connection.Delete(view);
                    }
                }

                if (hasBeganDeleteTransaction)
                    this.connection.Commit();

                if (!this.connection.GetTableInfo("SmartViews").Any())
                    this.connection.CreateTable<SmartView>();

                this.smartViews = new List<ISmartView>();
                foreach (var view in this.connection.Table<SmartView>())
                    this.smartViews.Add(view);

                if (!this.connection.GetTableInfo("Tags").Any())
                {
                    // the Tag column doesn't exists yet
                    this.connection.CreateTable<Tag>();

                    var tags = this.tasks
                        .SelectMany(t => t.Tags != null ? t.Tags.Split(Constants.TagSeparator) : Enumerable.Empty<string>())
                        .Distinct()
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .OrderBy(t => t)
                        .ToList();

                    foreach (var tag in tags)
                        this.AddTag(new Tag
                        {
                            Name = tag,
                            TaskGroup = TaskGroup.DueDate,
                            GroupAscending = true,
                        });
                }
                
                
                this.tags = new List<ITag>(this.connection.Table<Tag>());

                foreach (var task in this.tasks.OfType<Task>())
                {
                    if (task.ContextId >= 0)
                        task.Context = this.contexts.SingleOrDefault(c => c.Id == task.ContextId);
                    if (task.FolderId >= 0)
                        task.Folder = this.folders.SingleOrDefault(f => f.Id == task.FolderId);
                } 
            }
            
            foreach (var folder in this.folders)
                this.objectTracker.EnsureIsTracked(folder);
            foreach (var context in this.contexts)
                this.objectTracker.EnsureIsTracked(context);
            foreach (var task in this.tasks)
                this.objectTracker.EnsureIsTracked(task);
            foreach (var view in this.views)
                this.objectTracker.EnsureIsTracked(view);
            foreach (var smartview in this.smartViews)
                this.objectTracker.EnsureIsTracked(smartview);
            foreach (var tag in this.tags)
                this.objectTracker.EnsureIsTracked(tag);
        }

        public void CloseConnection()
        {
            this.connection.Close();
            this.connection.Dispose();
        }

        public void OpenConnection()
        {
            this.connection = CreateSQLiteConnection(this.FullPathFile);
        }

        private void Delete(INotifyPropertyChanged item)
        {
            this.connection.Delete(item);
            this.objectTracker.Untrack(item);
        }

        private static SQLiteConnection CreateSQLiteConnection(string path)
        {
            return new SQLiteConnection(new SQLitePlatformWinRT(), path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create, false);
        }

        private class ObjectTracker
        {
            private const int IntervalForAutomaticSaveSecond = 3;
            private readonly HashSet<INotifyPropertyChanged> items;
            private readonly HashSet<object> unsavedEditedItems;
            private readonly SQLiteConnection connection;
            private readonly DispatcherTimer timer;

            public ObjectTracker(SQLiteConnection connection, bool automaticSave)
            {
                this.connection = connection;

                this.items = new HashSet<INotifyPropertyChanged>();
                this.unsavedEditedItems = new HashSet<object>();

                if (automaticSave)
                {
                    this.timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(IntervalForAutomaticSaveSecond)};
                    this.timer.Tick += this.OnTimerTick;
                }
            }

            public void EnsureIsTracked(INotifyPropertyChanged item)
            {
                if (!this.items.Contains(item))
                {
                    this.items.Add(item);
                    item.PropertyChanged += this.OnItemPropertyChanged;
                }
            }

            public void Untrack(INotifyPropertyChanged item)
            {
                if (this.items.Contains(item))
                {
                    this.items.Remove(item);
                    item.PropertyChanged -= this.OnItemPropertyChanged;
                }                
            }

            private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                lock (this.unsavedEditedItems)
                {
                    if (!this.unsavedEditedItems.Contains(sender))
                    {
                        this.unsavedEditedItems.Add(sender);

                        if (this.timer != null)
                        {
                            if (this.timer.IsEnabled)
                            {
                                this.timer.Stop();
                                this.timer.Start();
                            }
                            else
                            {
                                this.timer.Start();
                            }
                        }
                    }
                }
            }

            private void OnTimerTick(object sender, object e)
            {
                this.SaveInDatabaseUnsavedEditedItems();
            }

            public void SaveInDatabaseUnsavedEditedItems()
            {
                lock (this.unsavedEditedItems)
                {
                    foreach (object pendingItem in this.unsavedEditedItems)
                    {
                        this.connection.Update(pendingItem);
                    }
                    this.unsavedEditedItems.Clear();

                    if (this.timer != null)
                        this.timer.Stop();
                }
            }
        }
    }

    public static class DatebaseDateTimeFixer
    {
        private static readonly Regex regex = new Regex(@"T(\d\d.\d\d.\d\d)");

        public static void Run(TrackingManager trackingManager, SQLiteConnection connection)
        {
            foreach (var table in new List<string> {"folder", "tasks"})
            {
                try
                {
                    var databaseEntries = connection.Query<DatabaseEntry>("SELECT * FROM " + table);
                    foreach (var databaseEntry in databaseEntries)
                    {
                        string update = string.Empty;

                        if (!string.IsNullOrWhiteSpace(databaseEntry.Added))
                            update += string.Format(" Added = '{0}',", NormalizeDatetime(databaseEntry.Added));

                        if (!string.IsNullOrWhiteSpace(databaseEntry.Alarm))
                            update += string.Format(" Alarm = '{0}',", NormalizeDatetime(databaseEntry.Alarm));

                        if (!string.IsNullOrWhiteSpace(databaseEntry.Modified))
                            update += string.Format(" Modified = '{0}',", NormalizeDatetime(databaseEntry.Modified));

                        if (!string.IsNullOrWhiteSpace(databaseEntry.Due))
                            update += string.Format(" Due = '{0}',", NormalizeDatetime(databaseEntry.Due));

                        if (!string.IsNullOrWhiteSpace(databaseEntry.Start))
                            update += string.Format(" Start = '{0}',", NormalizeDatetime(databaseEntry.Start));

                        if (!string.IsNullOrWhiteSpace(databaseEntry.Completed))
                            update += string.Format(" Completed = '{0}',", NormalizeDatetime(databaseEntry.Completed));

                        update = update.TrimEnd(',');

                        try
                        {
                            connection.Execute(string.Format("UPDATE {0} SET {1} WHERE Id = {2}", table, update, databaseEntry.Id));
                        }
                        catch (Exception e1)
                        {
                            trackingManager.Exception(e1, "DatebaseDateTimeFixer update query");
                        }
                    }
                }
                catch (Exception e2)
                {
                    trackingManager.Exception(e2, "DatebaseDateTimeFixer select query");
                }
            }
        }

        private static string NormalizeDatetime(string input)
        {
            var match = regex.Match(input);
            if (match.Success && match.Groups.Count == 2)
            {
                string oldValue = match.Groups[1].Value;
                string newValue = oldValue.Replace('.', ':');
                return input.Replace(oldValue, newValue);
            }

            return input;
        }

        private class DatabaseEntry
        {
            public int Id { get; set; }
            public string Modified { get; set; }
            public string Added { get; set; }
            public string Completed { get; set; }
            public string Due { get; set; }
            public string Start { get; set; }
            public string Alarm { get; set; }
        }
    }
}

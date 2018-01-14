using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Chartreuse.Today.Core.Shared.Model
{
    public interface IWorkbook : IModelEntity
    {
        event EventHandler<EventArgs> ViewsReordered;

        event EventHandler<EventArgs<IFolder>> FolderRemoved;
        event EventHandler<EventArgs<IFolder>> FolderAdded;
        event EventHandler<EventArgs> FoldersReordered;
        event EventHandler<PropertyChangedEventArgs> FolderChanged;

        event EventHandler<EventArgs<ISmartView>> SmartViewRemoved;
        event EventHandler<EventArgs<ISmartView>> SmartViewAdded;
        event EventHandler<EventArgs> SmartViewsReordered;
        event EventHandler<PropertyChangedEventArgs> SmartViewChanged; 
        
        event EventHandler<EventArgs<IContext>> ContextRemoved;
        event EventHandler<EventArgs<IContext>> ContextAdded;
        event EventHandler<PropertyChangedEventArgs> ContextChanged;
        event EventHandler<EventArgs> ContextsReordered;

        event EventHandler<EventArgs<ITask>> TaskRemoved;
        event EventHandler<EventArgs<ITask>> TaskAdded;
        event EventHandler<PropertyChangedEventArgs> TaskChanged;

        event EventHandler<EventArgs<ITag>> TagRemoved;
        event EventHandler<EventArgs<ITag>> TagAdded;
        event EventHandler<EventArgs> TagsReordered;

        IList<IFolder> Folders { get; }
        IList<IContext> Contexts { get; }
        IEnumerable<ISystemView> Views { get; }
        IList<ISmartView> SmartViews { get; }
        IEnumerable<ITag> Tags { get; }
        IList<ITask> Tasks { get; }

        bool IgnoreFolderChanges { get; set; }

        ISettings Settings { get; }

        IFolder this[int folderId] { get; }

        void AddView(ISystemView view);

        ISmartView AddSmartView(string name, string rules, int? id = null);
        bool RemoveSmartView(string name);

        IContext AddContext(string name, string syncId = null, int? id = null);
        bool RemoveContext(string name);

        IFolder AddFolder(string name, string syncId = null, int? id = null);
        bool RemoveFolder(string name);

        void RemoveTag(string name);

        void RemoveAll();
        void RemoveCompletedTasks();
        void RemoveAllTasks();
        void CompleteTasks();

        void Initialize();

        /// <summary>
        /// Submit the pending edited changes to the underlying database.
        /// </summary>
        void CommitEditedChanges();

        /// <summary>
        /// Begin a transaction on the underlying database
        /// </summary>
        /// <returns>An object that commit the transaction when it gets disposed</returns>
        IDisposable WithTransaction();

        IDisposable WithDuplicateProtection();

        int RemoveOldTasks();

        void ApplySmartViewOrder(IList<ISmartView> newOrder);
        void ApplyFolderOrder(IList<IFolder> newOrder);
        void ApplyContextOrder(IList<IContext> newOrder);
        void ApplyViewOrder(IList<ISystemView> newOrder);
        void ApplyTagOrder(IList<ITag> newOrder);

        ITask CreateTask(int? id = null);
        void SetupRemoteSyncFolder(IFolder folder);
        
        void LoadViews();
    }
}

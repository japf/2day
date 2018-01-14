using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Shared.Sync
{
    public interface ISynchronizationMetadata
    {
        DateTime LastSync { get; }
        SynchronizationService ActiveProvider { get; set; }
        Dictionary<string, string> ProviderDatas { get; }

        bool HasChanges { get; }

        List<string> Backups { get; }

        List<int> AddedFolders { get; }
        List<DeletedEntry> DeletedFolders { get; }
        List<int> EditedFolders { get; }

        List<int> AddedContexts { get; }
        List<DeletedEntry> DeletedContexts { get; }
        List<int> EditedContexts { get; }

        List<int> AddedSmartViews { get; }
        List<DeletedEntry> DeletedSmartViews { get; }
        List<int> EditedSmartViews { get; }

        List<int> AddedTasks { get; }
        List<DeletedEntry> DeletedTasks { get; }
        Dictionary<int, TaskProperties> EditedTasks { get; }

        List<int> AfterSyncEditedTasks { get; set; }
        List<string> AfterSyncEditedFolders { get; set; }
        List<string> AfterSyncEditedContexts { get; set; }
        List<string> AfterSyncEditedSmartViews { get; set; }
        
        void Reset();
        void IgnoreTask(ITask task);
        void IgnoreFolder(string name);
        void IgnoreContext(string name);
        void IgnoreSmartView(string smartview);

        void FolderAdded(IFolder folder);
        void FolderEdited(IFolder folder);
        void FolderRemoved(IFolder folder);

        void ContextAdded(IContext context);
        void ContextEdited(IContext context);
        void ContextRemoved(IContext context);

        void SmartViewAdded(ISmartView smartview);
        void SmartViewEdited(ISmartView smartview);
        void SmartViewRemoved(ISmartView smartview);

        void TaskAdded(ITask task);
        void TaskEdited(ITask task, string propertyName);
        void TaskRemoved(ITask task);
    }
}
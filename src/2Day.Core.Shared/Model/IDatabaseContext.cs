using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Model
{
    public interface IDatabaseContext
    {
        string Filename { get; }
        string FullPathFile { get;  }

        IEnumerable<ISystemView> Views { get; }
        void AddSystemView(ISystemView view);
        void RemoveSystemView(ISystemView view);

        IEnumerable<ISmartView> SmartViews { get; }
        void AddSmartView(ISmartView view);
        void RemoveSmartView(ISmartView view);

        IEnumerable<IFolder> Folders { get; }
        void AddFolder(IFolder folder);
        void RemoveFolder(IFolder folder);

        IEnumerable<IContext> Contexts { get; }
        void AddContext(IContext context);
        void RemoveContext(IContext context);

        IEnumerable<ITag> Tags { get; }
        void AddTag(ITag tag);
        void RemoveTag(ITag tag);

        IEnumerable<ITask> Tasks { get; }
        void AddTask(ITask task);
        void RemoveTask(ITask task);

        void SendChanges();

        IDisposable WithTransaction();

        IDisposable WithDuplicateProtection();
    }
}
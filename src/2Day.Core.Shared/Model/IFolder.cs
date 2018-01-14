using System;

namespace Chartreuse.Today.Core.Shared.Model
{
    public interface IFolder : IAbstractFolder
    {
        #pragma warning disable 108, 114
        // for x:Binding compatibility
        string Name { get; set; }
        #pragma warning restore 108,114

        new string Color { get; set; }
        new int IconId { get; set; }

        string SyncId { get; set; }
        DateTime Modified { get; }

        bool? ShowInViews { get; set; }

        bool RemoveTask(ITask task);
    }
}

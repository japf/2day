using System;

namespace Chartreuse.Today.Core.Shared.Sync
{
    public interface ISyncTask : ISyncItem
    {
        DateTime? Due { get; }
    }
}

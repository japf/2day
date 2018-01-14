namespace Chartreuse.Today.Core.Shared.Sync
{
    public enum SyncAdvancedMode
    {
        // remove all content from 2Day, then sync
        Replace,
        // get all content form remove service, then merge
        Merge
    }
}

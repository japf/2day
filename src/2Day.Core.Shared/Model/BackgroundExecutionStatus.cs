namespace Chartreuse.Today.Core.Shared.Model
{
    public enum BackgroundExecutionStatus
    {
        None,
        Started,
        Skipped,
        WorkbookLoaded,
        WorkbookNotLoaded,
        SyncStarted,
        SyncCompleted,
        SyncError,
        TileStarted,
        TileCompleted,
        CompletedSuccess,
        CompletedException,
        CompletedRootException,
        Cancelled
    }
}

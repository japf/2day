namespace Chartreuse.Today.Core.Shared.Model
{
    public interface ISmartView : IView
    {
        string SyncId { get; set; }

        string Rules { get; set; }

        bool ShowCompletedTasks { get; }
    }
}

using Windows.ApplicationModel.Background;
using Chartreuse.Today.App.Background;

namespace Chartreuse.Today.App.BackgroundService
{
    public sealed class TimerBackgroundService : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var backgroundTaskHelper = new BackgroundTaskHelper();
            backgroundTaskHelper.Run(taskInstance);
        }
    }
}

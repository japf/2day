using System;
using System.Threading.Tasks;
using Chartreuse.Today.App.Manager;

namespace Chartreuse.Today.App.Test.Impl
{
    public class TestBackgroundTaskManager : IBackgroundTaskManager
    {
        public event EventHandler BackgroundSyncStarting;
        public event EventHandler BackgroundSyncCompleted;
        public Task RegisterBackgroundTaskAsync()
        {
            return null;
        }
    }
}
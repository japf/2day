using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Chartreuse.Today.App.Test
{
    public static class UITestHelper
    {
        public static void ExecuteOnUIThread(Action action)
        {
            CoreApplication.MainView.Dispatcher
                .RunAsync(CoreDispatcherPriority.Normal, () => { action(); })
                .AsTask()
                .GetAwaiter()
                .GetResult();
        }
    }
}
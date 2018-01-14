using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Chartreuse.Today.App.Tools
{
    public static class WaitHandleHelper
    {
        public static void RegisterForSignal(string waitHandleName, Action signalReceived)
        {
            if (waitHandleName == null)
                throw new ArgumentNullException(nameof(waitHandleName));

            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, waitHandleName);

            var dispatcher = Window.Current.Dispatcher;

            // run a task forever that will block unil the wait handle send a signal
            Task.Run(() =>
            {
                while (true)
                {
                    waitHandle.WaitOne();

                    // at this point, the workbook has been updated on disk and we must reload it to fetch latest changes
                    dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => signalReceived());
                }
            });
        }
    }
}

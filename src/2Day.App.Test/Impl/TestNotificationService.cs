using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Services;

namespace Chartreuse.Today.App.Test.Impl
{
    public class TestNotificationService : INotificationService
    {
        public void StartAsyncOperation()
        {
        }

        public Task EndAsyncOperationAsync(string message = null)
        {
            return null;
        }

        public void ReportProgressAsyncOperation(string message)
        {
        }

        public Task ShowNotification(string message, ToastType type = ToastType.Info, Action cancelled = null)
        {
            return null;
        }

        public void ShowNativeToast(string title, string message, string argument)
        {
        }
    }
}
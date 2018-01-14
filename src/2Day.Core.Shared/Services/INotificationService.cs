using System;
using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Services
{
    public interface INotificationService
    {
        void StartAsyncOperation();

        Task EndAsyncOperationAsync(string message = null);

        void ReportProgressAsyncOperation(string message);

        Task ShowNotification(string message, ToastType type = ToastType.Info, Action cancelled = null);

        void ShowNativeToast(string title, string message, string argument);
    }
}
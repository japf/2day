using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Controls;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Universal.Notifications;

namespace Chartreuse.Today.App.Manager.UI
{
    public class NotificationService : INotificationService
    {
        private const string IconCheckCode = "\uE10B";
        private const string IconSearchCode = "\uE11A";

        private const int DelayBeforeNormalCloseMs = 3000;
        private const int DelayBeforeWarningCloseMs = 4000;

        private ToastIndicator toastIndicator;
        private Action cancelledAction;

        private int notificationCount;

        public void StartAsyncOperation()
        {
            this.toastIndicator = GetIndicator();
            if (this.toastIndicator != null)
            {
                this.notificationCount++;

                if (this.notificationCount > 1)
                    return;

                if (this.toastIndicator.IsAnimationRunning)
                {
                    this.toastIndicator.StopAnimation();
                }

                this.toastIndicator.IsAnimated = true;
                this.toastIndicator.Show(ToastType.Default);
            }
        }

        public async Task EndAsyncOperationAsync(string message = null)
        {
            if (this.notificationCount < 1)
                return;

            if (this.toastIndicator != null)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    this.toastIndicator.IsAnimated = false;
                    this.toastIndicator.Description = message;
                    this.toastIndicator.ToastType = ToastType.Default;
                }

                await Task.Delay(DelayBeforeNormalCloseMs);

                this.notificationCount--;

                if (this.toastIndicator != null && this.notificationCount == 0)
                {
                    this.toastIndicator.Hide();
                    this.toastIndicator = null;
                }
            }
        }

        public void ReportProgressAsyncOperation(string message)
        {
            if (this.notificationCount < 1)
                return;

            if (this.toastIndicator != null)
            {
                this.toastIndicator.IsAnimated = true;
                this.toastIndicator.Description = message;
                this.toastIndicator.ToastType = ToastType.Default;
            }
        }

        public async Task ShowNotification(string message, ToastType type = ToastType.Info, Action cancelled = null)
        {
            var indicator = GetIndicator();
            if (indicator != null)
            {
                this.notificationCount++;

                bool canCancel = cancelled != null;

                if (type == ToastType.Search)
                    indicator.IconCode = IconSearchCode;
                else
                    indicator.IconCode = IconCheckCode;

                indicator.IsAnimated = false;
                indicator.Description = message;
                indicator.Show(type, canCancel);

                if (canCancel)
                {
                    indicator.Tapped += this.OnIndicatorTapped;

                    this.cancelledAction = cancelled;
                }

                int delayCloseMs = DelayBeforeNormalCloseMs;
                if (type == ToastType.Warning)
                    delayCloseMs = DelayBeforeWarningCloseMs;

                await Task.Delay(delayCloseMs);
                
                this.notificationCount--;

                if (this.notificationCount == 0)
                    indicator.Hide();
            }
        }

        public void ShowNativeToast(string title, string message, string argument)
        {
            ToastHelper.ToastMessage(title, message, argument);
        }

        private void OnIndicatorTapped(object sender, TappedRoutedEventArgs e)
        {
            ToastIndicator indicator = (ToastIndicator) sender;

            if (this.cancelledAction != null)
            {
                this.cancelledAction();
                this.cancelledAction = null;
            }

            indicator.Tapped -= this.OnIndicatorTapped;
            indicator.Hide();
        }

        private static ToastIndicator GetIndicator()
        {
            ToastIndicator indicator = null;
            var frame = Window.Current.Content as Frame;
            if (frame != null && frame.Content is Page)
            {
                var page = (Page)frame.Content;
                var indicators = TreeHelper.FindVisualChildren<ToastIndicator>(page);

                indicator = indicators.FirstOrDefault(i => i.IsEnabled && i.Visibility == Visibility.Visible);
            }
            
            return indicator;
        }
    }
}

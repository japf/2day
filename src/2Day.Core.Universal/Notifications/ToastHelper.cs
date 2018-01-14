using System;
using Windows.UI.Notifications;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Manager;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.Core.Universal.Notifications
{
    public static class ToastHelper
    {
        public static void ToastMessage(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            ToastMessage(Constants.AppName, message);
        }

        public static void ToastMessage(string title, string message, string launchArgument = null)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                var xmlDoc = NotificationContentBuilder.CreateSimpleToastNotification(title, message, launchArgument);
                if (xmlDoc != null)
                {
                    ToastNotification notification = new ToastNotification(xmlDoc);
                    ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();

                    toastNotifier.Show(notification);
                }
            }
            catch (Exception ex)
            {
                // we might have an exception if the message is too long
                TrackingManagerHelper.Exception(ex, string.Format("ToastMessage exception: {0}", ex.Message));
            }
        }
    }
}

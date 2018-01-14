using System;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls.Primitives;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Tools
{
    public static class ResponsiveHelper
    {
        public const double MinWidth = 650;

        public static bool IsUsingSmallLayout()
        {
            try
            {
                // currentView can be null when 2Day is launched via a third party app with share contract activation (ex: share from Edge)
                ApplicationView currentView = ApplicationView.GetForCurrentView();
                if (currentView != null)
                    return currentView.VisibleBounds.Width <= MinWidth;
                else
                    return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static FlyoutPlacementMode GetPopupPlacement()
        {
            bool isUsingSmallLayout = false;
            try
            {
                isUsingSmallLayout = IsUsingSmallLayout();
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "GetPopupPlacement");
            }

            if (isUsingSmallLayout)
                return FlyoutPlacementMode.Full;
            else
                return FlyoutPlacementMode.Bottom;
        }

        public static bool IsInContinuum()
        {
            try
            {
                UIViewSettings currentView = UIViewSettings.GetForCurrentView();
                if (currentView.UserInteractionMode == UserInteractionMode.Mouse && AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile", StringComparison.OrdinalIgnoreCase))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
    }
}

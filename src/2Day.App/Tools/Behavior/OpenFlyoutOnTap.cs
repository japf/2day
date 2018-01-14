using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Controls;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Tools.Behavior
{
    public static class OpenFlyoutOnTap
    {
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(OpenFlyoutOnTap),
            new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;
            frameworkElement.Tapped += OnTapped;
        }

        private static void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                FrameworkElement frameworkElement = (FrameworkElement)sender;
                FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(frameworkElement);
                if (flyoutBase != null)
                {
                    Flyout flyout = flyoutBase as Flyout;
                    if (flyout != null)
                    {
                        flyout.Placement = ResponsiveHelper.GetPopupPlacement();
                        if (flyout.Content is IFlyoutContent)
                            ((IFlyoutContent)flyout.Content).HandleFlyout(flyout);
                    }

                    flyoutBase.ShowAt(frameworkElement);
                }
            }
            catch (Exception ex)
            {
                var messageBoxService = Ioc.Resolve<IMessageBoxService>();
                messageBoxService.ShowAsync(
                    StringResources.Message_Information, 
                    "It looks like 2Day is having trouble here - that seems due to the latest Windows 10 Mobile insiders build. You can change due date by sliding left a task in the task list for now :-)");

                TrackingManagerHelper.Exception(ex, "OpenFlyoutOnTap.OnTapped");
                throw;
            }

        }
    }
}

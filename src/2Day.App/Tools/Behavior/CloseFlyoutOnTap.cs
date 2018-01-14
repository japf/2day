using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Tools.Behavior
{
    public static class CloseFlyoutOnTap
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
            typeof(CloseFlyoutOnTap),
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
                var popup = TreeHelper.FindParent<Popup>(frameworkElement);
                if (popup != null)
                    popup.IsOpen = false;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "CloseFlyoutOnTap.OnTapped");
            }

        }
    }
}
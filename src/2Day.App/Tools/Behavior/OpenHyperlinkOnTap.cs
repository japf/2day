using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Tools.Behavior
{
    public static class OpenHyperlinkOnTap
    {
        public static string GetTarget(DependencyObject obj)
        {
            return (string)obj.GetValue(TargetProperty);
        }

        public static void SetTarget(DependencyObject obj, string value)
        {
            obj.SetValue(TargetProperty, value);
        }

        public static readonly DependencyProperty TargetProperty = DependencyProperty.RegisterAttached(
            "Target", 
            typeof(string), 
            typeof(OpenHyperlinkOnTap), 
            new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uiElement = d as FrameworkElement;
            if (uiElement != null)
                uiElement.Tapped += OnElementTapped;
        }

        private static async void OnElementTapped(object sender, TappedRoutedEventArgs e)
        {
            string target = GetTarget((DependencyObject)sender);
            try
            {
                if (!string.IsNullOrEmpty(target))
                    await Launcher.LaunchUriAsync(SafeUri.Get(target));
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Exception OpenHyperlinkOnTap.OnElementTapped");
            }
        }
    }
}

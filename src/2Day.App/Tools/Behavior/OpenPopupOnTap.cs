using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Tools.UI;

namespace Chartreuse.Today.App.Tools.Behavior
{
    public static class OpenPopupOnTap
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
            typeof(OpenPopupOnTap),
            new PropertyMetadata(false, PropertyChangedCallback));

        private static Popup currentPopup;

        private static void PropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;
            frameworkElement.Tapped += OnTapped;
        }

        private static void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement frameworkElement = (FrameworkElement)sender;
            var parent = frameworkElement.Parent as Panel;
            if (parent != null)
            {
                int senderIndex = parent.Children.IndexOf(frameworkElement);
                int nextIndex = senderIndex + 1;
                if (nextIndex < parent.Children.Count && parent.Children[nextIndex] is Popup)
                {
                    Popup popup = (Popup) parent.Children[nextIndex];
                    currentPopup = popup;

                    // in the GridViewActionBar control we set custom vertical/horizontal offset, so we don't want to reposition
                    // popup in that case - to distinguish this case, we setup a custom Tag property. So if it's null here, we
                    // assume we need to reposition the popup                
                    if (currentPopup.Tag == null)
                    {
                        // wait until LayoutUpdated event to have actual size of the popup and align it if needed
                        popup.LayoutUpdated += OnPopupLayoutUpdated;
                    }
                    
                    popup.IsOpen = true;
                }
            }
        }

        private static void OnPopupLayoutUpdated(object sender, object e)
        {
            if (currentPopup == null)
                return;

            currentPopup.LayoutUpdated -= OnPopupLayoutUpdated;

            Page page = TreeHelper.FindVisualAncestor<Page>(currentPopup);
            if (page == null || !(currentPopup.Child is FrameworkElement))
                return;

            Point popupLocation = currentPopup.TransformToVisual(page).TransformPoint(new Point());
            FrameworkElement popupChild = (FrameworkElement)currentPopup.Child;
            double popupWidth = popupChild.ActualWidth;
            double popupHeight = popupChild.ActualHeight;

            if (ResponsiveHelper.IsUsingSmallLayout())
            {
                // align verticaly and horizontally
                double newHorizontalOffset = (Window.Current.Bounds.Width - popupWidth) / 2;
                double newVerticalOffset = (Window.Current.Bounds.Height - popupHeight) / 2;

                currentPopup.HorizontalOffset = -popupLocation.X + newHorizontalOffset;
                currentPopup.VerticalOffset = -popupLocation.Y + newVerticalOffset;
            }
            else if (popupLocation.Y + popupChild.ActualHeight >= Window.Current.Bounds.Height)
            {
                // popup doesn't fit vertically:
                //  - it's not too close to the bottom of the page (otherwise it will open upward and we're fine)
                //  - it's going to be displaye
                double newVerticalOffset = (Window.Current.Bounds.Height - popupHeight) / 2;
                currentPopup.HorizontalOffset = 0;
                currentPopup.VerticalOffset = -popupLocation.Y + newVerticalOffset;
            }
            else
            {
                // don't do anything
                currentPopup.HorizontalOffset = 0;
                currentPopup.VerticalOffset = 0;
            }

            currentPopup = null;
        }
    }
}
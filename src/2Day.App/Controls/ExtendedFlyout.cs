using Windows.UI.Xaml;

namespace Chartreuse.Today.App.Controls
{
    public class ExtendedFlyout : DependencyObject
    {
        public static void SetFlyout(UIElement element, AppFlyout value)
        {
            element.SetValue(FlyoutProperty, value);
        }
        public static AppFlyout GetFlyout(UIElement element)
        {
            return (AppFlyout)element.GetValue(FlyoutProperty);
        }

        public static readonly DependencyProperty FlyoutProperty = DependencyProperty.RegisterAttached(
            nameof(FlyoutProperty),
            typeof(AppFlyout), 
            typeof(ExtendedFlyout),
            new PropertyMetadata(null, TemplatedFlyoutChanged));

        private static void TemplatedFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = d as UIElement;
            var flyout = e.NewValue as AppFlyout;

            if (flyout != null)
                flyout.Initialize(owner);
        }
    }
}
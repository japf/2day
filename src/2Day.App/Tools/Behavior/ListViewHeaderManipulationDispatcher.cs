using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.App.Tools.Behavior
{
    public static class ListViewHeaderManipulationDispatcher
    {
        private static IListViewAnimationManager listViewTaskFlyoutManager;

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
            typeof(ListViewHeaderManipulationDispatcher),
            new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;
            frameworkElement.PointerPressed += OnManipulationStarting;
        }

        private static void OnManipulationStarting(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement frameworkElement = (FrameworkElement)sender;

            if (listViewTaskFlyoutManager == null)
                listViewTaskFlyoutManager = Ioc.Resolve<IListViewAnimationManager>();

            bool handled = listViewTaskFlyoutManager.HandleManipulationStarting(frameworkElement);

            e.Handled = handled;
        }        
    }
}
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Chartreuse.Today.App.Tools
{
    public static class PopupHelper
    {
        public static void TryCloseParentPopup(this FrameworkElement frameworkElement)
        {
            var popup = frameworkElement.Parent as Popup;
            if (popup == null && frameworkElement.Parent is FrameworkElement)
                popup = ((FrameworkElement)frameworkElement.Parent).Parent as Popup;

            if (popup != null)
                popup.IsOpen = false;
        }
    }
}

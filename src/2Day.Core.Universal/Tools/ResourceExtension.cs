using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Chartreuse.Today.Core.Universal.Tools
{
    public static class ResourceExtensions
    {
        public static T FindResource<T>(this DependencyObject initial, string key)
        {
            DependencyObject current = initial;

            while (current != null)
            {
                if (current is FrameworkElement)
                {
                    if ((current as FrameworkElement).Resources.ContainsKey(key))
                    {
                        return (T)(current as FrameworkElement).Resources[key];
                    }
                }

                current = VisualTreeHelper.GetParent(current);
            }

            if (Application.Current.Resources.ContainsKey(key))
            {
                return (T)Application.Current.Resources[key];
            }

            return default(T);
        }
    }
}

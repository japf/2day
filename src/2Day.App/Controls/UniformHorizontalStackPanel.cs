using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chartreuse.Today.App.Controls
{
    /// <summary>
    /// A panel that stacks its visible children horizontally using the same width for all of them
    /// </summary>
    public class UniformHorizontalStackPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {           
            foreach (var child in this.Children)
            {
                child.Measure(availableSize);
            }

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var visibleChildren = this.Children.Where(c => c.Visibility == Visibility.Visible).OfType<FrameworkElement>().ToList();
            double width = finalSize.Width / visibleChildren.Count;

            for (var i = 0; i < visibleChildren.Count; i++)
            {
                var child = visibleChildren[i];
                child.Width = width;
                child.Arrange(new Rect(i * width, 0, width, finalSize.Height));
            }

            return finalSize;
        }
    }
}
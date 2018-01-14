using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Controls
{
    public class FlyoutHost : ContentControl
    {        
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(FlyoutHost),
            new PropertyMetadata(null));

        public Brush HeaderBackground
        {
            get { return (Brush)this.GetValue(HeaderBackgroundProperty); }
            set { this.SetValue(HeaderBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(
            "HeaderBackground",
            typeof(Brush),
            typeof(FlyoutHost),
            new PropertyMetadata(null));

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)this.GetValue(VerticalScrollBarVisibilityProperty); }
            set { this.SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(
            "VerticalScrollBarVisibility",
            typeof(ScrollBarVisibility),
            typeof(FlyoutHost),
            new PropertyMetadata(ScrollBarVisibility.Auto));
        
        protected override void OnApplyTemplate()
        {
            var textblock = (TextBlock) this.GetTemplateChild("PART_Title");
            var backButton = (Button) this.GetTemplateChild("PART_BackButton");
            var outterBorder = (Border) this.GetTemplateChild("PART_OutterBorder");
            var grid = (Grid) this.GetTemplateChild("PART_Grid");
            var content = (ContentControl) this.GetTemplateChild("PART_Content");

            textblock.Text = this.Text;

            // if flyout is shown as flyout and not in a full page, add a left border
            if (this.Parent is FrameworkElement && ((FrameworkElement)this.Parent).Parent is Popup)
            {
                outterBorder.Margin = new Thickness(0, 1, 0, 0);
                outterBorder.BorderThickness = new Thickness(1, 0, 0, 0);
            }

            if (ResponsiveHelper.IsUsingSmallLayout())
            {
                grid.RowDefinitions[0].Height = new GridLength(30);
                this.HeaderBackground = this.FindResource<SolidColorBrush>("HeaderBackgroundBrush");

                textblock.Style = this.FindResource<Style>("PageHeaderTextBlockStyle");
                textblock.Foreground = new SolidColorBrush(Colors.White);
                textblock.Text = textblock.Text.ToUpperInvariant();

                backButton.Visibility = Visibility.Collapsed;
                
                var currentInnerMargin = content.Margin;
                content.Margin = new Thickness(currentInnerMargin.Left, 20, currentInnerMargin.Right, currentInnerMargin.Bottom);
            }
            else
            {
                textblock.Style = this.FindResource<Style>("PageHeaderNormalFlyoutTextBlockStyle");

                backButton.Tapped += (s, e) =>
                {
                    Ioc.Resolve<INavigationService>().GoBack();
                };
            }
        }        
    }
}

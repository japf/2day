using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Tools.Behavior;
using Chartreuse.Today.Core.Shared.Icons;

namespace Chartreuse.Today.App.Controls
{
    public class IconButton : Button
    {
        public AppIconType Icon
        {
            get { return (AppIconType)this.GetValue(IconProperty); }
            set { this.SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon",
            typeof(AppIconType),
            typeof(IconButton),
            new PropertyMetadata(AppIconType.CommonAdd));

        public bool IsFullWidthFlyoutEnabled
        {
            get { return (bool)this.GetValue(IsFullWidthFlyoutEnabledProperty); }
            set { this.SetValue(IsFullWidthFlyoutEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsFullWidthFlyoutEnabledProperty = DependencyProperty.Register(
            "IsFullWidthFlyoutEnabled",
            typeof(AppIconType),
            typeof(IconButton),
            new PropertyMetadata(true));

        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(IconButton),
            new PropertyMetadata(null));

        public string SecondaryText
        {
            get { return (string)this.GetValue(SecondaryTextProperty); }
            set { this.SetValue(SecondaryTextProperty, value); }
        }

        public static readonly DependencyProperty SecondaryTextProperty = DependencyProperty.Register(
            "SecondaryText",
            typeof(string),
            typeof(IconButton),
            new PropertyMetadata(null));

        public IconButton()
        {
            this.Tapped += this.OnTapped;
        }

        protected override void OnApplyTemplate()
        {
            var border = this.GetTemplateChild("PART_Border") as Border;
            if (border != null && !string.IsNullOrEmpty(this.Text))
            {
                var panel = new StackPanel { Opacity = 0.7 };
                var text = new TextBlock
                {
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 250,
                    Text = this.Text
                };
                panel.Children.Add(text);

                if (!string.IsNullOrEmpty(this.SecondaryText))
                {
                    var secondaryText = new TextBlock
                    {
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 250,
                        Text = this.SecondaryText
                    };
                    panel.Children.Add(secondaryText);
                }

                this.Flyout = new Flyout {Content = panel};
            }
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(this);
            if (flyoutBase != null)
            {
                Flyout flyout = flyoutBase as Flyout;
                if (flyout != null)
                {
                    if (this.IsFullWidthFlyoutEnabled)
                        flyout.Placement = ResponsiveHelper.GetPopupPlacement();
                    else
                        flyout.Placement = FlyoutPlacementMode.Bottom;

                    if (flyout.Content is IFlyoutContent)
                        ((IFlyoutContent)flyout.Content).HandleFlyout(flyout);
                }

                flyoutBase.ShowAt(this);
            }
        }        
    }
}

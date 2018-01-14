using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Tools.UI;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class FlyoutAppBar : UserControl
    {
        public FlyoutAppBar()
        {
            this.InitializeComponent();
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ResponsiveHelper.IsUsingSmallLayout())
            {
                this.CustomCommandBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                var page = this.FindParent<Page>();
                if (page != null)
                {
                    var commandBar = page.BottomAppBar as CommandBar;
                    if (commandBar != null)
                    {
                        var appBarButtons = commandBar.PrimaryCommands.OfType<AppBarButton>().ToList();
                        commandBar.PrimaryCommands.Clear();
                        foreach (var child in appBarButtons)
                        {
                            this.CustomCommandBarGrid.Children.Add(child);

                            // the following is a nasty hack to hide the textual description of the AppBarButton in the new CustomCommandBarGrid grid
                            child.Loaded += (s, e1) =>
                            {
                                var textBlocks = TreeHelper.FindVisualChildren<TextBlock>(child);
                                foreach (var textBlock in textBlocks)
                                {
                                    if (textBlock.Name == "TextLabel" || textBlock.Name == "OverflowTextLabel")
                                        textBlock.Visibility = Visibility.Collapsed;
                                }
                            };
                        }

                        commandBar.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
    }
}

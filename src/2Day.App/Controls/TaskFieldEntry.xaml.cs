using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.Core.Shared.Icons;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class TaskFieldEntry : UserControl
    {
        private static Brush foregroundBrush;
        private bool lazyContentLoaded;

        public string Text
        {
            get { return (string) this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", 
            typeof(string), 
            typeof(TaskFieldEntry), 
            new PropertyMetadata(null));

        public string Description
        {
            get { return (string)this.GetValue(DescriptionProperty); }
            set { this.SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description",
            typeof(string),
            typeof(TaskFieldEntry),
            new PropertyMetadata(null));

        public object InnerContent
        {
            get { return (object)this.GetValue(InnerContentProperty); }
            set { this.SetValue(InnerContentProperty, value); }
        }

        public static readonly DependencyProperty InnerContentProperty = DependencyProperty.Register(
            "InnerContent",
            typeof(string),
            typeof(TaskFieldEntry),
            new PropertyMetadata(null));

        public AppIconType Icon
        {
            get { return (AppIconType) this.GetValue(IconProperty); }
            set { this.SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon",
            typeof(AppIconType),
            typeof(TaskFieldEntry),
            new PropertyMetadata(AppIconType.None));

        public Brush IconForeground
        {
            get { return (Brush)this.GetValue(IconForegroundProperty); }
            set { this.SetValue(IconForegroundProperty, value); }
        }

        public static readonly DependencyProperty IconForegroundProperty = DependencyProperty.Register(
            "IconForeground",
            typeof(AppIconType),
            typeof(Brush),
            new PropertyMetadata(null));

        public AppIconType SecondaryIcon
        {
            get { return (AppIconType)this.GetValue(SecondaryIconProperty); }
            set { this.SetValue(SecondaryIconProperty, value); }
        }

        public static readonly DependencyProperty SecondaryIconProperty = DependencyProperty.Register(
            "SecondaryIcon",
            typeof(AppIconType),
            typeof(TaskFieldEntry),
            new PropertyMetadata(AppIconType.CommonClear));

        public ICommand SecondaryCommand
        {
            get { return (ICommand) this.GetValue(SecondaryCommandProperty); }
            set { this.SetValue(SecondaryCommandProperty, value); }
        }

        public static readonly DependencyProperty SecondaryCommandProperty = DependencyProperty.Register(
            "SecondaryCommand",
            typeof(ICommand),
            typeof(TaskFieldEntry),
            new PropertyMetadata(null));

        public bool IsVisible
        {
            get { return (bool)this.GetValue(IsVisibleProperty); }
            set { this.SetValue(IsVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(
            "IsVisible",
            typeof(bool),
            typeof(TaskFieldEntry),
            new PropertyMetadata(false, OnIsVisibleChanged));

        public Button SourceButton
        {
            get { return (Button)this.GetValue(SourceButtonProperty); }
            set { this.SetValue(SourceButtonProperty, value); }
        }

        public static readonly DependencyProperty SourceButtonProperty = DependencyProperty.Register(
            "SourceButton",
            typeof(Button),
            typeof(TaskFieldEntry),
            new PropertyMetadata(null));

        public TaskFieldEntry This
        {
            get { return this; }
        }

        public TaskFieldEntry()
        {
            this.InitializeComponent();

            if (foregroundBrush == null)
                foregroundBrush = this.FindResource<SolidColorBrush>("ApplicationForegroundThemeBrush");

            this.IconForeground = foregroundBrush;
            this.Visibility = Visibility.Collapsed;

            this.Tapped += this.OnTapped;
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.SourceButton != null && TreeHelper.FindVisualAncestor<IconButton>(e.OriginalSource as UIElement) == null)
            {
                if (!this.lazyContentLoaded)
                {
                    var taskPage = TreeHelper.FindVisualAncestor<TaskPage>(this);
                    taskPage.FindName(this.SourceButton.Name + "FlyoutContent");
                    this.lazyContentLoaded = true;
                }

                var extendedFlyout = ExtendedFlyout.GetFlyout(this.SourceButton);
                if (extendedFlyout != null)
                    extendedFlyout.Show(e.OriginalSource as FrameworkElement);
            }
        }

        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TaskFieldEntry) d).OnIsVisibleChanged(e);
        }

        private async Task OnIsVisibleChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
            {
                this.Visibility = Visibility.Visible;
                this.Opacity = 0;
                this.SlideHorizontalAsync(TimeSpan.FromMilliseconds(400), -10, 0);
                this.AnimateOpacity(1, TimeSpan.FromMilliseconds(300));
            }
            else
            {
                this.Opacity = 1;
                this.AnimateOpacity(0, TimeSpan.FromMilliseconds(300));
                await this.SlideHorizontalAsync(TimeSpan.FromMilliseconds(400), 0, 10);
                this.Visibility = Visibility.Collapsed;
            }
        }
    }
}

using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Chartreuse.Today.App.Controls
{
    public partial class FolderIconPicker : UserControl
    {
        private const int AnimationDurationMs = 100;

        private readonly Point emptyPoint = new Point();

        private DoubleAnimation animationX;
        private DoubleAnimation animationY;
        private Storyboard storyboard;

        public FolderIconPicker()
        {
            this.InitializeComponent();

            this.iconListBox.SelectionChanged += this.OnSelectionChanged;

            this.Loaded += this.OnLoaded;
            this.SizeChanged += this.OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.overlayHost.Height = this.iconListBox.ActualHeight;
            this.overlayHost.Width = this.iconListBox.ActualWidth;
            this.overlayHost.Margin = this.iconListBox.Margin;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.overlayHost.Height = this.iconListBox.ActualHeight;
            this.overlayHost.Width = this.iconListBox.ActualWidth;
            this.overlayHost.Margin = this.iconListBox.Margin;

            this.UpdateHighlightedItem();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateHighlightedItem();
        }

        private void UpdateHighlightedItem()
        {
            if (this.iconListBox.SelectedIndex < 0)
                return;

            var element = this.iconListBox.ContainerFromIndex(this.iconListBox.SelectedIndex) as FrameworkElement;
            if (element != null)
            {
                // compute the location of the selected element
                Point destination = element.TransformToVisual(this.iconListBox).TransformPoint(this.emptyPoint);
                Point origin = this.iconOverlay.TransformToVisual(this.iconListBox).TransformPoint(this.emptyPoint);

                if (this.iconOverlay.RenderTransform as TranslateTransform == null)
                {
                    this.iconOverlay.RenderTransform = new TranslateTransform(); ;

                    TimeSpan duration = TimeSpan.FromMilliseconds(AnimationDurationMs);

                    this.animationX = new DoubleAnimation { Duration = duration };
                    this.animationY = new DoubleAnimation { Duration = duration };

                    this.storyboard = new Storyboard();
                    this.storyboard.Children.Add(this.animationX);
                    this.storyboard.Children.Add(this.animationY);

                    Storyboard.SetTarget(this.animationX, this.iconOverlay.RenderTransform);
                    Storyboard.SetTarget(this.animationY, this.iconOverlay.RenderTransform);
                    Storyboard.SetTargetProperty(this.animationX, "X");
                    Storyboard.SetTargetProperty(this.animationY, "Y");
                }
                else
                {
                    this.storyboard.Stop();
                }

                this.animationX.From = origin.X;
                this.animationY.From = origin.Y;
                this.animationX.To = destination.X;
                this.animationY.To = destination.Y;

                this.storyboard.Begin();
            }
        }
    }
}

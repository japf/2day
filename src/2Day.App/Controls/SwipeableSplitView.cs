using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Controls
{
    public sealed class SwipeableSplitView : SplitView
    {
        private Grid paneRoot;
        private Grid overlayRoot;
        private Rectangle panArea;
        private Rectangle dismissLayer;

        private CompositeTransform paneRootTransform;
        private CompositeTransform panAreaTransform;

        private Storyboard openSwipeablePane;
        private Storyboard closeSwipeablePane;

        public static readonly DependencyProperty IsSwipeablePaneOpenProperty = DependencyProperty.Register(
            nameof(IsSwipeablePaneOpen), 
            typeof(bool), 
            typeof(SwipeableSplitView), 
            new PropertyMetadata(false, OnIsSwipeablePaneOpenChanged));

        public bool IsSwipeablePaneOpen
        {
            get { return (bool) this.GetValue(IsSwipeablePaneOpenProperty); }
            set { this.SetValue(IsSwipeablePaneOpenProperty, value); }
        }

        public static readonly DependencyProperty PanAreaInitialTranslateXProperty = DependencyProperty.Register(
            nameof(PanAreaInitialTranslateX), 
            typeof(double), 
            typeof(SwipeableSplitView), 
            new PropertyMetadata(0d));

        public double PanAreaInitialTranslateX
        {
            get { return (double) this.GetValue(PanAreaInitialTranslateXProperty); }
            set { this.SetValue(PanAreaInitialTranslateXProperty, value); }
        }

        public static readonly DependencyProperty PanAreaThresholdProperty = DependencyProperty.Register(
            nameof(PanAreaThreshold), 
            typeof(double), 
            typeof(SwipeableSplitView), 
            new PropertyMetadata(20d));

        public double PanAreaThreshold
        {
            get { return (double) this.GetValue(PanAreaThresholdProperty); }
            set { this.SetValue(PanAreaThresholdProperty, value); }
        }

        public SwipeableSplitView()
        {
            this.DefaultStyleKey = typeof(SwipeableSplitView);
        }

        private void SetPaneRoot(Grid value)
        {
            if (this.paneRoot != null)
            {
                this.paneRoot.ManipulationStarted -= this.OnManipulationStarted;
                this.paneRoot.ManipulationDelta -= this.OnManipulationDelta;
                this.paneRoot.ManipulationCompleted -= this.OnManipulationCompleted;
            }

            this.paneRoot = value;

            if (this.paneRoot != null)
            {
                this.paneRoot.ManipulationStarted += this.OnManipulationStarted;
                this.paneRoot.ManipulationDelta += this.OnManipulationDelta;
                this.paneRoot.ManipulationCompleted += this.OnManipulationCompleted;
            }
        }

        private void SetPanArea(Rectangle value)
        {
            if (this.panArea != null)
            {
                this.panArea.ManipulationStarted -= this.OnManipulationStarted;
                this.panArea.ManipulationDelta -= this.OnManipulationDelta;
                this.panArea.ManipulationCompleted -= this.OnManipulationCompleted;
                this.panArea.Tapped -= this.OnDismissLayerTapped;
            }

            this.panArea = value;

            if (this.panArea != null)
            {
                this.panArea.ManipulationStarted += this.OnManipulationStarted;
                this.panArea.ManipulationDelta += this.OnManipulationDelta;
                this.panArea.ManipulationCompleted += this.OnManipulationCompleted;
                this.panArea.Tapped += this.OnDismissLayerTapped;
            }
        }

        private void SetDismissLayer(Rectangle value)
        {
            if (this.dismissLayer != null)
            {
                this.dismissLayer.Tapped -= this.OnDismissLayerTapped;
            }

            this.dismissLayer = value;

            if (this.dismissLayer != null)
            {
                this.dismissLayer.Tapped += this.OnDismissLayerTapped;
            }
        }

        private void SetOpenSwipeablePaneAnimation(Storyboard value)
        {
            if (this.openSwipeablePane != null)
            {
                this.openSwipeablePane.Completed -= this.OnOpenSwipeablePaneCompleted;
            }

            this.openSwipeablePane = value;

            if (this.openSwipeablePane != null)
            {
                this.openSwipeablePane.Completed += this.OnOpenSwipeablePaneCompleted;
            }
        }

        private void SetCloseSwipeablePaneAnimation(Storyboard value)
        {
            if (this.closeSwipeablePane != null)
            {
                this.closeSwipeablePane.Completed -= this.OnCloseSwipeablePaneCompleted;
            }

            this.closeSwipeablePane = value;

            if (this.closeSwipeablePane != null)
            {
                this.closeSwipeablePane.Completed += this.OnCloseSwipeablePaneCompleted;
            }
        }

        private static void OnIsSwipeablePaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var splitView = d as SwipeableSplitView;
                if (splitView != null)
                {
                    switch (splitView.DisplayMode)
                    {
                        case SplitViewDisplayMode.Inline:
                        case SplitViewDisplayMode.CompactOverlay:
                        case SplitViewDisplayMode.CompactInline:
                            splitView.IsPaneOpen = (bool)e.NewValue;
                            break;

                        case SplitViewDisplayMode.Overlay:
                            if (splitView.openSwipeablePane == null || splitView.closeSwipeablePane == null)
                                return;
                            if ((bool)e.NewValue)
                            {
                                splitView.OpenSwipeablePane();
                            }
                            else
                            {
                                splitView.CloseSwipeablePane();
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "SwipeableListView.OnIsSwipeablePaneOpenChanged");
            }
        }

        protected override void OnApplyTemplate()
        {
            try
            {
                this.SetPaneRoot(this.GetTemplateChild<Grid>("PaneRoot"));

                this.overlayRoot = this.GetTemplateChild<Grid>("OverlayRoot");
                this.SetPanArea(this.GetTemplateChild<Rectangle>("PanArea"));
                this.SetDismissLayer(this.GetTemplateChild<Rectangle>("DismissLayer"));

                var rootGrid = GetParent<Grid>(this.paneRoot);

                this.SetOpenSwipeablePaneAnimation(GetStoryboard(rootGrid, "OpenSwipeablePane"));
                this.SetCloseSwipeablePaneAnimation(GetStoryboard(rootGrid, "CloseSwipeablePane"));

                // initialization
                this.OnDisplayModeChanged(null, null);

                this.RegisterPropertyChangedCallback(DisplayModeProperty, this.OnDisplayModeChanged);

                // disable ScrollViewer as it will prevent finger from panning
                if (this.Pane is ListView || this.Pane is ListBox)
                {
                    ScrollViewer.SetVerticalScrollMode(this.Pane, ScrollMode.Disabled);
                }

                base.OnApplyTemplate();
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "SwipeableListView.OnApplyTemplate");
            }
        }

        private void OnDisplayModeChanged(DependencyObject sender, DependencyProperty dp)
        {
            try
            {
                switch (this.DisplayMode)
                {
                    case SplitViewDisplayMode.Inline:
                    case SplitViewDisplayMode.CompactOverlay:
                    case SplitViewDisplayMode.CompactInline:
                        this.PanAreaInitialTranslateX = 0d;
                        this.overlayRoot.Visibility = Visibility.Collapsed;
                        break;

                    case SplitViewDisplayMode.Overlay:
                        this.PanAreaInitialTranslateX = this.OpenPaneLength * -1;
                        this.overlayRoot.Visibility = Visibility.Visible;
                        break;
                }

                if (this.paneRoot.RenderTransform is CompositeTransform)
                    ((CompositeTransform)this.paneRoot.RenderTransform).TranslateX = this.PanAreaInitialTranslateX;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "SwipeableListView.OnDisplayModeChanged");
            }
        }

        private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            try
            {
                this.panAreaTransform = GetCompositeTransform(this.panArea);
                this.paneRootTransform = GetCompositeTransform(this.paneRoot);
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "SwipeableListView.OnManipulationStarted");
            }

        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                var x = this.panAreaTransform.TranslateX + e.Delta.Translation.X;

                // keep the pan within the bountry
                if (x < this.PanAreaInitialTranslateX || x > 0)
                    return;

                // while we are panning the PanArea on X axis, let's sync the PaneRoot's position X too
                this.paneRootTransform.TranslateX = this.panAreaTransform.TranslateX = x;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "SwipeableListView.OnManipulationDelta");
            }
        }

        private async void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            try
            {
                var x = e.Velocities.Linear.X;

                // ignore a little bit velocity (+/-0.1)
                if (x <= -0.1)
                {
                    this.CloseSwipeablePane();
                }
                else if (x > -0.1 && x < 0.1)
                {
                    if (Math.Abs(this.panAreaTransform.TranslateX) > Math.Abs(this.PanAreaInitialTranslateX) / 2)
                    {
                        this.CloseSwipeablePane();
                    }
                    else
                    {
                        this.OpenSwipeablePane();
                    }
                }
                else
                {
                    this.OpenSwipeablePane();
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "SwipeableListView.OnManipulationCompleted");
            }
        }
        
        private void OnDismissLayerTapped(object sender, TappedRoutedEventArgs e)
        {
            this.CloseSwipeablePane();
        }
        
        private void OnOpenSwipeablePaneCompleted(object sender, object e)
        {
            try
            {
                if (this.dismissLayer != null)
                    this.dismissLayer.IsHitTestVisible = true;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "SwipeableListView.OnOpenSwipeablePaneCompleted");
            }

        }

        private void OnCloseSwipeablePaneCompleted(object sender, object e)
        {
            try
            {
                if (this.dismissLayer != null)
                    this.dismissLayer.IsHitTestVisible = false;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "SwipeableListView.OnCloseSwipeablePaneCompleted");
            }

        }
        
        private void OpenSwipeablePane()
        {
            try
            {
                if (this.IsSwipeablePaneOpen)
                {
                    this.openSwipeablePane.Begin();
                }
                else
                {
                    this.IsSwipeablePaneOpen = true;
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "SwipeableListView.OpenSwipeablePane");
            }
        }

        private void CloseSwipeablePane()
        {
            try
            {
                if (!this.IsSwipeablePaneOpen)
                {
                    this.closeSwipeablePane.Begin();
                }
                else
                {
                    this.IsSwipeablePaneOpen = false;
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "SwipeableListView.CloseSwipeablePane");
            }
        }

        private T GetTemplateChild<T>(string name) where T : DependencyObject
        {
            var child = this.GetTemplateChild(name) as T;

            if (child == null)
            {             
                throw new NullReferenceException($"Could not find child {name}");
            }

            return child;
        }

        public void TryClose()
        {
            try
            {
                if (this.IsPaneOpen)
                {
                    this.IsPaneOpen = false;
                }
                else if (this.IsSwipeablePaneOpen)
                {
                    this.closeSwipeablePane.Begin();
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "SwipeableListView.TryClose");
            }
        }

        private static T GetParent<T>(FrameworkElement element, string message = null) where T : DependencyObject
        {
            var parent = element.Parent as T;

            if (parent == null)
            {
                if (message == null)
                {
                    message = "Parent element should not be null! Check the default Generic.xaml.";
                }

                throw new NullReferenceException(message);
            }

            return parent;
        }

        private static Storyboard GetStoryboard(FrameworkElement element, string name, string message = null)
        {
            var storyboard = element.Resources[name] as Storyboard;

            if (storyboard == null)
            {
                if (message == null)
                {
                    message = $"Storyboard '{name}' cannot be found! Check the default Generic.xaml.";
                }

                throw new NullReferenceException(message);
            }

            return storyboard;
        }

        private static CompositeTransform GetCompositeTransform(FrameworkElement element, string message = null)
        {
            var transform = element.RenderTransform as CompositeTransform;

            if (transform == null)
            {
                if (message == null)
                {
                    message = $"{element.Name}'s RenderTransform should be a CompositeTransform! Check the default Generic.xaml.";
                }

                throw new NullReferenceException(message);
            }

            return transform;
        }
    }    
}
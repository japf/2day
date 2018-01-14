using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Chartreuse.Today.App.Controls;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Groups;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Tools
{
    /// <summary>
    /// An help class which handles animation on the task list control. All the work is done in code here and not in 
    /// XAML for performance reasons: we do not want to instantiate all the stuff needed here until the user really 
    /// perform the interaction which we want to handle
    /// </summary>
    public class ListViewAnimationManager : IListViewAnimationManager
    {
        // time of the overlay animation (width)
        private const int AnimationDurationMs = 450;

        // time the user must tap an item without moving to start the overlay animation
        private const int DelayBeforeAnimation = 200;
        
        // opacity of the overlay
        private const double OverlayOpacity = 0.15;

        private static SolidColorBrush overlayBrush;

        private readonly ListView listview;
        private readonly IWorkbook workbook;
        private readonly INavigationService navigationService;

        private Storyboard storyboard;
        private DoubleAnimation animation;

        private Grid currentTarget;
        private readonly List<ITask> currentTasks;
        private Border overlay;
        private DispatcherTimer timer;
        private Flyout flyout;
        private ScrollViewer scrollviewer;

        public ListViewAnimationManager(ListView listview, IWorkbook workbook, INavigationService navigationService)
        {
            if (listview == null)
                throw new ArgumentNullException(nameof(listview));
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            this.listview = listview;
            this.workbook = workbook;
            this.navigationService = navigationService;
            this.currentTasks = new List<ITask>();

            // register handlers for the appropriate events
            this.listview.ManipulationStarting += this.OnManipulationStarting;
            this.listview.ManipulationDelta += this.OnManipulationDelta;
            this.listview.ManipulationCompleted += this.OnManipulationCompleted;
            this.listview.ItemClick += this.OnItemClick;

            overlayBrush = listview.FindResource<SolidColorBrush>("TaskHoldOverlayBrush");
        }

        private void EnsureAnimationReady()
        {
            if (this.scrollviewer == null)
            {
                this.scrollviewer = TreeHelper.FindVisualChild<ScrollViewer>(this.listview);
                this.scrollviewer.DirectManipulationStarted += (s, e) => this.CancelInteraction();
            }

            // use a lazy initialization in order to create elements only when we really need them
            if (this.storyboard != null)
                return;

            this.storyboard = new Storyboard();
            this.storyboard.Completed += this.OnStoryboardCompleted;

            this.animation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDurationMs)),
                From = -this.currentTarget.ActualWidth,
                To = 0,
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            var translateTransform = new TranslateTransform();

            this.overlay = new Border
            {
                Opacity = OverlayOpacity,
                Margin = new Thickness(0, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = this.currentTarget.ActualWidth,
                RenderTransform = translateTransform,
                Background = overlayBrush
            };

            this.storyboard.Children.Add(this.animation);
            Storyboard.SetTargetProperty(this.storyboard, "X");
            Storyboard.SetTarget(this.storyboard, translateTransform);

            this.timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(DelayBeforeAnimation) };
            this.timer.Tick += this.OnTimerTick;
        }

        private void OnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;
            if (originalSource == null)
                return;

            bool handled = this.HandleManipulationStarting(originalSource);
            e.Handled = handled;
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (this.flyout == null)
            {
                this.CancelInteraction();

                var task = (ITask)e.ClickedItem;
                this.navigationService.Navigate(ViewLocator.CreateEditTaskPageNew, task);
            }
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            this.CancelInteraction();
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            this.CancelInteraction();
        }

        public bool OpenFlyout(ITask task)
        {
            var container = this.listview.ContainerFromItem(task) as FrameworkElement;
            if (container != null)
            {
                var grid = TreeHelper.FindVisualChild<Grid>(container);
                if (grid != null)
                {
                    this.currentTarget = grid;
                    this.ShowFlyout();
                }
            }

            return false;
        }

        public bool HandleManipulationStarting(FrameworkElement originalSource)
        {
            bool handled = false;

            List<ITask> tasks;

            if (originalSource is ListViewItem)
                originalSource = TreeHelper.FindVisualChild<Grid>(originalSource);

            Grid target = TreeHelper.FindVisualAncestor<Grid>(originalSource);

            Group<ITask> group = null;
            if (target.Children.Count == 1 && target.Children[0] is FrameworkElement)
            {
                group = ((FrameworkElement)target.Children[0]).DataContext as Group<ITask>;
                if (group != null)
                {
                    foreach (var task in group)
                    {
                        var container = this.listview.ContainerFromItem(task);
                        if (container != null)
                        {
                            var swipableItem = TreeHelper.FindVisualChild<SwipableListViewItem>(container);
                            if (swipableItem != null)
                                swipableItem.Reset();
                        }
                    }
                }
            }

            double topMargin = 0;
            double bottomMargin = 0;

            if (group != null)
            {
                var grids = new List<Grid>();
                foreach (var task in group)
                {
                    var container = this.listview.ContainerFromItem(task) as FrameworkElement;
                    if (container != null)
                    {
                        var grid = TreeHelper.FindVisualChild<Grid>(container);
                        if (grid != null)
                            grids.Add(grid);
                    }
                }

                topMargin = target.ActualHeight;
                bottomMargin = -grids.Sum(g => g.ActualHeight);

                tasks = new List<ITask>(group);

                // prevent opening of the default menu
                handled = true;
            }
            else
            {
                var grid = target;
                var task = grid.DataContext as ITask;
                if (task == null)
                    return false;

                tasks = new List<ITask> {task};
                target = grid;
            }

            this.currentTarget = target;

            this.EnsureAnimationReady();

            this.overlay.Margin = new Thickness(0, topMargin, 0, bottomMargin);

            this.currentTasks.Clear();
            this.currentTasks.AddRange(tasks);
            
            this.timer.Start();

            return handled;
        }
        
        private void OnTimerTick(object sender, object o)
        {
            this.timer.Stop();

            if (this.currentTarget.Children.Contains(this.overlay))
                this.currentTarget.Children.Remove(this.overlay);

            this.currentTarget.Children.Add(this.overlay);
            this.storyboard.Begin();
        }

        private void OnStoryboardCompleted(object sender, object o)
        {
            this.ShowFlyout();
        }

        private void ShowFlyout()
        {
            if (this.currentTarget == null)
                return;

            var swipableItem = TreeHelper.FindVisualAncestor<SwipableListViewItem>(this.currentTarget);
            if (swipableItem != null)
                swipableItem.Reset();

            try
            {
                this.flyout = new Flyout
                {
                    Content = new ListViewActionBar(this.currentTasks, this.currentTarget) { MinHeight = 50 },
                    FlyoutPresenterStyle = this.currentTarget.FindResource<Style>("ModernFlyoutPresenterStyle"),
                    Placement = FlyoutPlacementMode.Bottom
                };

                this.flyout.ShowAt(this.currentTarget);
                this.flyout.Closed += (s, e) =>
                {
                    if (this.overlay != null && this.currentTarget.Children.Contains(this.overlay))
                        this.currentTarget.Children.Remove(this.overlay);

                    this.flyout = null;
                };
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "ListViewTaskFlyoutManager - Failed to open flyout");
            }
        }

        private void CancelInteraction()
        {
            if (this.storyboard != null && (this.storyboard.GetCurrentState() == ClockState.Active || this.timer.IsEnabled))
            {
                this.storyboard.Stop();
                this.timer.Stop();
            }

            if (this.currentTarget != null && this.overlay != null)
                this.currentTarget.Children.Remove(this.overlay);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Groups;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Manager.UI
{
    public class DragDropManager
    {
        private const int DragDropHostAnimationTimeMs = 200;
        private readonly double BackgroundIndicatorOpacity = 0.8;

        private readonly MainPage mainPage;

        private readonly GridView gridviewTasks;
        private readonly ListView listviewNavigation;

        private readonly IWorkbook workbook;

        private readonly INavigationService navigationService;
        private readonly ITrackingManager trackingManager;

        private readonly List<ITask> draggedItems;
        private ListViewItem highlightedListViewItem;
        private Group<ITask> currentTargetGroup;

        private readonly SolidColorBrush highlightBrush;
        private Border dragDropHost;
        private ItemsWrapGrid gridViewPanel;
        private int ignoreSelectionChanges;

        public DragDropManager(MainPage mainPage, GridView gridviewTasks, ListView listviewNavigation, IWorkbook workbook, INavigationService navigationService, ITrackingManager trackingManager)
        {
            if (mainPage == null)
                throw new ArgumentNullException(nameof(mainPage));
            if (gridviewTasks == null)
                throw new ArgumentNullException(nameof(gridviewTasks));
            if (listviewNavigation == null)
                throw new ArgumentNullException(nameof(listviewNavigation));
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));
            if (trackingManager == null)
                throw new ArgumentNullException(nameof(trackingManager));

            this.mainPage = mainPage;

            this.gridviewTasks = gridviewTasks;
            this.gridviewTasks.ItemClick += this.OnGridViewItemClick;
            this.gridviewTasks.Tapped += this.OnGridViewTapped;
            this.gridviewTasks.RightTapped += this.OnGridViewRightTapped;
            this.gridviewTasks.DragItemsStarting += this.OnGridViewDragItemsStarting;
            this.gridviewTasks.Loaded += this.OnGridViewLoaded;
            this.gridviewTasks.SelectionChanged += this.OnGridViewSelectionChanged;
               
            this.listviewNavigation = listviewNavigation;
            this.listviewNavigation.DragOver += this.OnListviewNavigationDragOver;
            this.listviewNavigation.Drop += this.OnListviewNavigationDrop;
            this.listviewNavigation.DragLeave += this.OnListviewNavigationDragLeave;

            this.workbook = workbook;
            this.navigationService = navigationService;
            this.trackingManager = trackingManager;
            this.draggedItems  = new List<ITask>();

            this.highlightBrush = this.mainPage.FindResource<SolidColorBrush>("ApplicationAccentBrush");
            this.ignoreSelectionChanges = -1;

            if (this.workbook.Settings.GetValue<bool>(CoreSettings.UseDarkTheme))
                this.BackgroundIndicatorOpacity = 0.6;
            else
                this.BackgroundIndicatorOpacity = 0.4;
        }

        private void OnGridViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ignoreSelectionChanges >= 0)
            {
                this.ignoreSelectionChanges--;
                return;
            }

            if (this.mainPage.ActualWidth < ResponsiveHelper.MinWidth)
                return;

            if (this.gridviewTasks.SelectedItems.Count > 0)
            {
                this.mainPage.ShowContextualActionBar();
            }
            else if (this.gridviewTasks.SelectedItems.Count == 0)
            {
                this.mainPage.HideContextualActionBar();
            }
        }

        private void OnGridViewItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ITask && (!WinKeyboardHelper.IsKeyDown(VirtualKey.Control) && !WinKeyboardHelper.IsKeyDown(VirtualKey.Shift)))
            {
                var task = (ITask)e.ClickedItem;

                // hack to make sure we'll ignore the next 2 SelectionChanged event
                // we do this, because we don't want to open the action bar in that case
                this.ignoreSelectionChanges = 3;
                this.gridviewTasks.SelectedItems.Clear();
                this.mainPage.HideContextualActionBar();

                this.navigationService.FlyoutTo(ViewLocator.CreateEditTaskPageNew, task);
            }
        }

        private void OnGridViewTapped(object sender, TappedRoutedEventArgs e)
        {
            // if we tap an actual item, do nothing as this is handled in OnGridViewItemClick
            // if we tap a group header, do nothing too
            var fe = e.OriginalSource as FrameworkElement;
            if (fe != null && !(fe.DataContext is ITask) && !(fe.DataContext is Group<ITask>))
            {
                this.navigationService.CloseFlyouts();
                this.gridviewTasks.SelectedItems.Clear();
                this.mainPage.HideContextualActionBar();

                this.ignoreSelectionChanges = -1;
            }
        }

        private void OnGridViewRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var fe = e.OriginalSource as FrameworkElement;
            if (fe != null && fe.DataContext is ITask)
            {
                this.navigationService.CloseFlyouts();
                this.ignoreSelectionChanges = -1;

                if (!this.gridviewTasks.SelectedItems.Contains(fe.DataContext))
                    this.gridviewTasks.SelectedItems.Add(fe.DataContext);
                else
                    this.gridviewTasks.SelectedItems.Remove(fe.DataContext);
            }
        }

        private void OnGridViewLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.gridViewPanel == null)
            {
                this.gridViewPanel = TreeHelper.FindVisualChild<ItemsWrapGrid>(this.gridviewTasks);
                if (this.gridViewPanel != null)
                {
                    this.gridViewPanel.Drop += this.OnGridViewGroupDrop;
                    this.gridViewPanel.DragOver += this.OnGridViewDragOver;
                    this.gridViewPanel.DragLeave += this.OnGridViewDragLeave;

                    this.dragDropHost = TreeHelper.FindVisualChildren<Border>(this.gridviewTasks).FirstOrDefault(c => c.Name == "dragDropFeedbackHost");
                    this.dragDropHost.DragOver += this.OnGridViewDragOver;
                }
            }
        }

        private void OnListviewNavigationDrop(object sender, DragEventArgs e)
        {
            if (this.dragDropHost != null)
                this.dragDropHost.Child = null;

            if (this.highlightedListViewItem != null && this.draggedItems.Count > 0)
            {
                this.highlightedListViewItem.ClearValue(Control.BackgroundProperty);

                var folderItem = this.highlightedListViewItem.Content as FolderItemViewModel;
                if (folderItem != null && folderItem.Folder.CanReceiveTasks)
                {
                    using (this.workbook.WithTransaction())
                    {
                        folderItem.Folder.MoveTasks(this.draggedItems);
                    }
                }
            }

            this.draggedItems.Clear();
        }

        private void OnListviewNavigationDragOver(object sender, DragEventArgs e)
        {
            var elements = VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(this.mainPage), this.listviewNavigation).ToList();
            var item = elements.FirstOrDefault(i => i is ListViewItem);
            if (item != null && this.draggedItems.Count > 0)
            {
                e.AcceptedOperation = DataPackageOperation.Move;
                if (e.DragUIOverride != null)
                {
                    e.DragUIOverride.IsContentVisible = false;
                    e.DragUIOverride.IsCaptionVisible = true;

                    string title = this.draggedItems[0].Title;
                    if (title.Length > 24)
                        title = title.Substring(0, 24) + "...";

                    if (this.draggedItems.Count == 1)
                        e.DragUIOverride.Caption = title;
                    else
                        e.DragUIOverride.Caption = string.Format("{0} + {1}", title, this.draggedItems.Count - 1);
                }

                var folderItem = ((ListViewItem)item).Content as FolderItemViewModel;
                if (folderItem == null || !folderItem.Folder.CanReceiveTasks)
                    return;

                if (this.highlightedListViewItem != null)
                    this.highlightedListViewItem.ClearValue(Control.BackgroundProperty);

                this.highlightedListViewItem = (ListViewItem)item;
                this.highlightedListViewItem.Background = this.highlightBrush;
            }
        }

        private void OnListviewNavigationDragLeave(object sender, DragEventArgs e)
        {
            if (this.highlightedListViewItem != null)
                this.highlightedListViewItem.ClearValue(Control.BackgroundProperty);
        }

        private void OnGridViewDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            this.draggedItems.Clear();
            this.draggedItems.AddRange(e.Items.OfType<ITask>());
        }

        private void OnGridViewGroupDrop(object sender, DragEventArgs e)
        {
            if (this.draggedItems.Count > 0 && this.currentTargetGroup != null)
            {
                this.dragDropHost.Child = null;

                List<ITask> tasks = this.draggedItems;
                ITask sampleTask = this.currentTargetGroup[0];

                if (this.currentTargetGroup.Owner is IAbstractFolder)
                {
                    using (this.workbook.WithTransaction())
                    {
                        var abstractFolder = (IAbstractFolder) this.currentTargetGroup.Owner;
                        switch (abstractFolder.TaskGroup)
                        {
                            case TaskGroup.DueDate:
                                foreach (var task in tasks)
                                    task.SetDueAndAdjustReminder(sampleTask.Due, tasks);
                                break;
                            case TaskGroup.Priority:
                                foreach (var task in tasks)
                                    task.Priority = sampleTask.Priority;
                                break;
                            case TaskGroup.Status:
                                foreach (var task in tasks)
                                    task.IsCompleted = sampleTask.IsCompleted;
                                break;
                            case TaskGroup.Folder:
                                foreach (var task in tasks)
                                    task.Folder = sampleTask.Folder;
                                break;
                            case TaskGroup.Action:
                                break;
                            case TaskGroup.Progress:
                                foreach (var task in tasks)
                                    task.Progress = sampleTask.Progress;
                                break;
                            case TaskGroup.Context:
                                foreach (var task in tasks)
                                    task.Context = sampleTask.Context;
                                break;
                            case TaskGroup.StartDate:
                                foreach (var task in tasks)
                                    task.Start = sampleTask.Start;
                                break;
                            case TaskGroup.Completed:
                                foreach (var task in tasks)
                                    task.Completed = sampleTask.Completed;
                                break;
                            case TaskGroup.Modified:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                this.StopGridDragOver();
            }

            this.draggedItems.Clear();
        }

        private void OnGridViewDragOver(object sender, DragEventArgs e)
        {
            if (this.draggedItems.Count == 0)
                return;

            e.AcceptedOperation = DataPackageOperation.Move;

            // lot of hacks in this method...
            Point intersectingPoint = e.GetPosition(this.mainPage);

            // first is to find a GridViewItem based on the current mouse position
            FrameworkElement element = null;
            int index = 0;
            while (element == null && index < 2)
            {
                // use X from touch event, but find out Y because user can drag of an empty
                // area (if he moves the first item of a column)

                // get the one of the first GridViewItem in the list just to get its height + Y position
                var container = this.gridviewTasks.ContainerFromIndex(index) as GridViewItem;
                if (container == null)
                    return;

                var point = container.TransformToVisual(this.mainPage).TransformPoint(new Point());
                intersectingPoint.Y = point.Y + container.ActualHeight / 2;

                element = VisualTreeHelper
                    .FindElementsInHostCoordinates(intersectingPoint, this.gridviewTasks)
                    .OfType<FrameworkElement>()
                    .FirstOrDefault(fe => fe.DataContext is ITask);

                index++;
            }

            ITask task = null;
            if (element != null && element.DataContext is ITask)
                task = (ITask) element.DataContext;

            if (task != null)
            {
                var smartCollection = ((FolderItemViewModel)this.gridviewTasks.DataContext).SmartCollection;

                IFolder folder = task.Folder;
                if (folder.TaskGroup != TaskGroup.Action)
                {
                    Group<ITask> group = smartCollection.Items.FirstOrDefault(g => g.Contains(task));
                    if (group == this.currentTargetGroup)
                        return;

                    this.currentTargetGroup = group;

                    // find out the coordinates needed to display a visual indicator
                    var firstItem = this.currentTargetGroup[0];
                    var firstContainer = (GridViewItem)this.gridviewTasks.ContainerFromItem(firstItem);
                    if (firstContainer == null)
                        return;

                    var pA = firstContainer.TransformToVisual(this.dragDropHost).TransformPoint(new Point());

                    var lastItem = this.currentTargetGroup[this.currentTargetGroup.Count - 1];
                    var lastContainer = (GridViewItem)this.gridviewTasks.ContainerFromItem(lastItem);
                    if (lastContainer == null)
                        return;

                    var pB = lastContainer.TransformToVisual(this.dragDropHost).TransformPoint(new Point());

                    pB.X += lastContainer.ActualWidth;
                    pB.Y += lastContainer.ActualHeight;

                    var border = new Border
                    {
                        Background = this.highlightBrush,
                        Opacity = 0
                    };
                    border.AnimateOpacity(this.BackgroundIndicatorOpacity, TimeSpan.FromMilliseconds(DragDropHostAnimationTimeMs));

                    this.dragDropHost.Child = border;
                    border.Width = Math.Abs(pB.X - pA.X);
                    border.Height = this.dragDropHost.ActualHeight - 20;
                    border.VerticalAlignment = VerticalAlignment.Top;
                    border.HorizontalAlignment = HorizontalAlignment.Left;
                    border.Margin = new Thickness(Math.Abs(pA.X), Math.Abs(pA.Y), 0, 0);
                }
            }
        }

        private void OnGridViewDragLeave(object sender, DragEventArgs e)
        {
            this.StopGridDragOver();
        }

        private void StopGridDragOver()
        {
            if (this.dragDropHost.Child is FrameworkElement)
                ((FrameworkElement)this.dragDropHost.Child).AnimateOpacity(0.0, TimeSpan.FromMilliseconds(DragDropHostAnimationTimeMs), () => this.dragDropHost.Child = null);

            this.currentTargetGroup = null;
        }
    }
}

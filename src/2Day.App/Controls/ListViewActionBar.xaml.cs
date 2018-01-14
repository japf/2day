using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Chartreuse.Today.App.Tools;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Controls
{
    public sealed partial class ListViewActionBar : UserControl
    {
        private readonly List<ITask> targetTasks;
        private readonly FrameworkElement parentTarget;
        private List<ITask> currentTasks;

        public ListViewActionBar(List<ITask> targetTasks, FrameworkElement parentTarget)
        {
            this.InitializeComponent();

            this.targetTasks = targetTasks;
            this.parentTarget = parentTarget;

            this.btnComplete.Visibility = Visibility.Visible;
        }

        public ListViewActionBar()
        {
            this.InitializeComponent();
        }

        private void OnBtnCompleteClick(object sender, RoutedEventArgs e)
        {
            this.EnsureTasks();

            ModelHelper.SoftComplete(this.currentTasks);

            this.EndChanges();
        }

        private async void OnBtnDeleteClick(object sender, RoutedEventArgs e)
        {
            this.EnsureTasks();

            ModelHelper.SoftDelete(this.currentTasks);

            this.EndChanges();
        }

        private void OnBtnMoveClick(object sender, RoutedEventArgs e)
        {
            this.EnsureTasks();

            var content = new ListViewTaskEditorFlyout();
            content.MoveRequest += (s, e1) =>
            {
                var target = e1.Item;
                if (target.CanReceiveTasks)
                    target.MoveTasks(this.currentTasks);
            };

            this.ShowFlyout(content);
        }

        private void OnBtnPostponeClick(object sender, RoutedEventArgs e)
        {
            this.EnsureTasks();

            var picker = new DateTimePicker { NoDateSelectionOnLoad = true, ShowTime = false };
            picker.DateChanged += (s, e1) =>
            {
                DateTime? newDate = e1.Item;
                foreach (var selectedTask in this.currentTasks)
                {
                    selectedTask.SetDueAndAdjustReminder(newDate, this.currentTasks, true);
                }
            };

            this.ShowFlyout(picker);
        }

        private void ShowFlyout(UIElement content)
        {
            Flyout flyout = new Flyout
            {
                Content = content,
                FlyoutPresenterStyle = this.FindResource<Style>("ModernFlyoutPresenterStyle"),
                Placement = FlyoutPlacementMode.Full
            };
            flyout.Closed += (s1, e2) => { this.EndChanges(); };

            FrameworkElement flyoutHost = this.parentTarget;
            if (flyoutHost == null)
                flyoutHost = TreeHelper.FindVisualAncestor<SwipableListViewItem>(this);

            if (flyoutHost != null)
                flyout.ShowAt(flyoutHost);
        }

        private void EnsureTasks()
        {
            if (this.targetTasks != null)
            {
                this.currentTasks = this.targetTasks;
            }
            else
            {
                if (this.currentTasks == null)
                    this.currentTasks = new List<ITask>();
                else
                    this.currentTasks.Clear();

                if (this.DataContext is ITask)
                    this.currentTasks.Add((ITask)this.DataContext);
            }
        }

        private void EndChanges()
        {
            var root = this.parentTarget;
            if (root == null)
                root = this;

            var swipableListViewItem = TreeHelper.FindVisualAncestor<SwipableListViewItem>(root);
            if (swipableListViewItem != null)
                swipableListViewItem.Reset();

            this.TryCloseParentPopup();
        }
    }
}

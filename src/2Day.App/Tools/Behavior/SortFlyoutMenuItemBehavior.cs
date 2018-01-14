using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.App.Tools.Behavior
{
    public static class SortFlyoutMenuItemBehavior
    {
        private static readonly Dictionary<Button, MenuFlyout> menuFlyouts = new Dictionary<Button, MenuFlyout>(); 
        private static IMainPageViewModel mainViewModel;
        private static FolderItemViewModel selectedFolderItemViewModel;

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(SortFlyoutMenuItemBehavior), 
            new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as Button;
            if (button == null)
                throw new NotSupportedException("Attached type must be MenuFlyout");

            MenuFlyout menuFlyout = null;
            menuFlyouts.TryGetValue(button, out menuFlyout);

            if (menuFlyout != null)
            {
                foreach (var item in menuFlyout.Items.OfType<ToggleMenuFlyoutItem>())
                    item.Click -= OnMenuItemClick;

                menuFlyouts.Remove(button);
            }

            menuFlyout = new MenuFlyout();
            menuFlyouts[button] = menuFlyout;

            var taskGroups = new[]
            {
                TaskGroup.DueDate, 
                TaskGroup.Priority, 
                TaskGroup.Folder, 
                TaskGroup.Context, 
                TaskGroup.Progress, 
                TaskGroup.StartDate, 
                TaskGroup.Status,
                TaskGroup.Completed,
                TaskGroup.Modified
            };

            foreach (var taskGroup in taskGroups)
            {
                var menuItem = new ToggleMenuFlyoutItem
                {
                    Text = taskGroup.GetDescription(),
                    Tag = taskGroup
                };

                menuItem.Click += OnMenuItemClick;
                menuFlyout.Items.Add(menuItem);
            }

            menuFlyout.Items.Add(new MenuFlyoutSeparator());

            var orders = new[] { SortOrder.Ascending, SortOrder.Descending };
            foreach (var order in orders)
            {
                var menuItem = new ToggleMenuFlyoutItem
                {
                    Text = order.GetDescription(),
                    Tag = order
                };

                menuItem.Click += OnMenuItemClick;
                menuFlyout.Items.Add(menuItem);
            }

            button.Flyout = menuFlyout;

            // data context is not set at this point, fetch it when the button is clicked
            button.Click += OnButtonClick;
        }

        private static void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (mainViewModel == null)
            {
                mainViewModel = Ioc.Resolve<IMainPageViewModel>();
                mainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;

                selectedFolderItemViewModel = mainViewModel.SelectedFolderItem;
            }

            UpdateMenu();
        }

        private static void OnMenuItemClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ToggleMenuFlyoutItem menuItem = (ToggleMenuFlyoutItem) sender;
            if (menuItem.Tag is TaskGroup)
                selectedFolderItemViewModel.SelectedTaskGroup = (TaskGroup) menuItem.Tag;
            else if (menuItem.Tag is SortOrder)
                selectedFolderItemViewModel.SelectedTaskOrdering = (SortOrder)menuItem.Tag;

            UpdateMenu();
        }

        private static void OnMainViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedMenuItem")
            {
                if (selectedFolderItemViewModel != null)
                    selectedFolderItemViewModel.PropertyChanged -= FolderItemViewModelOnPropertyChanged;

                selectedFolderItemViewModel = mainViewModel.SelectedFolderItem;

                UpdateMenu();

                selectedFolderItemViewModel.PropertyChanged += FolderItemViewModelOnPropertyChanged;
            }
        }

        private static void FolderItemViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedTaskGroup" || e.PropertyName == "SelectedTaskOrdering")
            {
                UpdateMenu();
            }
        }

        private static void UpdateMenu()
        {
            foreach (var menuFlyout in menuFlyouts.Values)
            {
                foreach (var toogleMenuItem in menuFlyout.Items.OfType<ToggleMenuFlyoutItem>())
                {
                    if (toogleMenuItem.Tag is TaskGroup)
                    {
                        if (mainViewModel.SelectedFolderItem != null && mainViewModel.SelectedFolderItem.SelectedTaskGroup == (TaskGroup) toogleMenuItem.Tag)
                            toogleMenuItem.IsChecked = true;
                        else
                            toogleMenuItem.IsChecked = false;
                    }
                    else if (toogleMenuItem.Tag is SortOrder)
                    {
                        if (mainViewModel.SelectedFolderItem != null && mainViewModel.SelectedFolderItem.SelectedTaskOrdering == (SortOrder) toogleMenuItem.Tag)
                            toogleMenuItem.IsChecked = true;
                        else
                            toogleMenuItem.IsChecked = false;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
        }
    }
}

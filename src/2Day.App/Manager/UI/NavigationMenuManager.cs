using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.App.Manager.UI
{
    public class NavigationMenuManager
    {
        private readonly IWorkbook workbook;
        private readonly ISynchronizationManager synchronizationManager;
        private readonly IMainPageViewModel viewModel;
        private readonly ObservableCollection<MenuItemViewModel> menuItems;

        private int lastRemovedMenuItemIndex = -1;
        private bool trackReordering;
        private bool ignoreWorkbookOrder;
        private ListView navigationMenu;

        public NavigationMenuManager(IWorkbook workbook, ISynchronizationManager synchronizationManager, IMainPageViewModel viewModel)
        {
            if (workbook == null)
                throw new ArgumentNullException("workbook");
            if (synchronizationManager == null)
                throw new ArgumentNullException("synchronizationManager");
            if (viewModel == null)
                throw new ArgumentNullException("viewModel");

            this.workbook = workbook;
            this.synchronizationManager = synchronizationManager;
            this.viewModel = viewModel;
            this.menuItems = this.viewModel.MenuItems;

            this.workbook.FolderAdded += this.OnFolderAdded;
            this.workbook.FolderRemoved += this.OnFolderRemoved;
            this.workbook.FoldersReordered += this.OnFolderReordered;

            this.workbook.SmartViewAdded += this.OnSmartViewAdded;
            this.workbook.SmartViewRemoved += this.OnSmartViewRemoved;
            this.workbook.SmartViewsReordered += this.OnSmartViewReordered;

            this.workbook.ContextAdded += this.OnContextAdded;
            this.workbook.ContextRemoved += this.OnContextRemoved;
            this.workbook.ContextsReordered += this.OnContextReordered;

            this.workbook.TagAdded += this.OnTagAdded;
            this.workbook.TagRemoved += this.OnTagRemoved;
            this.workbook.TagsReordered += this.OnTagReordered;

            foreach (var view in this.workbook.Views)
            {
                view.PropertyChanged += this.OnViewPropertyChanged;
            }
            this.workbook.ViewsReordered += this.OnViewsReordered;

            this.viewModel.MenuItems.CollectionChanged += this.OnMenuItemsCollectionChanged;
        }

        public void AttachMenu(ListView menu)
        {
            if (menu == null)
                throw new ArgumentNullException("menu");

            if (this.navigationMenu != null)
            {
                this.navigationMenu.PointerExited -= this.NavigationMenuOnPointerExited;
                this.navigationMenu.PointerEntered -= this.NavigationMenuOnPointerEntered;
            }

            this.navigationMenu = menu;

            this.navigationMenu.PointerExited += this.NavigationMenuOnPointerExited;
            this.navigationMenu.PointerEntered += this.NavigationMenuOnPointerEntered;
        }
        
        private void NavigationMenuOnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.trackReordering = true;
        }

        private void NavigationMenuOnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            // event is sent when drag operation starts, so we check "IsInContact"
            // to prevent setting the flag to false when drag starts
            if (!e.Pointer.IsInContact)
            {
                this.trackReordering = false;    
            }
        }

        private void OnMenuItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!this.trackReordering || this.synchronizationManager.IsSyncRunning)
                return;

            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count == 1)
            {
                this.lastRemovedMenuItemIndex = e.OldStartingIndex;
            }

            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.Count == 1 && this.lastRemovedMenuItemIndex >= 0 && e.NewItems[0] is FolderItemViewModel)
            {
                var menuItem = ((FolderItemViewModel)e.NewItems[0]);
                var folder = menuItem.Folder;
                int index = e.NewStartingIndex;

                // check that insertion index is "correct" regarding the organization: views/folders/contexts
                bool isInsertionIndexValid = false;

                int lastView = this.LastIndexOf(f => f is ISystemView && f != folder);

                int firstSmartView = this.FirstIndexOf(f => f is ISmartView && f != folder);
                int lastSmartView = this.LastIndexOf(f => f is ISmartView && f != folder);

                int firstFolder = this.FirstIndexOf(f => f is IFolder && f != folder);
                int lastFolder = this.LastIndexOf(f => f is IFolder && f != folder);

                int firstContext = this.FirstIndexOf(f => f is IContext && f != folder);
                int lastContext = this.LastIndexOf(f => f is IContext && f != folder);

                int firstTag = this.FirstIndexOf(f => f is ITag && f != folder);
                int lastTag = this.LastIndexOf(f => f is ITag && f != folder);

                if (folder is ISystemView)
                    isInsertionIndexValid = lastView >= 0 && index <= lastView;
                else if (folder is ISmartView)
                    isInsertionIndexValid = index >= firstSmartView - 1 && index <= lastSmartView -1;
                else if (folder is IFolder)
                    isInsertionIndexValid = index >= firstFolder - 1 && index <= lastFolder;
                else if (folder is IContext)
                    isInsertionIndexValid = index >= firstContext - 1 && index <= lastContext;
                else if (folder is ITag)
                    isInsertionIndexValid = index >= firstTag - 1 && index <= lastTag;

                if (!isInsertionIndexValid)
                {
                    // use the dispatcher to postpone the move because we cannot modify a collection
                    // inside the CollectionChanged event
                    Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (this.lastRemovedMenuItemIndex >= 0 && this.lastRemovedMenuItemIndex < this.viewModel.MenuItems.Count)
                        {
                            this.viewModel.MenuItems.TryMove(index, this.lastRemovedMenuItemIndex);
                            this.lastRemovedMenuItemIndex = -1;
                        }
                    });
                }
                else
                {
                    this.ignoreWorkbookOrder = true;

                    if (folder is ISystemView)
                    {
                        // special case for view are those disabled are no in the menuItems collection
                        var view = (ISystemView)folder;
                        var views = this.workbook.Views.ToList();

                        int oldIndex = views.IndexOf(view);
                        int beforeIndex = this.viewModel.MenuItems.IndexOf(menuItem);
                        
                        views.RemoveAt(oldIndex);
                        views.Insert(beforeIndex, view);

                        this.workbook.ApplyViewOrder(views);
                    }
                    else if (folder is ISmartView)
                    {
                        this.workbook.ApplySmartViewOrder(this.viewModel.MenuItems.Where(f => f.Folder is ISmartView).Select(f => (ISmartView)f.Folder).ToList());
                    }
                    else if (folder is IFolder)
                    {
                        this.workbook.ApplyFolderOrder(this.viewModel.MenuItems.Where(f => f.Folder is IFolder).Select(f => (IFolder)f.Folder).ToList());
                    }
                    else if (folder is IContext)
                    {
                        this.workbook.ApplyContextOrder(this.viewModel.MenuItems.Where(f => f.Folder is IContext).Select(f => (IContext)f.Folder).ToList());
                    } 
                    else if (folder is ITag)
                    {
                        this.workbook.ApplyTagOrder(this.viewModel.MenuItems.Where(f => f.Folder is ITag).Select(f => (ITag)f.Folder).ToList());
                    }

                    this.ignoreWorkbookOrder = false;
                }
            }
        }

        private void OnFolderAdded(object sender, EventArgs<IFolder> e)
        {
            this.UpdateAbstractFolders(e.Item, null);
        }

        private void OnFolderRemoved(object sender, EventArgs<IFolder> e)
        {
            this.UpdateAbstractFolders(null, e.Item);
        }

        private void OnFolderReordered(object sender, EventArgs e)
        {
            if (!this.ignoreWorkbookOrder)
                this.UpdateAbstractFolders(null, null);
        }

        private void OnSmartViewAdded(object sender, EventArgs<ISmartView> e)
        {
            this.UpdateAbstractFolders(e.Item, null);
        }

        private void OnSmartViewRemoved(object sender, EventArgs<ISmartView> e)
        {
            this.UpdateAbstractFolders(null, e.Item);
        }

        private void OnSmartViewReordered(object sender, EventArgs e)
        {
            if (!this.ignoreWorkbookOrder)
                this.UpdateAbstractFolders(null, null);
        }

        private void OnViewsReordered(object sender, EventArgs e)
        {
            if (!this.ignoreWorkbookOrder)
                this.UpdateAbstractFolders(null, null);
        }

        private void OnContextRemoved(object sender, EventArgs<IContext> e)
        {
            this.UpdateAbstractFolders(null, e.Item);
        }

        private void OnContextAdded(object sender, EventArgs<IContext> e)
        {
            this.UpdateAbstractFolders(e.Item, null);
        }

        private void OnContextReordered(object sender, EventArgs e)
        {
            if (!this.ignoreWorkbookOrder)
                this.UpdateAbstractFolders(null, null);
        }

        private void OnTagAdded(object sender, EventArgs<ITag> e)
        {
            this.UpdateAbstractFolders(e.Item, null);
        }

        private void OnTagRemoved(object sender, EventArgs<ITag> e)
        {
            this.UpdateAbstractFolders(null, e.Item);
        }

        private void OnTagReordered(object sender, EventArgs e)
        {
            if (!this.ignoreWorkbookOrder)
                this.UpdateAbstractFolders(null, null);
        }

        private void OnViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEnabled")
            {
                var view = (ISystemView)sender;

                if (view.IsEnabled)
                    this.UpdateAbstractFolders(view, null);
                else
                    this.UpdateAbstractFolders(null, view);
            }
        }

        private void UpdateAbstractFolders(IAbstractFolder added, IAbstractFolder removed)
        {
            var sortedFolders = new List<IAbstractFolder>();
            sortedFolders.AddRange(this.workbook.Views.OrderBy(v => v.Order));
            sortedFolders.AddRange(this.workbook.SmartViews.OrderBy(v => v.Order));
            sortedFolders.AddRange(this.workbook.Folders.OrderBy(f => f.Order));
            sortedFolders.AddRange(this.workbook.Contexts.OrderBy(c => c.Order));
            sortedFolders.AddRange(this.workbook.Tags.OrderBy(t => t.Order));

            if (removed != null)
            {
                var target = this.GetFolderItemViewModel(removed);
                if (target != null)
                    this.menuItems.Remove(target);
            }
            else
            {
                if (added != null && this.menuItems.All(m => m.Folder != added))
                    this.menuItems.Add(new FolderItemViewModel(this.workbook, added));

                var sorted = sortedFolders
                    .Select(f => this.menuItems.FirstOrDefault(m => m.Folder == f))
                    .ToList();
                this.menuItems.ReorderFrom(sorted);
            }

            // check if we must update selected navigation menu item
            if (!this.menuItems.Contains(this.viewModel.SelectedMenuItem))
                this.viewModel.SelectedMenuItem = this.menuItems.OfType<FolderItemViewModel>().FirstOrDefault();

            // update separators
            this.RemoveSeparator(Constants.SeparatorSmartViewId);
            this.RemoveSeparator(Constants.SeparatorFolderId);
            this.RemoveSeparator(Constants.SeparatorContextId);
            this.RemoveSeparator(Constants.SeparatorTagId);
            
            bool group1 = this.workbook.Views.Any(v => v.IsEnabled);
            bool group2 = this.workbook.SmartViews.Any();
            bool group3 = this.workbook.Folders.Any();
            bool group4 = this.workbook.Contexts.Any();
            bool group5 = this.workbook.Tags.Any();

            if (group1 && (group2 || group3 || group4 || group5))
            {
                if (this.GetSeparatorIndex(Constants.SeparatorFolderId) < 0)
                    this.menuItems.Insert(this.LastIndexOf(f => f is IView && !(f is ITag) && !(f is ISmartView)) + 1, new SeparatorItemViewModel(Constants.SeparatorFolderId));
            }

            if (group2 && (group3 || group4 || group5))
            {
                if (this.GetSeparatorIndex(Constants.SeparatorSmartViewId) < 0)
                    this.menuItems.Insert(this.LastIndexOf(f => f is ISmartView) + 1, new SeparatorItemViewModel(Constants.SeparatorSmartViewId));
            }

            if (group3 && (group4 || group5))
            {
                if (this.GetSeparatorIndex(Constants.SeparatorContextId) < 0)
                    this.menuItems.Insert(this.LastIndexOf(f => f is IFolder) + 1, new SeparatorItemViewModel(Constants.SeparatorContextId));
            }

            if (group4 && group5)
            {
                if (this.GetSeparatorIndex(Constants.SeparatorTagId) < 0)
                    this.menuItems.Insert(this.LastIndexOf(f => f is IContext) + 1, new SeparatorItemViewModel(Constants.SeparatorTagId));
            }
        }

        private int FirstIndexOf(Func<IAbstractFolder, bool> selector)
        {
            var item = this.menuItems.FirstOrDefault(m => selector(m.Folder));
            if (item != null)
                return this.menuItems.IndexOf(item);
            
            return -1;
        }

        private int LastIndexOf(Func<IAbstractFolder, bool> selector)
        {
            var item = this.menuItems.LastOrDefault(m => selector(m.Folder));
            if (item != null)
                return this.menuItems.IndexOf(item);

            return -1;
        }

        private FolderItemViewModel GetFolderItemViewModel(IAbstractFolder folder)
        {
            return this.menuItems.FirstOrDefault(vm => vm.Folder == folder) as FolderItemViewModel;
        }

        private void RemoveSeparator(string separatorName)
        {
            var separator = this.menuItems.OfType<SeparatorItemViewModel>().FirstOrDefault(s => s.Name.Equals(separatorName, StringComparison.OrdinalIgnoreCase));
            if (separator != null)
                this.menuItems.Remove(separator);
        }

        private int GetSeparatorIndex(string separatorName)
        {
            return this.menuItems.IndexOf(this.menuItems.OfType<SeparatorItemViewModel>().FirstOrDefault(vm => vm.Name == separatorName));
        }
    }
}
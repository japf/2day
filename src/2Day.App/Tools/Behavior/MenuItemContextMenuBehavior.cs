using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Dialog;
using Chartreuse.Today.Core.Shared.Tools.Navigation;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Manager;

namespace Chartreuse.Today.App.Tools.Behavior
{
    public static class NavMenuItemFlyoutBehavior
    {
        public static bool GetIsContextMenuEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsContextMenuEnabledProperty);
        }

        public static void SetIsContextMenuEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsContextMenuEnabledProperty, value);
        }

        public static readonly DependencyProperty IsContextMenuEnabledProperty = DependencyProperty.RegisterAttached(
            "IsContextMenuEnabled",
            typeof(bool),
            typeof(NavMenuItemFlyoutBehavior),
            new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uiElement = d as FrameworkElement;
            if (uiElement != null)
                uiElement.RightTapped += OnElementRightTapped;
        }
        
        private static void AddMenuItem(MenuFlyout menu, string text)
        {
            var menuFlyoutItem = new MenuFlyoutItem { Text = text };
            menuFlyoutItem.Click += OnMenuItemClicked;

            menu.Items.Add(menuFlyoutItem);
        }

        private static async void OnMenuItemClicked(object sender, RoutedEventArgs e)
        {
            string text = ((MenuFlyoutItem)sender).Text;
            var abstractFolder = ((FolderItemViewModel)((FrameworkElement)sender).DataContext).Folder;

            var mbs = Ioc.Resolve<IMessageBoxService>();
            var workbook = Ioc.Resolve<IWorkbook>();
            var tileManager = Ioc.Resolve<ITileManager>();

            if (text == StringResources.AppMenu_ContextMenu_Folder_Edit)
            {
                // edit
                if (abstractFolder is ISmartView)
                {
                    Ioc.Resolve<INavigationService>().FlyoutTo(typeof(CreateEditSmartViewPage), abstractFolder);
                }
                else if (abstractFolder is IFolder)
                {
                    Ioc.Resolve<INavigationService>().FlyoutTo(ViewLocator.CreateEditFolderPage, abstractFolder);
                }
                else if (abstractFolder is IContext)
                {
                    string newName = await mbs.ShowCustomTextEditDialogAsync(StringResources.EditContext_Title, StringResources.EditContext_Placeholder, abstractFolder.Name);
                    var context = (IContext)abstractFolder;
                    context.TryRename(workbook, newName);
                }
                else if (abstractFolder is ITag)
                {
                    string newName = await mbs.ShowCustomTextEditDialogAsync(StringResources.EditTag_Title, StringResources.EditContext_Placeholder, abstractFolder.Name);
                    var tag = (ITag)abstractFolder;
                    tag.TryRename(workbook, newName);
                }
            }
            else if (text == StringResources.AppMenu_ContextMenu_Folder_Unpin || text == StringResources.AppMenu_ContextMenu_Folder_Pin)
            {
                if (tileManager.IsPinned(abstractFolder))
                {
                    await tileManager.UnpinAsync(abstractFolder);
                }
                else
                {
                    await tileManager.PinAsync(abstractFolder);
                }
            }
            else if (text == StringResources.AppMenu_ContextMenu_Hide)
            {
                if (abstractFolder is ISystemView)
                {
                    ((ISystemView)abstractFolder).IsEnabled = false;
                }
            }
            else if (text == StringResources.AppMenu_ContextMenu_Folder_Delete)
            {
                // delete
                if (abstractFolder is IFolder)
                {
                    var result = await mbs.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_DeleteFolderText, DialogButton.YesNo);
                    if (result == DialogResult.Yes)
                        workbook.RemoveFolder(abstractFolder.Name);
                }
                else if (abstractFolder is IContext)
                {
                    var result = await mbs.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_DeleteContextText, DialogButton.YesNo);
                    if (result == DialogResult.Yes)
                        workbook.RemoveContext(abstractFolder.Name);
                }
                else if (abstractFolder is ISmartView)
                {
                    var result = await mbs.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_DeleteSmartViewText, DialogButton.YesNo);
                    if (result == DialogResult.Yes)
                        workbook.RemoveSmartView(abstractFolder.Name);
                }
                else if (abstractFolder is ITag)
                {
                    var result = await mbs.ShowAsync(StringResources.Dialog_TitleConfirmation, StringResources.Dialog_DeleteTagText, DialogButton.YesNo);
                    if (result == DialogResult.Yes)
                        workbook.RemoveTag(abstractFolder.Name);
                }
            }
        }

        private static void OnElementRightTapped(object sender, RightTappedRoutedEventArgs rightTappedRoutedEventArgs)
        {
            if (sender is FrameworkElement && ((FrameworkElement)sender).DataContext is FolderItemViewModel)
            {
                var frameworkElement = (FrameworkElement)sender;

                var abstractFolder = ((FolderItemViewModel)frameworkElement.DataContext).Folder;

                var menu = new MenuFlyout();
                var tileManager = Ioc.Resolve<ITileManager>();

                if (!(abstractFolder is ISystemView))
                    AddMenuItem(menu, StringResources.AppMenu_ContextMenu_Folder_Edit);

                if (tileManager.IsPinned(abstractFolder))
                    AddMenuItem(menu, StringResources.AppMenu_ContextMenu_Folder_Unpin);
                else
                    AddMenuItem(menu, StringResources.AppMenu_ContextMenu_Folder_Pin);

                if (abstractFolder is IFolder || abstractFolder is IContext || abstractFolder is ISmartView || abstractFolder is ITag)
                    AddMenuItem(menu, StringResources.AppMenu_ContextMenu_Folder_Delete);

                if (abstractFolder is ISystemView)
                    AddMenuItem(menu, StringResources.AppMenu_ContextMenu_Hide);

                menu.ShowAt(frameworkElement);
            }
        }
    }
}

using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class MenuNavigateShorcut : KeyboardShortcutBase
    {
        public MenuNavigateShorcut(Frame rootFrame, INavigationService navigationService)
            : base(rootFrame, navigationService)
        {
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            return this.IsModifierKeyDown(VirtualKey.Control) 
                   && (e.Key == VirtualKey.PageUp || e.Key == VirtualKey.PageDown)
                   && this.GetCurrentPageDataContextAs<MainPageViewModel>() != null;
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            var viewmodel = this.GetCurrentPageDataContextAs<MainPageViewModel>();
            if (viewmodel != null && viewmodel.SelectedMenuItem != null)
            {
                int currentIndex = viewmodel.MenuItems.IndexOf(viewmodel.SelectedMenuItem);
                int newIndex = e.Key == VirtualKey.PageDown ? currentIndex + 1 : currentIndex - 1;
                if (newIndex >= viewmodel.MenuItems.Count)
                {
                    newIndex = 0;
                }
                else if (newIndex < 0)
                {
                    newIndex = viewmodel.MenuItems.Count - 1;
                }
                else if (viewmodel.MenuItems[newIndex] is SeparatorItemViewModel)
                {
                    newIndex = e.Key == VirtualKey.PageDown ? newIndex + 1 : newIndex - 1;
                    if (newIndex >= viewmodel.MenuItems.Count)
                        newIndex = 0;
                }

                viewmodel.SelectedMenuItem = viewmodel.MenuItems[newIndex];

                return true;
            }

            return false;
        }
    }
}
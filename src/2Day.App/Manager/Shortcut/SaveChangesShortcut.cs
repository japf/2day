using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class SaveChangesShortcut : KeyboardShortcutBase
    {
        public SaveChangesShortcut(Frame rootFrame, INavigationService navigationService)
            : base(rootFrame, navigationService)
        {
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            return e.Key == VirtualKey.S && this.IsModifierKeyDown(VirtualKey.Control) && !this.IsModifierKeyDown(VirtualKey.LeftMenu) && !this.IsModifierKeyDown(VirtualKey.RightMenu);
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            // try to find quick add first
            PageViewModelBase pageViewModel = this.GetCurrentPageDataContextAs<QuickAddTaskViewModel>();
            if (pageViewModel == null)
                pageViewModel = this.GetCurrentPageDataContextAs<PageViewModelBase>();

            if (pageViewModel != null)
            {
                pageViewModel.SaveCommand.Execute(null);
                return true;
            }

            return false;
        }
    }
}
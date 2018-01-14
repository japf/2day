using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class AddSubTaskShortcut : KeyboardShortcutBase
    {
        public AddSubTaskShortcut(Frame rootFrame, INavigationService navigationService) 
            : base(rootFrame, navigationService)
        {
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            return (e.Key == VirtualKey.Enter && this.IsModifierKeyDown(VirtualKey.Shift))
                   && this.GetCurrentPageAs<TaskPage>() != null;
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            var taskPage = this.GetCurrentPageAs<TaskPage>();
            if (taskPage != null)
            {
                taskPage.OpenAddSubtaskPopup();
                return true;
            }

            return false;
        }
    }
}

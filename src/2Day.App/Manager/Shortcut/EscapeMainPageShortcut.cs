using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Controls;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    /// <summary>
    /// A shortcut that handles Esc key on main page. It either close full screen dialog
    /// or clear active selection
    /// </summary>
    public class EscapeMainPageShortcut : KeyboardShortcutBase
    {
        public EscapeMainPageShortcut(Frame rootFrame, INavigationService navigationService)
            : base(rootFrame, navigationService)
        {
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            return ((e.Key == VirtualKey.Back && !this.IsFromTextControl(e)) || e.Key == VirtualKey.Escape)
                   && this.GetCurrentPageAs<MainPage>() != null;
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            var page = this.GetCurrentPageAs<MainPage>();
            if (page != null)
            {
                var customDialog = TreeHelper.FindVisualChild<FullScreenDialog>(page);

                if (customDialog != null)
                {
                    customDialog.Close();
                }
                else
                {
                    bool handled = false;
                    var taskPage = this.GetCurrentPageAs<TaskPage>();
                    if (taskPage != null)
                    {
                        handled = taskPage.IsHandlingEscapeKey();
                    }

                    if (!handled)
                    {
                        this.navigationService.CloseFlyouts();
                        page.SelectionManager.ClearSelection();
                    }
                }

                return true;
            }

            return false;
        }
    }
}
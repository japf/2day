using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class StartSpeechRecognitionShortcut : KeyboardShortcutBase
    {
        public StartSpeechRecognitionShortcut(Frame rootFrame, INavigationService navigationService)
            : base(rootFrame, navigationService)
        {
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            return !this.IsFromTextControl(e) && 
                ((e.Key == VirtualKey.E && this.IsModifierKeyDown(VirtualKey.Control))
                && (this.GetCurrentPageDataContextAs<MainPageViewModel>() != null || this.GetCurrentPageDataContextAs<TaskViewModelBase>() != null));
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            MainPageViewModel mainPageViewModel = this.GetCurrentPageDataContextAs<MainPageViewModel>();
            TaskViewModelBase taskViewModel = this.GetCurrentPageDataContextAs<TaskViewModelBase>();

            if (mainPageViewModel != null)
            {
                mainPageViewModel.SpeechCommand.Execute(null);
                return true;
            }
            else if (taskViewModel != null)
            {
                taskViewModel.StartSpeechTitleCommand.Execute(null);
                return true;
            }

            return false;
        }
    }
}
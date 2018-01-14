using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class CreateTaskShortcut : KeyboardShortcutBase
    {
        private readonly IWorkbook workbook;

        public CreateTaskShortcut(Frame rootFrame, INavigationService navigationService, IWorkbook workbook)
            : base(rootFrame, navigationService)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            this.workbook = workbook;
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            return (e.Key == VirtualKey.N && this.IsModifierKeyDown(VirtualKey.Control)) && this.GetCurrentPageDataContextAs<MainPageViewModel>() != null;
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            var viewmodel = this.GetCurrentPageDataContextAs<MainPageViewModel>();
            if (viewmodel != null)
            {
                this.navigationService.FlyoutTo(ViewLocator.CreateEditTaskPageNew, viewmodel.SelectedFolder.GetTaskCreationParameters());
                return true;
            }

            return false;
        }
    }
}
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.App.VoiceCommand;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App
{
    internal class CortanaRuntimeAction : ICortanaRuntimeActions
    {
        public void SelectFolder(IAbstractFolder folder)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.Content is MainPage)
            {
                var mainpage = (MainPage)rootFrame.Content;
                var viewmodel = (MainPageViewModel)mainpage.DataContext;
                viewmodel.SelectedMenuItem = viewmodel.MenuItems.FirstOrDefault(m => m.Folder == folder);
            }
        }
    }
}

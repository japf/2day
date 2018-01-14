using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public interface IMainPageViewModel : INotifyPropertyChanged
    {
        string SearchText { get; set; }

        ObservableCollection<MenuItemViewModel> MenuItems { get; }
        MenuItemViewModel SelectedMenuItem { get; set; }
        FolderItemViewModel SelectedFolderItem { get; }

        Task RefreshAsync();
    }
}
using System.Windows.Input;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public interface IPageViewModel
    {
        bool IsBusy { get; }
        string BusyText { get; }
        ICommand GoBackCommand { get; }
    }
}

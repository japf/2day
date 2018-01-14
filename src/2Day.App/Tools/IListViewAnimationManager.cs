using Windows.UI.Xaml;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.Tools
{
    public interface IListViewAnimationManager
    {
        bool HandleManipulationStarting(FrameworkElement originalSource);
        bool OpenFlyout(ITask task);
    }
}
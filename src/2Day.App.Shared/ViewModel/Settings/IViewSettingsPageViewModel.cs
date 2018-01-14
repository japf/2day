using System.Collections.Specialized;

namespace Chartreuse.Today.App.Shared.ViewModel.Settings
{
    public interface IViewSettingsPageViewModel
    {
        INotifyCollectionChanged CollectionView { get; }
    }
}
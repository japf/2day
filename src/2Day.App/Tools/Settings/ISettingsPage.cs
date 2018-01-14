
namespace Chartreuse.Today.App.Tools.Settings
{
    public interface ISettingsPage
    {
        SettingsSizeMode Size { get; }

        void OnNavigatedTo(object parameter);
    }
}

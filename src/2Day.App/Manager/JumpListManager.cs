using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.StartScreen;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.LaunchArguments;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.Manager
{
    public static class JumpListManager
    {
        private const int DefinitionVersion = 1;
        private const string SettingKey = "JumpListDefinitionVersion";

        public static async Task SetupJumpListAsync()
        {
            if (JumpList.IsSupported())
            {
                try
                {
                    var localSettings = ApplicationData.Current.LocalSettings;

                    bool isDefinitionVersionObsolete = !localSettings.Values.ContainsKey(SettingKey) || (int)localSettings.Values[SettingKey] != DefinitionVersion;
#if DEBUG
                    isDefinitionVersionObsolete = true;
#endif
                    if (isDefinitionVersionObsolete)
                    {
                        localSettings.Values[SettingKey] = DefinitionVersion;

                        var jumplist = await JumpList.LoadCurrentAsync();
                        jumplist.Items.Clear();

                        var quickAdd = JumpListItem.CreateWithArguments(LaunchArgumentsHelper.QuickAddTask, StringResources.Tile_QuickAdd);
                        quickAdd.Logo = new Uri("ms-appx:///Assets/JumpList.Add.png");
                        jumplist.Items.Add(quickAdd);

                        var sync = JumpListItem.CreateWithArguments(LaunchArgumentsHelper.Sync, StringResources.Settings_TitleSync);
                        sync.Logo = new Uri("ms-appx:///Assets/JumpList.Sync.png");
                        jumplist.Items.Add(sync);

                        await jumplist.SaveAsync();
                    }
                }
                catch (Exception e)
                {
                    TrackingManagerHelper.Exception(e, "Exception will setting up jumplist");
                }
             }
        }
    }
}

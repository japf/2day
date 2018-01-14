using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Core.Universal.Manager;
using Chartreuse.Today.Core.Universal.Model;
using System;
using Windows.UI.Xaml.Controls;

namespace Chartreuse.Today.App.Tools
{
    public static class PageExtension
    {
        public static void SetRequestedTheme(this Page page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            try
            {
                bool useDarkTheme = WinSettings.Instance.GetValue<bool>(CoreSettings.UseDarkTheme);
                page.RequestedTheme = useDarkTheme ? Windows.UI.Xaml.ElementTheme.Dark : Windows.UI.Xaml.ElementTheme.Light;
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, $"Error while setting RequestedTheme");
            }            
        }
    }
}

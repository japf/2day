using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;

namespace Chartreuse.Today.Core.Shared.Resources
{
    public static class ResourcesLocator
    {
        private const string FolderIconPathFormat = "ms-appx:///Resources/Icons/Folder/img{0}-hi.png";

        private static readonly string[] patterns;
        private static readonly string[] readablePatterns;

        public const string TagViewColor = "transparent";

        public static string ViewColor
        {
            get; private set;
        }

        public static string ContextColor
        {
            get; private set;
        }

        static ResourcesLocator()
        {
            patterns = new[]
                {
                    "asfalt", "binding_dark", "black-Linen", "broken_noise", "dark_geometric",
                    "dark_mosaic", "debut_dark", "navy_blue", "noisy_net", "office",
                    "skewed_print", "txture", "wild_oliva"
                };


            readablePatterns = patterns.Select(p =>
            {
                string noSpecialChar = p.Replace("-", " ").Replace("_", " ");
                return noSpecialChar.Substring(0, 1).ToUpper() + noSpecialChar.Substring(1).ToLower();
            }).ToArray();
        }

        public static string GetAppIconPng()
        {
            return string.Format(FolderIconPathFormat, "00");
        }

        public static string GetFolderIconPng(IAbstractFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            return string.Format(FolderIconPathFormat, folder.IconId.ToString("00"));
        }

        public static string BuildPatternPath(string imageName, bool useDarkTheme)
        {
            int index = readablePatterns.IndexOf(p => p.Equals(imageName, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                string name = patterns[index];
                if (!useDarkTheme)
                    name += "-light";

                return string.Format("/Resources/Icons/Patterns/{0}.png", name);
            }
            else
            {
                return string.Empty;
            }
        }

        public static IEnumerable<string> Patterns
        {
            get { return readablePatterns; }
        }

        public static Uri syncExchangeIconUri;
        public static Uri syncOffice365IconUri;
        public static Uri syncToodleDoIconUri;
        public static Uri syncVercorsIconUri;
        public static Uri syncOutlookIconUri;
        public static Uri syncActiveSyncIconUri;

        public static Uri SyncAdvancedModeReplace;
        public static Uri SyncAdvancedModeDiag;
        public static Uri SyncAdvancedModeDelete;

        private static string uriPrefix;
        private static UriKind uriKind;

        public static void Initialize(string prefix, UriKind kind)
        {
            uriPrefix = prefix;
            uriKind = kind;

            syncExchangeIconUri = GetUri("/Resources/Icons/Sync/microsoft-exchange-logo.png");
            syncOffice365IconUri = GetUri("/Resources/Icons/Sync/microsoft-office365-logo.png");
            syncToodleDoIconUri = GetUri("/Resources/Icons/Sync/toodledo-logo.png");
            syncVercorsIconUri = GetUri("/Resources/Icons/Sync/vercors-cloud.png");
            syncOutlookIconUri = GetUri("/Resources/Icons/Sync/outlook-logo.png");
            syncActiveSyncIconUri = GetUri("/Resources/Icons/Sync/activesync-logo.png");

            SyncAdvancedModeReplace = GetUri("/Resources/Icons/Sync/sync-mode-replace.png");
            SyncAdvancedModeDiag = GetUri("/Resources/Icons/Sync/sync-mode-diagnostic.png");
            SyncAdvancedModeDelete = GetUri("/Resources/Icons/Sync/sync-mode-delete.png");

            // set to dark theme for now, a call to UpdateTheme can change that later on
            UpdateTheme(false);
        }

        public static void UpdateTheme(bool useLightTheme)
        {
            string suffixDark = ".png";

            if (useLightTheme)
            {
                suffixDark = "-dark.png";
            }

            SyncAdvancedModeReplace = GetUri("/Resources/Icons/Sync/sync-mode-replace" + suffixDark);
            SyncAdvancedModeDiag = GetUri("/Resources/Icons/Sync/sync-mode-diagnostic" + suffixDark);
            SyncAdvancedModeDelete = GetUri("/Resources/Icons/Sync/sync-mode-delete" + suffixDark);

            if (!useLightTheme)
            {
                ViewColor = "white";
                ContextColor = "white";
            }
            else
            {
                ViewColor = "#DE000000";
                ContextColor = "#DE000000";
            }
        }

        public static string GetSyncServiceIcon(SynchronizationService service, string serverInfo = null)
        {
            switch (service)
            {
                case SynchronizationService.None:
                    return string.Empty;
                case SynchronizationService.ToodleDo:
                    return syncToodleDoIconUri.ToString();
                case SynchronizationService.Exchange:
                case SynchronizationService.ExchangeEws:
                    if (!string.IsNullOrEmpty(serverInfo) && serverInfo.ToLower().Contains("office365"))
                        return syncOffice365IconUri.ToString();
                    else
                        return syncExchangeIconUri.ToString();
                case SynchronizationService.OutlookActiveSync:
                    return syncOutlookIconUri.ToString();
                case SynchronizationService.ActiveSync:
                    return syncActiveSyncIconUri.ToString();
                case SynchronizationService.Vercors:
                    return syncVercorsIconUri.ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Uri GetUri(string path)
        {
            return SafeUri.Get(uriPrefix + path, uriKind);
        }
    }
}

using System;

namespace Chartreuse.Today.Core.Shared
{
    public static class Constants
    {
        public const string AppName = "2Day";

        public const string SeparatorSmartViewId = "smartview";
        public const string SeparatorFolderId = "folder";
        public const string SeparatorContextId = "context";
        public const string SeparatorTagId = "tag";

        public const string SyncPrimitiveBackgroundEvent = "2DaySyncPrimitiveBackgroundEvent";
        public const string SyncPrimitiveAppRunningForeground = "2DaySyncPrimitiveAppForeground";

        public const int AppColorR = 0x28;
        public const int AppColorG = 0x9F;
        public const int AppColorB = 0xDA;

        public const double AppMinWidth = 350.0;
        public const double AppMinHeight = 700.0;
        
        public const string WindowsChangelogAddress = "https://www.2day-app.com/updates/windows-10";

        public const string SupportEmailAddress = "support@2day-app.com";

        public const string AzureExchangeServiceAdress = "https://2day-exchange.azurewebsites.net/";
        public const string Office365Endpoint = "https://outlook.office365.com";

        public const string HelpPageAddress = "https://www.2day-app.com/help";
        public const string HelpPageExchangeAddress = "https://www.2day-app.com/help/exchange-setup";
        public const string HelpPageChooseSyncAddress = "https://www.2day-app.com/help/choosing-sync";
        public const string HelpPageSmartViewAddress = "https://www.2day-app.com/help/smart-views";
        public const string HelpPageFixSyncIssues = "https://www.2day-app.com/help/fix-common-issues";

        public const string GitHubAddress = "https://github.com/2DayApp/2day";

        public const string WebSiteAddress = "https://www.2day-app.com";

        public const char TagSeparator = ',';

        public const string WinCollectionUIExceptionContent = "0x8000FFFF (E_UNEXPECTED)";

        public const string LiveAccountCreateAppPassword = "https://account.live.com/proofs/Manage";

        public const string VercorsServiceUri = "https://vercors.azure-mobile.net/";
        public const string VercorsApplicationKey = "VBxVNqNGPdqXESbNMHSinszsEudMQP64";
        
        public const string DefaultLoginInfo = "...";
        
        public const string PackageFamilyName = "62844chartreuse.2Day_j6whz2c319cty";
        
        public static string MarketplaceReviewAppUri
        {
            get { return string.Format("ms-windows-store://REVIEW?PFN={0}", PackageFamilyName); }
        }

        public static string MarketplaceOpenAppUri
        {
            get { return string.Format("ms-windows-store://PDP?PFN={0}", PackageFamilyName); }
        }
    }
}

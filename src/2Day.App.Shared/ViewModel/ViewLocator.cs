using System;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public static class ViewLocator
    {
        public static Type CreateEditFolderPage { get; set; }
        public static Type CreateEditTaskPageNew { get; set; }
        public static Type EditNotesPage { get; set; }
        public static Type CreateEditSmartViewPage { get; set; }

        public static Type SyncToodleDoSettingsPage { get; set; }
        public static Type SyncExchangeSettingsPage { get; set; }
        public static Type SyncOutlookActiveSyncSettingsPage { get; set; }
        public static Type SyncActiveSyncSettingsPage { get; set; }
        public static Type SyncVercorsSettingsPage { get; set; }
        public static Type SyncAdvancedSyncSettingsPage { get; set; }

        public static Type WelcomePage { get; set; }

        public static Type SettingsGeneralPage { get; set; }
        public static Type SettingsDisplayPage { get; set; }
        public static Type SettingsViewsPage { get; set; }
        public static Type SettingsFoldersPage { get; set; }
        public static Type SettingsSmartViewsPage { get; set; }
        public static Type SettingsContextsPage { get; set; }
        public static Type SettingsTaskOrderingPage { get; set; }
        public static Type SettingsSyncPage { get; set; }
        public static Type SettingsBackupPage { get; set; }
        public static Type SettingsMiscPage { get; set; }
        public static Type SettingsAboutPage { get; set; }  
        
        public static Type DebugPage { get; set; } 
        public static Type LogViewerSettingsPage { get; set; } 
    }
}

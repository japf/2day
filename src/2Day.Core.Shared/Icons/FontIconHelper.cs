using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Chartreuse.Today.Core.Shared.Icons
{
    public static class FontIconHelper
    {
        private static readonly Dictionary<AppIconType, FontIconDescriptor> iconDescriptors;

        private static readonly List<AppIconType> folderIcons;
        private static readonly List<AppIconType> viewIcons;
        private static readonly List<AppIconType> priorityIcons;

        public const int FolderIconsCount = 42;

        private const int ViewIconIndexStart = 90;

        public static readonly int IconIdPriorityNone =     1000;
        public static readonly int IconIdPriorityLow =      1001;
        public static readonly int IconIdPriorityMedium =   1002;
        public static readonly int IconIdPriorityHigh =     1003;
        public static readonly int IconIdPriorityStar =    1004;

        public static readonly int IconIdContext = 14;
        public static readonly int IconIdCloud = 11;
        public static readonly int IconIdFolder = 8;
        public static readonly int IconIdTag = 42;
        public static readonly int IconIdMagic = 26;

        public static readonly int FolderTodayIconId = 91;
        public static readonly int FolderNextWeekIconId = 92;
        public static readonly int FolderAllIconId = 93;
        public static readonly int FolderCompletedIconId = 94;
        public static readonly int FolderStarredIconId = 95;
        public static readonly int FolderNoDateIconId = 96;
        public static readonly int FolderTomorrowIconId = 97;
        public static readonly int FolderReminderIconId = 98;
        public static readonly int FolderLateIconId = 99;
        public static readonly int FolderStartDateIconId = 100;
        public static readonly int FolderNonCompletedIconId = 101;
        public static readonly int FolderToSyncIconId = 102;

        static FontIconHelper()
        {
            folderIcons = new List<AppIconType>
            {
                AppIconType.Folder01,
                AppIconType.Folder02,
                AppIconType.Folder03,
                AppIconType.Folder04,
                AppIconType.Folder05,
                AppIconType.Folder06,
                AppIconType.Folder07,
                AppIconType.Folder08,
                AppIconType.Folder09,
                AppIconType.Folder10,
                AppIconType.Folder11,
                AppIconType.Folder12,
                AppIconType.Folder13,
                AppIconType.Folder14,
                AppIconType.Folder15,
                AppIconType.Folder16,
                AppIconType.Folder17,
                AppIconType.Folder18,
                AppIconType.Folder19,
                AppIconType.Folder20,
                AppIconType.Folder21,
                AppIconType.Folder22,
                AppIconType.Folder23,
                AppIconType.Folder24,
                AppIconType.Folder25,
                AppIconType.Folder26,
                AppIconType.Folder27,
                AppIconType.Folder28,
                AppIconType.Folder29,
                AppIconType.Folder30,
                AppIconType.Folder31,
                AppIconType.Folder32,
                AppIconType.Folder33,
                AppIconType.Folder34,
                AppIconType.Folder35,
                AppIconType.Folder36,
                AppIconType.Folder37,
                AppIconType.Folder38,
                AppIconType.Folder39,
                AppIconType.Folder40,
                AppIconType.Folder41,
                AppIconType.Folder42,
            };

            viewIcons = new List<AppIconType>
            {
                AppIconType.View01,
                AppIconType.View02,
                AppIconType.View03,
                AppIconType.View04,
                AppIconType.View05,
                AppIconType.View06,
                AppIconType.View07,
                AppIconType.View08,
                AppIconType.View09,
                AppIconType.View10,
                AppIconType.View11,
                AppIconType.View12,
                AppIconType.View13
            };

            priorityIcons = new List<AppIconType>
            {
                AppIconType.PriorityNone,  
                AppIconType.PriorityLow,   
                AppIconType.PriorityMedium,
                AppIconType.PriorityHigh,  
                AppIconType.PriorityStar,  
            };

            iconDescriptors = new Dictionary<AppIconType, FontIconDescriptor>
            {
                { AppIconType.LogoNoText,               new FontIconDescriptor(FontIconGroup.Logo,       0) },
                { AppIconType.LogoTextHorizontal,       new FontIconDescriptor(FontIconGroup.Logo,       1) },
                { AppIconType.LogoText,                 new FontIconDescriptor(FontIconGroup.Logo,       2) },

                { AppIconType.CommonAdd,                new FontIconDescriptor(FontIconGroup.Common,     0) },
                { AppIconType.CommonAlarm,              new FontIconDescriptor(FontIconGroup.Common,     1) },
                { AppIconType.CommonAlarmDisabled,      new FontIconDescriptor(FontIconGroup.Common,     2) },
                { AppIconType.CommonLeft,               new FontIconDescriptor(FontIconGroup.Common,     3) },
                { AppIconType.CommonRight,              new FontIconDescriptor(FontIconGroup.Common,     4) },
                { AppIconType.CommonPrevious,           new FontIconDescriptor(FontIconGroup.Common,     5) },
                { AppIconType.CommonNext,               new FontIconDescriptor(FontIconGroup.Common,     6) },
                { AppIconType.CommonDelete,             new FontIconDescriptor(FontIconGroup.Common,     7) },
                { AppIconType.CommonShare,              new FontIconDescriptor(FontIconGroup.Common,     8) },
                { AppIconType.CommonEdit,               new FontIconDescriptor(FontIconGroup.Common,     9) },
                { AppIconType.CommonMove,               new FontIconDescriptor(FontIconGroup.Common,     10) },
                { AppIconType.CommonPin,                new FontIconDescriptor(FontIconGroup.Common,     11) },
                { AppIconType.CommonUnpin,              new FontIconDescriptor(FontIconGroup.Common,     12) },
                { AppIconType.CommonMic,                new FontIconDescriptor(FontIconGroup.Common,     13) },
                { AppIconType.CommonConfigure,          new FontIconDescriptor(FontIconGroup.Common,     14) },
                { AppIconType.CommonPhone,              new FontIconDescriptor(FontIconGroup.Common,     15) },
                { AppIconType.CommonMail,               new FontIconDescriptor(FontIconGroup.Common,     16) },
                { AppIconType.CommonSms,                new FontIconDescriptor(FontIconGroup.Common,     17) },
                { AppIconType.CommonHide,               new FontIconDescriptor(FontIconGroup.Common,     18) },
                { AppIconType.CommonEdge,               new FontIconDescriptor(FontIconGroup.Common,     19) },
                { AppIconType.CommonTwitter,            new FontIconDescriptor(FontIconGroup.Common,     20) },
                { AppIconType.CommonFacebook,           new FontIconDescriptor(FontIconGroup.Common,     21) },
                { AppIconType.CommonUserVoice,          new FontIconDescriptor(FontIconGroup.Common,     22) },
                { AppIconType.CommonStore,              new FontIconDescriptor(FontIconGroup.Common,     23) },
                { AppIconType.CommonRefresh,            new FontIconDescriptor(FontIconGroup.Common,     24) },
                { AppIconType.CommonQuestion,           new FontIconDescriptor(FontIconGroup.Common,     25) },
                { AppIconType.CommonQuickAdd,           new FontIconDescriptor(FontIconGroup.Common,     26) },
                { AppIconType.CommonPictureLandscape,   new FontIconDescriptor(FontIconGroup.Common,     27) },
                { AppIconType.CommonPicturePattern,     new FontIconDescriptor(FontIconGroup.Common,     28) },
                { AppIconType.CommonPictureNone,        new FontIconDescriptor(FontIconGroup.Common,     29) },
                { AppIconType.CommonRecurrence,         new FontIconDescriptor(FontIconGroup.Common,     30) },
                { AppIconType.CommonFlash,              new FontIconDescriptor(FontIconGroup.Common,     31) },
                { AppIconType.CommonAppBarAdd,          new FontIconDescriptor(FontIconGroup.Common,     32) },
                { AppIconType.CommonAppBarAddContinue,  new FontIconDescriptor(FontIconGroup.Common,     33) },
                { AppIconType.CommonWindowsLogo,        new FontIconDescriptor(FontIconGroup.Common,     34) },
                { AppIconType.CommonMagnify,            new FontIconDescriptor(FontIconGroup.Common,     35) },
                { AppIconType.CommonAddFolder,          new FontIconDescriptor(FontIconGroup.Common,     36) },
                { AppIconType.CommonRing,               new FontIconDescriptor(FontIconGroup.Common,     37) },
                { AppIconType.CommonExport,             new FontIconDescriptor(FontIconGroup.Common,     38) },
                { AppIconType.CommonImport,             new FontIconDescriptor(FontIconGroup.Common,     39) },
                { AppIconType.CommonHyperlink,          new FontIconDescriptor(FontIconGroup.Common,     40) },
                { AppIconType.CommonClear,              new FontIconDescriptor(FontIconGroup.Common,     41) },
                { AppIconType.CommonExpand,             new FontIconDescriptor(FontIconGroup.Common,     42) },
                { AppIconType.CommonSmile,              new FontIconDescriptor(FontIconGroup.Common,     43) },
                { AppIconType.CommonGitHub,             new FontIconDescriptor(FontIconGroup.Common,     44) },

                { AppIconType.PriorityNone,             new FontIconDescriptor(FontIconGroup.Priority,   0) },
                { AppIconType.PriorityLow,              new FontIconDescriptor(FontIconGroup.Priority,   1) },
                { AppIconType.PriorityMedium,           new FontIconDescriptor(FontIconGroup.Priority,   2) },
                { AppIconType.PriorityHigh,             new FontIconDescriptor(FontIconGroup.Priority,   3) },
                { AppIconType.PriorityStar,             new FontIconDescriptor(FontIconGroup.Priority,   4) },

                { AppIconType.SettingGeneral,           new FontIconDescriptor(FontIconGroup.Setting,   0) },
                { AppIconType.SettingDisplay,           new FontIconDescriptor(FontIconGroup.Setting,   1) },
                { AppIconType.SettingOrdering,          new FontIconDescriptor(FontIconGroup.Setting,   2) },
                { AppIconType.SettingViews,             new FontIconDescriptor(FontIconGroup.Setting,   3) },
                { AppIconType.SettingSmartViews,        new FontIconDescriptor(FontIconGroup.Setting,   4) },
                { AppIconType.SettingFolders,           new FontIconDescriptor(FontIconGroup.Setting,   5) },
                { AppIconType.SettingContexts,          new FontIconDescriptor(FontIconGroup.Setting,   6) },
                { AppIconType.SettingMisc,              new FontIconDescriptor(FontIconGroup.Setting,   7) },
                { AppIconType.SettingSync,              new FontIconDescriptor(FontIconGroup.Setting,   8) },
                { AppIconType.SettingAbout,             new FontIconDescriptor(FontIconGroup.Setting,   9) },
                { AppIconType.SettingHelp,              new FontIconDescriptor(FontIconGroup.Common,   25) },

                { AppIconType.GroupAction,              new FontIconDescriptor(FontIconGroup.Group,   0) },
                { AppIconType.GroupAscending,           new FontIconDescriptor(FontIconGroup.Group,   1) },
                { AppIconType.GroupDescending,          new FontIconDescriptor(FontIconGroup.Group,   2) },
                { AppIconType.GroupDueDate,             new FontIconDescriptor(FontIconGroup.Group,   3) },
                { AppIconType.GroupContext,             new FontIconDescriptor(FontIconGroup.Group,   4) },
                { AppIconType.GroupFolder,              new FontIconDescriptor(FontIconGroup.Group,   5) },
                { AppIconType.GroupPriority,            new FontIconDescriptor(FontIconGroup.Group,   6) },
                { AppIconType.GroupProgress,            new FontIconDescriptor(FontIconGroup.Group,   7) },
                { AppIconType.GroupStartDate,           new FontIconDescriptor(FontIconGroup.Group,   8) },
                { AppIconType.GroupStatus,              new FontIconDescriptor(FontIconGroup.Group,   9) },
                { AppIconType.GroupCompleted,           new FontIconDescriptor(FontIconGroup.Group,   10) },
                { AppIconType.GroupModified,            new FontIconDescriptor(FontIconGroup.Group,   11) },

                { AppIconType.View01,                   new FontIconDescriptor(FontIconGroup.View,   0) },
                { AppIconType.View02,                   new FontIconDescriptor(FontIconGroup.View,   1) },
                { AppIconType.View03,                   new FontIconDescriptor(FontIconGroup.View,   2) },
                { AppIconType.View04,                   new FontIconDescriptor(FontIconGroup.View,   3) },
                { AppIconType.View05,                   new FontIconDescriptor(FontIconGroup.View,   4) },
                { AppIconType.View06,                   new FontIconDescriptor(FontIconGroup.View,   5) },
                { AppIconType.View07,                   new FontIconDescriptor(FontIconGroup.View,   6) },
                { AppIconType.View08,                   new FontIconDescriptor(FontIconGroup.View,   7) },
                { AppIconType.View09,                   new FontIconDescriptor(FontIconGroup.View,   8) },
                { AppIconType.View10,                   new FontIconDescriptor(FontIconGroup.View,   9) },
                { AppIconType.View11,                   new FontIconDescriptor(FontIconGroup.View,   10) },
                { AppIconType.View12,                   new FontIconDescriptor(FontIconGroup.View,   11) },
                { AppIconType.View13,                   new FontIconDescriptor(FontIconGroup.View,   12) },

                { AppIconType.Folder01,                 new FontIconDescriptor(FontIconGroup.Folder,   0) },
                { AppIconType.Folder02,                 new FontIconDescriptor(FontIconGroup.Folder,   1) },
                { AppIconType.Folder03,                 new FontIconDescriptor(FontIconGroup.Folder,   2) },
                { AppIconType.Folder04,                 new FontIconDescriptor(FontIconGroup.Folder,   3) },
                { AppIconType.Folder05,                 new FontIconDescriptor(FontIconGroup.Folder,   4) },
                { AppIconType.Folder06,                 new FontIconDescriptor(FontIconGroup.Folder,   5) },
                { AppIconType.Folder07,                 new FontIconDescriptor(FontIconGroup.Folder,   6) },
                { AppIconType.Folder08,                 new FontIconDescriptor(FontIconGroup.Folder,   7) },
                { AppIconType.Folder09,                 new FontIconDescriptor(FontIconGroup.Folder,   8) },
                { AppIconType.Folder10,                 new FontIconDescriptor(FontIconGroup.Folder,   9) },
                { AppIconType.Folder11,                 new FontIconDescriptor(FontIconGroup.Folder,   10) },
                { AppIconType.Folder12,                 new FontIconDescriptor(FontIconGroup.Folder,   11) },
                { AppIconType.Folder13,                 new FontIconDescriptor(FontIconGroup.Folder,   12) },
                { AppIconType.Folder14,                 new FontIconDescriptor(FontIconGroup.Folder,   13) },
                { AppIconType.Folder15,                 new FontIconDescriptor(FontIconGroup.Folder,   14) },
                { AppIconType.Folder16,                 new FontIconDescriptor(FontIconGroup.Folder,   15) },
                { AppIconType.Folder17,                 new FontIconDescriptor(FontIconGroup.Folder,   16) },
                { AppIconType.Folder18,                 new FontIconDescriptor(FontIconGroup.Folder,   17) },
                { AppIconType.Folder19,                 new FontIconDescriptor(FontIconGroup.Folder,   18) },
                { AppIconType.Folder20,                 new FontIconDescriptor(FontIconGroup.Folder,   19) },
                { AppIconType.Folder21,                 new FontIconDescriptor(FontIconGroup.Folder,   20) },
                { AppIconType.Folder22,                 new FontIconDescriptor(FontIconGroup.Folder,   21) },
                { AppIconType.Folder23,                 new FontIconDescriptor(FontIconGroup.Folder,   22) },
                { AppIconType.Folder24,                 new FontIconDescriptor(FontIconGroup.Folder,   23) },
                { AppIconType.Folder25,                 new FontIconDescriptor(FontIconGroup.Folder,   24) },
                { AppIconType.Folder26,                 new FontIconDescriptor(FontIconGroup.Folder,   25) },
                { AppIconType.Folder27,                 new FontIconDescriptor(FontIconGroup.Folder,   26) },
                { AppIconType.Folder28,                 new FontIconDescriptor(FontIconGroup.Folder,   27) },
                { AppIconType.Folder29,                 new FontIconDescriptor(FontIconGroup.Folder,   28) },
                { AppIconType.Folder30,                 new FontIconDescriptor(FontIconGroup.Folder,   29) },
                { AppIconType.Folder31,                 new FontIconDescriptor(FontIconGroup.Folder,   30) },
                { AppIconType.Folder32,                 new FontIconDescriptor(FontIconGroup.Folder,   31) },
                { AppIconType.Folder33,                 new FontIconDescriptor(FontIconGroup.Folder,   32) },
                { AppIconType.Folder34,                 new FontIconDescriptor(FontIconGroup.Folder,   33) },
                { AppIconType.Folder35,                 new FontIconDescriptor(FontIconGroup.Folder,   34) },
                { AppIconType.Folder36,                 new FontIconDescriptor(FontIconGroup.Folder,   35) },
                { AppIconType.Folder37,                 new FontIconDescriptor(FontIconGroup.Folder,   36) },
                { AppIconType.Folder38,                 new FontIconDescriptor(FontIconGroup.Folder,   37) },
                { AppIconType.Folder39,                 new FontIconDescriptor(FontIconGroup.Folder,   38) },
                { AppIconType.Folder40,                 new FontIconDescriptor(FontIconGroup.Folder,   39) },
                { AppIconType.Folder41,                 new FontIconDescriptor(FontIconGroup.Folder,   40) },
                { AppIconType.Folder42,                 new FontIconDescriptor(FontIconGroup.Folder,   41) },

                { AppIconType.LinkView,                 new FontIconDescriptor(FontIconGroup.Folder,   5) },
                { AppIconType.LinkFolder,               new FontIconDescriptor(FontIconGroup.Folder,   7) },
                { AppIconType.LinkContext,              new FontIconDescriptor(FontIconGroup.Folder,   13) },
                { AppIconType.LinkTag,                  new FontIconDescriptor(FontIconGroup.Folder,   41) },
                { AppIconType.LinkCalendar,             new FontIconDescriptor(FontIconGroup.Folder,   6) },
                { AppIconType.LinkCloud,                new FontIconDescriptor(FontIconGroup.Folder,   10) },
                { AppIconType.LinkMagic,                new FontIconDescriptor(FontIconGroup.Folder,   25) },                
            };
        }
        
        public static string GetSymbolCode(AppIconType icon)
        {
            if (!iconDescriptors.ContainsKey(icon))
            {
                Debug.WriteLine("WARNING: unknown icon: {0}", icon);
                return String.Empty;
            }

            FontIconDescriptor fontIconDescriptor = iconDescriptors[icon];

            char code = (char) (fontIconDescriptor.Offset + (int) fontIconDescriptor.Group);

            return code.ToString();
        }

        public static AppIconType GetAppIcon(int iconId)
        {
            if (iconId >= 1000)
                return priorityIcons[iconId - 1000];
            if (iconId > FolderIconsCount) 
                return viewIcons[iconId - ViewIconIndexStart];
            else if (iconId > 0)
                return folderIcons[iconId - 1];
            else
                return AppIconType.None;
        }
        
        private struct FontIconDescriptor
        {
            public FontIconDescriptor(FontIconGroup iconGroup, int offset) : this()
            {
                this.Group = iconGroup;
                this.Offset = offset;
            }

            public int Offset { get; private set; }
            public FontIconGroup Group { get; private set; }
        }

        private enum FontIconGroup
        {
            Common = 0xE000,
            Folder = 0xE040,
            Group = 0xE080,
            Logo = 0xE0C0,
            Priority = 0xE100,
            Setting = 0xE140,
            View = 0xE180,
        }
    }
}

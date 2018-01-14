using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model
{
    public static class ReminderDisplayModeConverter
    {
        private static readonly Dictionary<ReminderDisplayMode, string> descriptions;

        public static IEnumerable<string> Descriptions
        {
            get { return descriptions.Values; }
        }

        static ReminderDisplayModeConverter()
        {
            descriptions = new Dictionary<ReminderDisplayMode, string>
                               {
                                   {ReminderDisplayMode.IconAndTime,    StringResources.ReminderDisplayMode_IconAndTime},
                                   {ReminderDisplayMode.Icon,           StringResources.ReminderDisplayMode_Icon},
                                   {ReminderDisplayMode.Time,           StringResources.ReminderDisplayMode_Time},
                                   {ReminderDisplayMode.None,           StringResources.ReminderDisplayMode_None},
                               };
        }

        public static ReminderDisplayMode FromIndex(int i)
        {
            if (i < 0 || i > descriptions.Count())
                throw new ArgumentOutOfRangeException("i");

            return (ReminderDisplayMode)i;
        }

        public static ReminderDisplayMode FromDescription(string description)
        {
            foreach (var kvp in descriptions)
            {
                if (kvp.Value.Equals(description, StringComparison.CurrentCultureIgnoreCase))
                {
                    return kvp.Key;
                }
            }

            return ReminderDisplayMode.None;
        }

        public static string GetDescription(this ReminderDisplayMode focusFilter)
        {
            return descriptions[focusFilter];
        }
    }
}

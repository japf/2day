using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model
{
    public enum DefaultDate
    {
        None = 0,
        Today = 1,
        Tomorrow = 2
    }
    
    public static class DefaultDateConverter
    {
        private static readonly Dictionary<DefaultDate, string> descriptions;

        public static IEnumerable<string> Descriptions
        {
            get { return descriptions.Values; }
        }

        static DefaultDateConverter()
        {
            descriptions = new Dictionary<DefaultDate, string>
                {
                    {DefaultDate.None,     StringResources.DefaultDate_None},
                    {DefaultDate.Today,    StringResources.DefaultDate_Today},
                    {DefaultDate.Tomorrow, StringResources.DefaultDate_Tomorrow},
                };
        }

        public static DefaultDate FromIndex(int i)
        {
            if (i < 0 || i > 7)
                throw new ArgumentOutOfRangeException("i");

            return (DefaultDate)i;
        }

        public static DefaultDate FromDescription(string description)
        {
            foreach (var kvp in descriptions)
            {
                if (kvp.Value.Equals(description, StringComparison.CurrentCultureIgnoreCase))
                    return kvp.Key;
            }

            return DefaultDate.None;
        }

        public static string GetDescription(this DefaultDate defaultDate)
        {
            return descriptions[defaultDate];
        }
    }
}

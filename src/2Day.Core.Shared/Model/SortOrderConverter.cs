using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model
{
    public static class SortOrderConverter
    {
        private static readonly Dictionary<SortOrder, string> descriptions;

        public static IEnumerable<string> Descriptions
        {
            get { return descriptions.Values; }
        }

        static SortOrderConverter()
        {
            descriptions = new Dictionary<SortOrder, string>
            {
                {SortOrder.Ascending,       StringResources.SortOrder_Ascending},
                {SortOrder.Descending,      StringResources.SortOrder_Descending},
            };
        }

        public static SortOrder FromIndex(int i)
        {
            if (i < 0 || i > 1)
                throw new ArgumentOutOfRangeException("i");

            return (SortOrder)i;
        }

        public static SortOrder FromDescription(string description)
        {
            foreach (var kvp in descriptions)
            {
                if (kvp.Value.Equals(description, StringComparison.CurrentCultureIgnoreCase))
                    return kvp.Key;
            }

            return SortOrder.Ascending;
        }

        public static string GetDescription(this SortOrder sortOrder)
        {
            return descriptions[sortOrder];
        }
    }
}
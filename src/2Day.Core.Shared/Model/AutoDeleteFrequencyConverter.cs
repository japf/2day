using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model
{
    public static class AutoDeleteFrequencyConverter
    {
        private static readonly Dictionary<AutoDeleteFrequency, string> descriptions;

        public static IEnumerable<string> Descriptions
        {
            get { return descriptions.Values; }
        }

        static AutoDeleteFrequencyConverter()
        {
            descriptions = new Dictionary<AutoDeleteFrequency, string>
                               {
                                   {AutoDeleteFrequency.Never,          StringResources.AutoDelete_Never},         
                                   {AutoDeleteFrequency.OneDay,         StringResources.AutoDelete_OneDay},
                                   {AutoDeleteFrequency.ThreeDays,      StringResources.AutoDelete_ThreeDays},
                                   {AutoDeleteFrequency.OneWeek,        StringResources.AutoDelete_OneWeek},
                                   {AutoDeleteFrequency.TwoWeeks,       StringResources.AutoDelete_TwoWeeks},
                                   {AutoDeleteFrequency.OneMonths,      StringResources.AutoDelete_OneMonth},
                                   {AutoDeleteFrequency.ThreeMonths,    StringResources.AutoDelete_ThreeMonths},
                                   {AutoDeleteFrequency.SixMonths,      StringResources.AutoDelete_SixMonths},
                                   {AutoDeleteFrequency.TwelveMonths,   StringResources.AutoDelete_TwelveMonths},
                                   {AutoDeleteFrequency.EighteenMonths, StringResources.AutoDelete_EighteenMonths},
                               };
        }

        public static AutoDeleteFrequency FromIndex(int i)
        {
            if (i < 0 || i > 7)
                throw new ArgumentOutOfRangeException("i");

            return (AutoDeleteFrequency)i;
        }

        public static AutoDeleteFrequency FromDescription(string description)
        {
            foreach (var kvp in descriptions)
            {
                if (kvp.Value.Equals(description, StringComparison.CurrentCultureIgnoreCase))
                    return kvp.Key;
            }

            return AutoDeleteFrequency.Never;
        }

        public static string GetDescription(this AutoDeleteFrequency frequency)
        {
            return descriptions[frequency];
        }
    }
}
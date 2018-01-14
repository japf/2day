using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model
{
    public enum CompletedTaskMode
    {
        Unkown,
        Hide,
        Show,
        Group
    }

    public static class CompletedTaskModeConverter
    {
        private static readonly Dictionary<CompletedTaskMode, string> descriptions;

        public static IEnumerable<string> Descriptions
        {
            get { return descriptions.Values; }
        }

        static CompletedTaskModeConverter()
        {
            descriptions = new Dictionary<CompletedTaskMode, string>
                               {
                                   {CompletedTaskMode.Hide,     StringResources.CompletedTaskMode_Hide},
                                   {CompletedTaskMode.Show,     StringResources.CompletedTaskMode_Show},
                                   {CompletedTaskMode.Group,    StringResources.CompletedTaskMode_Group}
                               };
        }

        public static CompletedTaskMode FromIndex(int i)
        {
            if (i < 0 || i > descriptions.Count)
                throw new ArgumentOutOfRangeException("i");

            return (CompletedTaskMode)i;
        }

        public static CompletedTaskMode FromDescription(string description)
        {
            foreach (var kvp in descriptions)
            {
                if (kvp.Value.Equals(description, StringComparison.CurrentCultureIgnoreCase))
                {
                    return kvp.Key;
                }
            }

            return CompletedTaskMode.Hide;
        }

        public static string GetDescription(this CompletedTaskMode completedTaskMode)
        {
            return descriptions[completedTaskMode];
        }
    }
}

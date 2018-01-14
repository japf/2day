using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model
{
    public static class TaskPriorityConverter
    {
        private static readonly Dictionary<TaskPriority, string> descriptions;

        public static IEnumerable<string> Descriptions
        {
            get { return descriptions.Values; }
        }

        static TaskPriorityConverter()
        {
            descriptions = new Dictionary<TaskPriority, string>
                               {
                                   {TaskPriority.None,     StringResources.TaskPriority_None},
                                   {TaskPriority.Low,      StringResources.TaskPriority_Low},
                                   {TaskPriority.Medium,   StringResources.TaskPriority_Medium},
                                   {TaskPriority.High,     StringResources.TaskPriority_High},
                                   {TaskPriority.Star,     StringResources.TaskPriority_Star},
                               };
        }

        public static TaskPriority FromIndex(int i)
        {
            if (i < 0 || i > 4)
                throw new ArgumentOutOfRangeException("i");

            return (TaskPriority)i;
        }

        public static TaskPriority FromDescription(string description)
        {
            foreach (var kvp in descriptions)
            {
                if (kvp.Value.Equals(description, StringComparison.CurrentCultureIgnoreCase))
                    return kvp.Key;
            }

            return TaskPriority.None;
        }

        public static string GetDescription(this TaskPriority taskPriority)
        {
            return descriptions[taskPriority];
        }
    }
}
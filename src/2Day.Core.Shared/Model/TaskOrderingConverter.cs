using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model
{
    public static class TaskOrderingConverter
    {
        private static readonly Dictionary<TaskOrdering, string> descriptions;

        public static IEnumerable<string> Descriptions
        {
            get { return descriptions.Values; }
        }

        static TaskOrderingConverter()
        {
            descriptions = new Dictionary<TaskOrdering, string>
                               {
                                   {TaskOrdering.Priority,      StringResources.TaskOrdering_Priority},
                                   {TaskOrdering.DueDate,       StringResources.TaskOrdering_DueDate},
                                   {TaskOrdering.Folder,        StringResources.TaskOrdering_Folder},
                                   {TaskOrdering.Alphabetical,  StringResources.TaskOrdering_Alphabetical},
                                   {TaskOrdering.Context,       StringResources.TaskOrdering_Context},
                                   {TaskOrdering.AddedDate,     StringResources.TaskOrdering_AddedDate},
                                   {TaskOrdering.ModifiedDate,  StringResources.TaskOrdering_ModifiedDate},
                                   {TaskOrdering.StartDate,     StringResources.TaskOrdering_StartDate},
                                   {TaskOrdering.Alarm,         StringResources.TaskOrdering_Reminder},
                               };
        }

        public static TaskOrdering FromIndex(int i)
        {
            if (i < 0 || i > 7)
                throw new ArgumentOutOfRangeException("i");

            return (TaskOrdering)i;
        }

        public static TaskOrdering FromDescription(string description)
        {
            foreach (var kvp in descriptions)
            {
                if (kvp.Value.Equals(description, StringComparison.CurrentCultureIgnoreCase))
                    return kvp.Key;
            }

            return TaskOrdering.Priority;
        }

        public static string GetDescription(this TaskOrdering taskPriority)
        {
            return descriptions[taskPriority];
        }
    }
}
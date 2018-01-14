using System;
using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model
{
    public static class TaskGroupConverter
    {
        private static readonly Dictionary<TaskGroup, string> descriptions;

        public static IEnumerable<string> Descriptions
        {
            get { return descriptions.Values; }
        }

        static TaskGroupConverter()
        {
            descriptions = new Dictionary<TaskGroup, string>
            {
                {TaskGroup.DueDate,       StringResources.TaskGroup_Due},
                {TaskGroup.Priority,      StringResources.TaskGroup_Priority},
                {TaskGroup.Status,        StringResources.TaskGroup_Status},
                {TaskGroup.Folder,        StringResources.TaskGroup_Folder},
                {TaskGroup.Action,        StringResources.TaskGroup_Action},
                {TaskGroup.Progress,      StringResources.TaskGroup_Progress},
                {TaskGroup.Context,       StringResources.TaskGroup_Context},
                {TaskGroup.StartDate,     StringResources.TaskGroup_StartDate},
                {TaskGroup.Completed,     StringResources.TaskGroup_Completed},
                {TaskGroup.Modified,      StringResources.TaskGroup_Modified},
            };
        }

        public static TaskGroup FromIndex(int i)
        {
            if (i < 0 || i > 6)
                throw new ArgumentOutOfRangeException("i");

            return (TaskGroup)i;
        }

        public static TaskGroup FromDescription(string description)
        {
            foreach (var kvp in descriptions)
            {
                if (kvp.Value.Equals(description, StringComparison.CurrentCultureIgnoreCase))
                    return kvp.Key;
            }

            return TaskGroup.Priority;
        }

        public static TaskGroup FromName(string name)
        {
            foreach (var kvp in descriptions)
            {
                if (kvp.Key.ToString().Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    return kvp.Key;
            }

            return TaskGroup.Priority;
        }

        public static string GetDescription(this TaskGroup taskGroup)
        {
            return descriptions[taskGroup];
        }
    }
}
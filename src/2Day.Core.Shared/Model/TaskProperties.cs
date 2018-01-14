using System;

namespace Chartreuse.Today.Core.Shared.Model
{
    [Flags]
    public enum TaskProperties
    {
        // do not edit this enum without updating ExchangeTaskProperties too
        None = 0x00,
        Title = 0x01,
        Folder = 0x02,
        Added = 0x04,
        Modified = 0x08,
        Due = 0x10,
        Completed = 0x20,
        Priority = 0x40,
        Note = 0x80,
        Progress = 0x100,
        Frequency = 0x200,
        RepeatFrom = 0x400,
        Tags = 0x800,
        Context = 0x1000,
        Start = 0x2000,
        Alarm = 0x4000,
        Parent = 0x8000,
        All = 0xFFFF
    }

    public static class TaskPropertiesUtil
    {
        public static TaskProperties StringToTaskProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "Title":
                    return TaskProperties.Title;
                case "Folder":
                    return TaskProperties.Folder;
                case "Context":
                    return TaskProperties.Context;
                case "Added":
                    return TaskProperties.Added;
                case "Modified":
                    return TaskProperties.Modified;
                case "Due":
                    return TaskProperties.Due;
                case "Start":
                    return TaskProperties.Start;
                case "Completed":
                    return TaskProperties.Completed;
                case "Priority":
                    return TaskProperties.Priority;
                case "Note":
                    return TaskProperties.Note;
                case "Tags":
                    return TaskProperties.Tags;
                case "Progress":
                    return TaskProperties.Progress;
                case "CustomFrequency":
                    return TaskProperties.Frequency;
                case "UseFixedDate":
                    return TaskProperties.RepeatFrom;
                case "Alarm":
                    return TaskProperties.Alarm;
                case "ParentId":
                    return TaskProperties.Parent;
                default:
                    return TaskProperties.None;
            }
        }
    }

}

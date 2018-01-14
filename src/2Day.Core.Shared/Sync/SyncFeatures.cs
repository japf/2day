using System;

namespace Chartreuse.Today.Core.Shared.Sync
{
    [Flags]
    public enum SyncFeatures
    {
        Title = 0x01,
        DueDate = 0x02,
        Priority = 0x04,
        Folder = 0x08,
        Colors = 0x10,
        Icons = 0x20,
        Recurrence = 0x40,
        Reminders = 0x80,
        Notes = 0x100,
        Context = 0x200,
        StartDate = 0x400,
        Progress = 0x800,
        SmartView = 0x1000
    }
}
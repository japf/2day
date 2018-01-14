using System;

namespace Chartreuse.Today.Exchange.Model
{
    [Flags]
    public enum ExchangeTaskProperties
    {
        // do not edit this enum without updating TaskProperties too
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
        All = 0xFFFF
    }
}

using System;

namespace Chartreuse.Today.Core.Shared.Tools.Logging
{
    [Flags]
    public enum LogLevel
    {
        None = 0x0000,
        Network = 0x0001,
        Debug = 0x0002,
        Warning = 0x0004,
        Error = 0x0008,

        Normal = Debug | Warning | Error,
        Verbose = Network | Debug | Warning | Error
    }
}

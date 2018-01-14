using System;

namespace Chartreuse.Today.Exchange.Ews.Model
{
    [Flags]
    public enum EwsFields
    {
        None            = 0,
        Subject         = 1,
        Body            = 2,
        DueDate         = 4,
        StartDate       = 8,
        OrdinalDate     = 16,
        CompleteDate    = 32,
        Reminder        = 64,
        Recurrence      = 128,
        Categories      = 256,
        Importance      = 512,
        PercentComplete = 1024,
        Status          = 2048,

        All             = 0xFFFF
    }
}
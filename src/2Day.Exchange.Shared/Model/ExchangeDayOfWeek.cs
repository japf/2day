using System;

namespace Chartreuse.Today.Exchange.Model
{
    [Flags]
    public enum ExchangeDayOfWeek
    {
        None = 0,
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64,
        Day = 128,
        Weekday = 256,
        WeekendDay = 512
    }
}
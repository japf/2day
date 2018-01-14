namespace Chartreuse.Today.ToodleDo
{
    public class ToodleDoConstants
    {
        public const string ServerUrl = "https://api.toodledo.com/2";
        public const string Server = "http://www.toodledo.com";

        public const string RecurrencyNone = "None";
        public const string RecurrencyDaily = "Daily";
        public const string RecurrencyWeekly = "Weekly";
        public const string RecurrencyMonthly = "Monthly";
        public const string RecurrencyYearly = "Yearly";

        // Days of week pattern
        public const string RecurrencyMonday = "Mon";
        public const string RecurrencyTuesday = "Tue";
        public const string RecurrencyWednesday = "Wed";
        public const string RecurrencyThursday = "Thu";
        public const string RecurrencyFriday = "Fri";
        public const string RecurrencySaturday = "Sat";
        public const string RecurrencySunday = "Sun";
        public const string RecurrencyWeekday = "weekday";
        public const string RecurrencyWeekend = "weekend";
        public const string RecurrencyDays = "days";
        public const string RecurrencyDay = "day";
        public const string RecurrencyWeeks = "weeks";
        public const string RecurrencyWeek = "week";
        public const string RecurrencyMonths = "months";
        public const string RecurrencyMonth = "month";
        public const string RecurrencyYears = "years";
        public const string RecurrencyYear = "year";
        public const string RecurrencyFirst = "first";
        public const string RecurrencySecond = "second";
        public const string RecurrencyThird = "third";
        public const string RecurrencyFourth = "fourth";
        public const string RecurrencyLast = "last";
        public const string RecurrencyDaySeparator = ",";
        public const string RecurrencyDaysOfWeekPattern = "Every {0}";
        public const string RecurrencyEveryPeriodPattern = "Every {0} {1}";
        public const string RecurrencyOnXDayPattern = "On the {0} {1} of each month";

        public const string RecurrencyToodleDoNone = "None";
        public const string RecurrencyToodleDoDaily = "Daily";
        public const string RecurrencyToodleDoWeekly = "Weekly";
        public const string RecurrencyToodleDoBiweekly = "Biweekly";
        public const string RecurrencyToodleDoMonthly = "Monthly";
        public const string RecurrencyToodleDoBimonthly = "Bimonthly";
        public const string RecurrencyToodleDoQuarterly = "Quarterly";
        public const string RecurrencyToodleDoSemiannually = "Semiannually";
        public const string RecurrencyToodleDoYearly = "Yearly";

        public const string RecurrencyToodleDoOnXDayRegex = "The (?<Position>1st|2nd|3rd|4th|5th|last) (?<DayOfWeek>mon|tue|wed|thu|fri|sat|sun) of each month";
        public const string RecurrencyToodleDoDaysOfWeekRegex = "Every\\s+(?:(?<DayOfWeek>mon|tue|wed|thu|fri|sat|sun|weekday|weekend)(?:\\s|,|and)*)+";
        public const string RecurrencyToodleDoEveryPeriodRegex = "Every\\s+(?<Rate>\\d+)\\s+(?<Scale>day|week|month|year)s*";
        public const string RecurrencyFirstShort = "1st";
        public const string RecurrencySecondShort = "2nd";
        public const string RecurrencyThirdShort = "3rd";
        public const string RecurrencyFourthShort = "4th";
        public const string RecurrencyFifthShort = "5th";
    }
}

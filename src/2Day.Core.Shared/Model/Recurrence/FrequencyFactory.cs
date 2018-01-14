using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public static class FrequencyFactory
    {
        private static readonly Dictionary<FrequencyType, Type> frequencies;

        static FrequencyFactory()
        {
            frequencies = new Dictionary<FrequencyType, Type>
            {
                {FrequencyType.Once, typeof (OnceOnlyFrequency)},
                {FrequencyType.Daily, typeof (DailyFrequency)},
                {FrequencyType.Weekly, typeof (WeeklyFrequency)},
                {FrequencyType.Monthly, typeof (MonthlyFrequency)},
                {FrequencyType.Yearly, typeof (YearlyFrequency)},
                {FrequencyType.EveryXPeriod, typeof (EveryXPeriodFrequency)},
                {FrequencyType.DaysOfWeek, typeof (DaysOfWeekFrequency)},
                {FrequencyType.OnXDayOfEachMonth, typeof (OnXDayFrequency)}
            };
        }

        public static ICustomFrequency GetCustomFrequency(FrequencyType frequencyType)
        {
            return GetCustomFrequency<ICustomFrequency>(frequencyType, String.Empty);
        }

        public static T GetCustomFrequency<T>(FrequencyType frequencyType) where T : class, ICustomFrequency
        {
            return GetCustomFrequency<T>(frequencyType, String.Empty);
        }

        public static T GetCustomFrequency<T>(FrequencyType frequencyType, string value) where T : ICustomFrequency
        {
            if (frequencies.ContainsKey(frequencyType))
            {
                T frequency = (T)Activator.CreateInstance(frequencies[frequencyType]);
                frequency.Value = value;

                return frequency;
            }
            else
            {
                throw new NotSupportedException("This type has not associated ICustomFrequency implementation");
            }
        }
    }
}

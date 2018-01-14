using System;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public class FrequencyItem<T>
    {
        public T Value { get; set; }
        public string DisplayValue { get; set; }

        public override string ToString()
        {
            return this.DisplayValue;
        } 
    }

    public class RankingItem : FrequencyItem<RankingPosition>
    {
    }

    public class DayOfWeekItem : FrequencyItem<DayOfWeek>
    {
    }

    public class RateItem : FrequencyItem<int>
    {
    }

    public class ScaleItem : FrequencyItem<CustomFrequencyScale>
    {
    }
}

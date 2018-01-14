using System;
using System.Collections.Generic;
using System.Globalization;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public static class FrequencyMetadata
    {
        public const int MaxFrequencyRate = 30;

        public static readonly List<RankingItem> AvailableRankings = new List<RankingItem>
        {
            new RankingItem { Value = RankingPosition.First, DisplayValue = StringResources.Frequency_First},
            new RankingItem { Value = RankingPosition.Second, DisplayValue = StringResources.Frequency_Second},
            new RankingItem { Value = RankingPosition.Third, DisplayValue = StringResources.Frequency_Third},
            new RankingItem { Value = RankingPosition.Fourth, DisplayValue = StringResources.Frequency_Fourth},
            new RankingItem { Value = RankingPosition.Last, DisplayValue = StringResources.Frequency_Last}
        };

        public static readonly List<DayOfWeekItem> AvailableDays = new List<DayOfWeekItem>
        {
            new DayOfWeekItem { Value = DayOfWeek.Monday, DisplayValue = DateTimeExtensions.MondayString},
            new DayOfWeekItem { Value = DayOfWeek.Tuesday, DisplayValue = DateTimeExtensions.TuesdayString},
            new DayOfWeekItem { Value = DayOfWeek.Wednesday, DisplayValue = DateTimeExtensions.WednesdayString},
            new DayOfWeekItem { Value = DayOfWeek.Thursday, DisplayValue = DateTimeExtensions.ThursdayString},
            new DayOfWeekItem { Value = DayOfWeek.Friday, DisplayValue = DateTimeExtensions.FridayString},
            new DayOfWeekItem { Value = DayOfWeek.Saturday, DisplayValue = DateTimeExtensions.SaturdayString},
            new DayOfWeekItem { Value = DayOfWeek.Sunday, DisplayValue = DateTimeExtensions.SundayString},
        };

        public static readonly List<RateItem> AvailableRates = new List<RateItem> 
        { 
            new RateItem { Value = 1, DisplayValue = StringResources.Frequency_Rate1 }
        };

        public static readonly List<ScaleItem> AvailableScales = new List<ScaleItem>
        {
            new ScaleItem() { Value = CustomFrequencyScale.Day, DisplayValue = StringResources.Frequency_Scale_Day},
            new ScaleItem() { Value = CustomFrequencyScale.Week, DisplayValue = StringResources.Frequency_Scale_Week},
            new ScaleItem() { Value = CustomFrequencyScale.Month, DisplayValue = StringResources.Frequency_Scale_Month},
            new ScaleItem() { Value = CustomFrequencyScale.Year, DisplayValue = StringResources.Frequency_Scale_Year}
        };


        static FrequencyMetadata()
        {
            for (int i = 2; i <= MaxFrequencyRate; i++)
            {
                AvailableRates.Add(new RateItem { Value = i, DisplayValue = string.Format(CultureInfo.CurrentCulture, StringResources.Frequency_RateFormat, i) });
            }
        }
    }
}
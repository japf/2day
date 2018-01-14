using System;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.ToodleDo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.ToodleDo
{
    [TestClass]
    public class ToodleDoRecurrencyTest
    {

        [TestMethod]
        public void ToodleDoNoneFrequencyTest()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency(null);
            Assert.IsTrue(frequency is OnceOnlyFrequency);
            frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency(String.Empty);
            Assert.IsTrue(frequency is OnceOnlyFrequency);
            frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("none");
            Assert.IsTrue(frequency is OnceOnlyFrequency);
            frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("None");
            Assert.IsTrue(frequency is OnceOnlyFrequency);
        }

        [TestMethod]
        public void ToodleDoDailyFrequencyTest()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("daily");
            Assert.IsTrue(frequency is DailyFrequency);
            frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("Daily");
            Assert.IsTrue(frequency is DailyFrequency);
            frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("every 1 DAY");
            Assert.IsTrue(frequency is DailyFrequency);
        }

        [TestMethod]
        public void ToodleDoWeeklyFrequencyTest()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("weekly");
            Assert.IsTrue(frequency is WeeklyFrequency);
            frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("WeeKLy");
            Assert.IsTrue(frequency is WeeklyFrequency);
            frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("eVERY 1 Week");
            Assert.IsTrue(frequency is WeeklyFrequency);
        }

        [TestMethod]
        public void ToodleDoMonthlyFrequencyTest()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("monthly");
            Assert.IsTrue(frequency is MonthlyFrequency);
            frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("MonthlY");
            Assert.IsTrue(frequency is MonthlyFrequency);
            frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("every 1 monTH");
            Assert.IsTrue(frequency is MonthlyFrequency);
        }

        [TestMethod]
        public void ToodleDoYearlyFrequencyTest()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("yearly");
            Assert.IsTrue(frequency is YearlyFrequency);
            frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("yeARly");
            Assert.IsTrue(frequency is YearlyFrequency);
            frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("Every 1 Year");
            Assert.IsTrue(frequency is YearlyFrequency);
        }

        [TestMethod]
        public void ToodleDoDaysOfWeekFrequencyTest1()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("every mon");
            DaysOfWeekFrequency freq = frequency as DaysOfWeekFrequency;
            Assert.IsNotNull(freq);
            Assert.IsTrue(freq.IsMonday);
            Assert.IsFalse(freq.IsTuesday);
            Assert.IsFalse(freq.IsWednesday);
            Assert.IsFalse(freq.IsThursday);
            Assert.IsFalse(freq.IsFriday);
            Assert.IsFalse(freq.IsSaturday);
            Assert.IsFalse(freq.IsSunday);
        }

        [TestMethod]
        public void ToodleDoDaysOfWeekFrequencyTest2()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("every tue, WED");
            DaysOfWeekFrequency freq = frequency as DaysOfWeekFrequency;
            Assert.IsNotNull(freq);
            Assert.IsFalse(freq.IsMonday);
            Assert.IsTrue(freq.IsTuesday);
            Assert.IsTrue(freq.IsWednesday);
            Assert.IsFalse(freq.IsThursday);
            Assert.IsFalse(freq.IsFriday);
            Assert.IsFalse(freq.IsSaturday);
            Assert.IsFalse(freq.IsSunday);
        }

        [TestMethod]
        public void ToodleDoDaysOfWeekFrequencyTest3()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("every fri, sat and sun");
            DaysOfWeekFrequency freq = frequency as DaysOfWeekFrequency;
            Assert.IsNotNull(freq);
            Assert.IsFalse(freq.IsMonday);
            Assert.IsFalse(freq.IsTuesday);
            Assert.IsFalse(freq.IsWednesday);
            Assert.IsFalse(freq.IsThursday);
            Assert.IsTrue(freq.IsFriday);
            Assert.IsTrue(freq.IsSaturday);
            Assert.IsTrue(freq.IsSunday);
        }

        [TestMethod]
        public void ToodleDoDaysOfWeekFrequencyTest4()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("every weekday");
            DaysOfWeekFrequency freq = frequency as DaysOfWeekFrequency;
            Assert.IsNotNull(freq);
            Assert.IsTrue(freq.IsMonday);
            Assert.IsTrue(freq.IsTuesday);
            Assert.IsTrue(freq.IsWednesday);
            Assert.IsTrue(freq.IsThursday);
            Assert.IsTrue(freq.IsFriday);
            Assert.IsFalse(freq.IsSaturday);
            Assert.IsFalse(freq.IsSunday);
        }

        [TestMethod]
        public void ToodleDoDaysOfWeekFrequencyTest5()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("every weekEND");
            DaysOfWeekFrequency freq = frequency as DaysOfWeekFrequency;
            Assert.IsNotNull(freq);
            Assert.IsFalse(freq.IsMonday);
            Assert.IsFalse(freq.IsTuesday);
            Assert.IsFalse(freq.IsWednesday);
            Assert.IsFalse(freq.IsThursday);
            Assert.IsFalse(freq.IsFriday);
            Assert.IsTrue(freq.IsSaturday);
            Assert.IsTrue(freq.IsSunday);
        }

        [TestMethod]
        public void ToodleDoEveryPeriodFrequencyTest1()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("every 2 days");
            EveryXPeriodFrequency freq = frequency as EveryXPeriodFrequency;
            Assert.IsNotNull(freq);
            Assert.AreEqual(2,freq.Rate);
            Assert.AreEqual(CustomFrequencyScale.Day,freq.Scale);  
        }

        [TestMethod]
        public void ToodleDoEveryPeriodFrequencyTest2()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("every 4 months");
            EveryXPeriodFrequency freq = frequency as EveryXPeriodFrequency;
            Assert.IsNotNull(freq);
            Assert.AreEqual(4, freq.Rate);
            Assert.AreEqual(CustomFrequencyScale.Month, freq.Scale);
        }

        [TestMethod]
        public void ToodleDoEveryPeriodFrequencyTest3()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("EveRy 160 WeeKs");
            EveryXPeriodFrequency freq = frequency as EveryXPeriodFrequency;
            Assert.IsNotNull(freq);
            Assert.AreEqual(160, freq.Rate);
            Assert.AreEqual(CustomFrequencyScale.Week, freq.Scale);
        }

        [TestMethod]
        public void ToodleDoEveryPeriodFrequencyTest4()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("EveRy 3 Years");
            EveryXPeriodFrequency freq = frequency as EveryXPeriodFrequency;
            Assert.IsNotNull(freq);
            Assert.AreEqual(3, freq.Rate);
            Assert.AreEqual(CustomFrequencyScale.Year, freq.Scale);
        }

        [TestMethod]
        public void ToodleDoOnXDayFrequencyTest1()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("The 1st mon of each month");
            OnXDayFrequency freq = frequency as OnXDayFrequency;
            Assert.IsNotNull(freq);
            Assert.AreEqual(DayOfWeek.Monday, freq.DayOfWeek);
            Assert.AreEqual(RankingPosition.First, freq.RankingPosition);
        }

        [TestMethod]
        public void ToodleDoOnXDayFrequencyTest2()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("The 2ND THU of each month");
            OnXDayFrequency freq = frequency as OnXDayFrequency;
            Assert.IsNotNull(freq);
            Assert.AreEqual(DayOfWeek.Thursday, freq.DayOfWeek);
            Assert.AreEqual(RankingPosition.Second, freq.RankingPosition);
        }

        [TestMethod]
        public void ToodleDoOnXDayFrequencyTest3()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("The 3rd SaT of each month");
            OnXDayFrequency freq = frequency as OnXDayFrequency;
            Assert.IsNotNull(freq);
            Assert.AreEqual(DayOfWeek.Saturday, freq.DayOfWeek);
            Assert.AreEqual(RankingPosition.Third, freq.RankingPosition);
        }

        [TestMethod]
        public void ToodleDoOnXDayFrequencyTest4()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("The 4th sun of each month");
            OnXDayFrequency freq = frequency as OnXDayFrequency;
            Assert.IsNotNull(freq);
            Assert.AreEqual(DayOfWeek.Sunday, freq.DayOfWeek);
            Assert.AreEqual(RankingPosition.Fourth, freq.RankingPosition);
        }

        [TestMethod]
        public void ToodleDoOnXDayFrequencyTest5()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("The 5th tue of each month");
            OnXDayFrequency freq = frequency as OnXDayFrequency;
            Assert.IsNotNull(freq);
            Assert.AreEqual(DayOfWeek.Tuesday, freq.DayOfWeek);
            Assert.AreEqual(RankingPosition.Last, freq.RankingPosition);
        }

        [TestMethod]
        public void ToodleDoOnXDayFrequencyTest6()
        {
            ICustomFrequency frequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency("The LasT WeD of each month");
            OnXDayFrequency freq = frequency as OnXDayFrequency;
            Assert.IsNotNull(freq);
            Assert.AreEqual(DayOfWeek.Wednesday, freq.DayOfWeek);
            Assert.AreEqual(RankingPosition.Last, freq.RankingPosition);
        }

    }
}

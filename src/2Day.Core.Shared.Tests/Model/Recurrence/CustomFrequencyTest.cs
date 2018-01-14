using System;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Model.Recurrence
{
    [TestClass]
    public class CustomFrequencyTest
    {
        [TestMethod]
        public void OnXDayFrequencyComputeNextDateTest1()
        {
            OnXDayFrequency freq = new OnXDayFrequency();
            freq.DayOfWeek = DayOfWeek.Monday;
            freq.RankingPosition = RankingPosition.First;
            DateTime referenceDate = new DateTime(2013, 1, 1);
            DateTime result = freq.ComputeNextDate(referenceDate);
            Assert.AreEqual(new DateTime(2013,1,7),result);
            referenceDate = new DateTime(2013, 1, 10);
            result = freq.ComputeNextDate(referenceDate);
            Assert.AreEqual(new DateTime(2013, 2,4), result);
        }

        [TestMethod]
        public void OnXDayFrequencyComputeNextDateTest2()
        {
            OnXDayFrequency freq = new OnXDayFrequency();
            freq.DayOfWeek = DayOfWeek.Wednesday;
            freq.RankingPosition = RankingPosition.Second;
            DateTime referenceDate = new DateTime(2013, 2, 5);
            DateTime result = freq.ComputeNextDate(referenceDate);
            Assert.AreEqual(new DateTime(2013, 2, 13), result);
            referenceDate = new DateTime(2013, 2, 18);
            result = freq.ComputeNextDate(referenceDate);
            Assert.AreEqual(new DateTime(2013, 3, 13), result);
        }

        [TestMethod]
        public void OnXDayFrequencyComputeNextDateTest3()
        {
            OnXDayFrequency freq = new OnXDayFrequency();
            freq.DayOfWeek = DayOfWeek.Wednesday;
            freq.RankingPosition = RankingPosition.Third;
            DateTime referenceDate = new DateTime(2013, 2, 5);
            DateTime result = freq.ComputeNextDate(referenceDate);
            Assert.AreEqual(new DateTime(2013, 2, 20), result);
            referenceDate = new DateTime(2013, 2, 20);
            result = freq.ComputeNextDate(referenceDate);
            Assert.AreEqual(new DateTime(2013, 3, 20), result);
        }

        [TestMethod]
        public void OnXDayFrequencyComputeNextDateTest4()
        {
            OnXDayFrequency freq = new OnXDayFrequency();
            freq.DayOfWeek = DayOfWeek.Wednesday;
            freq.RankingPosition = RankingPosition.Fourth;
            DateTime referenceDate = new DateTime(2013, 2, 5);
            DateTime result = freq.ComputeNextDate(referenceDate);
            Assert.AreEqual(new DateTime(2013, 2, 27), result);
            referenceDate = new DateTime(2013, 2, 28);
            result = freq.ComputeNextDate(referenceDate);
            Assert.AreEqual(new DateTime(2013, 3, 27), result);
        }

        [TestMethod]
        public void OnXDayFrequencyComputeNextDateTest5()
        {
            OnXDayFrequency freq = new OnXDayFrequency();
            freq.DayOfWeek = DayOfWeek.Tuesday;
            freq.RankingPosition = RankingPosition.Last;
            DateTime referenceDate = new DateTime(2013, 2, 5);
            DateTime result = freq.ComputeNextDate(referenceDate);
            Assert.AreEqual(new DateTime(2013, 2, 26), result);
            referenceDate = new DateTime(2013, 2, 28);
            result = freq.ComputeNextDate(referenceDate);
            Assert.AreEqual(new DateTime(2013, 3, 26), result);
            referenceDate = new DateTime(2013, 3, 27);
            result = freq.ComputeNextDate(referenceDate);
            // It's the fifth tuesday in April
            Assert.AreEqual(new DateTime(2013, 4, 30), result);
        }
    }
}

using System;
using System.Globalization;
using System.Threading;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Model.Recurrence
{
    [TestClass]
    public class YearlyFrequencyTest
    {
        [TestMethod]
        public void Description_should_contains_only_month_and_day()
        {
            // setup
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");

            var frequency = new YearlyFrequency();
            frequency.SetReferenceDate(new DateTime(2014, 10, 14)); // Tuesday October 14th

            // act
            
            // check
            Assert.AreEqual("each October 14", frequency.DisplayValue);
        }
    }
}

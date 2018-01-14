using System;
using System.Globalization;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Tools.Extensions
{
    [TestClass]
    public class DateTimeExtensionsTest
    {
        [TestMethod]
        public void FormatMonthDay_Russia_1()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-ru");

            string str = DateTime.Now.FormatMonthDay(MonthDisplay.Full, false);

            Assert.IsFalse(str.Contains("."));
        }

        [TestMethod]
        public void FormatMonthDay_Russia_2()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-ru");

            string str = DateTime.Now.FormatMonthDay(MonthDisplay.Full, true);

            Assert.IsFalse(str.Contains("."));
        }

        [TestMethod]
        public void FormatMonthDay_France_1()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("fr-fr");

            string str = DateTime.Now.FormatMonthDay(MonthDisplay.Full, false);

            Assert.IsFalse(str.Contains("."));
        }

        [TestMethod]
        public void FormatMonthDay_France_2()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("fr-fr");

            string str = DateTime.Now.FormatMonthDay(MonthDisplay.Full, true);

            Assert.IsFalse(str.Contains("."));
        }

        [TestMethod]
        public void FormatWeekDayMonthDay_CurrentYear()
        {
            var now = DateTime.Now;

            string content = now.FormatWeekDayMonthDay();

            Assert.IsFalse(content.Contains(now.Year.ToString()));
        }

        [TestMethod]
        public void FormatWeekDayMonthDay_NextYear()
        {
            var now = DateTime.Now.AddYears(1);

            string content = now.FormatWeekDayMonthDay();

            Assert.IsTrue(content.Contains(now.Year.ToString()));
        }
    }
}

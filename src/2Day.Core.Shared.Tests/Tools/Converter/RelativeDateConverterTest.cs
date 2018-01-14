using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Converter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Chartreuse.Today.Core.Shared.Tests.Tools.Converter
{
    [TestClass]
    public class RelativeDateConverterTest
    {
        private DateTime now;
        private List<DateTime> dates;
        private bool useGroupedDates;

        [TestInitialize]
        public void Setup()
        {
            var workbook = new Mock<IWorkbook>();
            var settings = new Mock<ISettings>();
            settings.Setup(s => s.GetValue<bool>(CoreSettings.UseGroupedDates)).Returns(() => this.useGroupedDates);
            workbook.Setup(w => w.Settings).Returns(settings.Object);

            // Ioc is static => make sure we don't have another instance of workbook setup
            if(Ioc.HasType<IWorkbook>())
                Ioc.RemoveInstance<IWorkbook>();
    
            Ioc.RegisterInstance<IWorkbook, IWorkbook>(workbook.Object);

            // today: 3 May 2011
            this.now = new DateTime(2011, 5, 3);
            this.dates = new List<DateTime>
            {
                // 1 January 2010
                new DateTime(2010, 1, 1),
                // 2 February 2011
                new DateTime(2011, 2, 2),
                // before yesterday: 1 May 2011
                new DateTime(2011, 5, 1),
                // yesterday: 2 May 2011
                new DateTime(2011, 5, 2),
                // today: 3 May 2011
                new DateTime(2011, 5, 3),
                // tomorrow : 4 May 2011
                new DateTime(2011, 5, 4),
                // after tomorrow: 5 May 2011
                new DateTime(2011, 5, 5),
                // 6 May 2011
                new DateTime(2011, 5, 6),
                // 7 May 2011
                new DateTime(2011, 5, 7),
                // 8 May 2011
                new DateTime(2011, 5, 8),
                // 9 May 2011
                new DateTime(2011, 5, 9),
                // 10 May 2011
                new DateTime(2011, 5, 10),
                // 11 May 2011
                new DateTime(2011, 5, 11),
                // 1 June 2011
                new DateTime(2011, 6, 1),
                // 1 June 2012
                new DateTime(2012, 6, 1),
            };
        }

        [TestMethod]
        public void Convert()
        {
            this.useGroupedDates = true;

            var c = this.GetConvertedValues();

            // Late (x4)
            // Today (x1)
            // Tomorrow (x1)
            // Next 7 days (x6)
            // Future (x3)

            Assert.AreEqual(4, CountOccurence(c, "Late"));
            Assert.AreEqual(1, CountOccurence(c, "Today"));
            Assert.AreEqual(1, CountOccurence(c, "Tomorrow"));
            Assert.AreEqual(6, CountOccurence(c, "Next 7 days"));
            Assert.AreEqual(3, CountOccurence(c, "Future"));

            this.useGroupedDates = false;

            c = this.GetConvertedValues();

            // 01 Jan 2010
            // 02 Feb
            // 01 May
            // Yesterday 02 May
            // Today 03 May
            // Tomorrow 04 May
            // 05 May
            // 06 May
            // 07 May
            // 08 May
            // 09 May
            // 10 May
            // 11 May
            // 01 Jun
            // 01 Jun 2012
            Assert.AreEqual(1, CountOccurence(c, "Friday January 1 2010"));
            Assert.AreEqual(1, CountOccurence(c, "Wednesday February 2"));
            Assert.AreEqual(1, CountOccurence(c, "Sunday May 1"));
            Debug.WriteLine(Thread.CurrentThread.CurrentCulture);
            Debug.WriteLine(Thread.CurrentThread.CurrentUICulture);

            Debug.WriteLine(c[2]);
            Debug.WriteLine(c[3]);

            Assert.AreEqual(1, CountOccurence(c, "Yesterday Monday May 2"));
            Assert.AreEqual(1, CountOccurence(c, "Today Tuesday May 3"));
            Assert.AreEqual(1, CountOccurence(c, "Tomorrow Wednesday May 4"));
            Assert.AreEqual(1, CountOccurence(c, "Thursday May 5"));
            Assert.AreEqual(1, CountOccurence(c, "Friday May 6"));
            Assert.AreEqual(1, CountOccurence(c, "Saturday May 7"));
            Assert.AreEqual(1, CountOccurence(c, "Sunday May 8"));
            Assert.AreEqual(1, CountOccurence(c, "Monday May 9"));
            Assert.AreEqual(1, CountOccurence(c, "Tuesday May 10"));
            Assert.AreEqual(1, CountOccurence(c, "Wednesday May 11"));
            Assert.AreEqual(1, CountOccurence(c, "Wednesday June 1"));
            Assert.AreEqual(1, CountOccurence(c, "Friday June 1 2012"));
        }

        private List<string> GetConvertedValues()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            RelativeDateConverter.LoadResources();

            var result = new List<string>();
            foreach (var date in this.dates)
            {
                StaticTestOverrides.Now = this.now;
                string convertedValue = RelativeDateConverter.ConvertRelative(date);
                result.Add(convertedValue);
            }

            return result;
        }

        private static int CountOccurence(List<string> source, string match)
        {
            return source.Count(s => s.Equals(match, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

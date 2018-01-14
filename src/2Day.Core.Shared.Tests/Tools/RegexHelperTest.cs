using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Chartreuse.Today.Core.Shared.Tests.Tools
{
    [TestClass]
    public class RegexHelperTest
    {
        private readonly IPlatformService platformService;

        public RegexHelperTest()
        {
            var mock = new Mock<IPlatformService>();
            mock.Setup(p => p.DeviceFamily).Returns(DeviceFamily.WindowsMobile);
            this.platformService = mock.Object;
        }

        [TestMethod]
        public void France1()
        {
            Assert.AreEqual("0479369005", this.FindPhoneNumber("hello 0479369005 world"));
        }

        [TestMethod]
        public void France2()
        {
            Assert.AreEqual("04 79 36 90 05", this.FindPhoneNumber("hello 04 79 36 90 05 world"));
        }

        [TestMethod]
        public void German1()
        {
            Assert.AreEqual("02151921506", this.FindPhoneNumber("hello 02151921506 world"));
        }

        [TestMethod]
        public void German2()
        {
            Assert.AreEqual("02 15 19 21 506", this.FindPhoneNumber("hello 02 15 19 21 506 world"));
        }

        [TestMethod]
        public void US1()
        {
            Assert.AreEqual("123 456 7890", this.FindPhoneNumber("hello 123 456 7890 world"));
        }

        [TestMethod]
        public void International1()
        {
            Assert.AreEqual("00 33 6 18 12 78 12", this.FindPhoneNumber("hello 00 33 6 18 12 78 12 world"));
        }

        [TestMethod]
        public void International2()
        {
            Assert.AreEqual("+33 6 18 12 78 12", this.FindPhoneNumber("hello +33 6 18 12 78 12 world"));
        }

        [TestMethod]
        public void NotNumber1()
        {
            Assert.AreEqual(null, this.FindPhoneNumber("hello 38360 world"));
        }

        [TestMethod]
        public void Uri_not()
        {
            string input = "hello";

            var uris = this.FindUri(input);

            Assert.AreEqual(0, uris.Count);
        }

        [TestMethod]
        public void Uri_simple_http()
        {
            string input = "http://www.google.fr";

            var uris = this.FindUri(input);

            Assert.AreEqual(1, uris.Count);
            Assert.AreEqual(input, uris[0]);
        }

        [TestMethod]
        public void Uri_simple_www()
        {
            string input = "www.google.fr";

            var uris = this.FindUri(input);

            Assert.AreEqual(1, uris.Count);
            Assert.AreEqual(input, uris[0]);
        }

        [TestMethod]
        public void Uri_in_sentence()
        {
            string input = "hello www.google.fr world ";

            var uris = this.FindUri(input);

            Assert.AreEqual(1, uris.Count);
            Assert.AreEqual("www.google.fr", uris[0]);
        }

        [TestMethod]
        public void Uri_multiple()
        {
            string input = "hello www.google.fr world www.2day-app.com cool";

            var uris = this.FindUri(input);

            Assert.AreEqual(2, uris.Count);
            Assert.AreEqual("www.google.fr", uris[0]);
            Assert.AreEqual("www.2day-app.com", uris[1]);
        }

        [TestMethod]
        public void Uri_colon()
        {
            string input = "https://parclick.fr/parking-barcelone/bsm_moll_de_la_fusta?lt=41.380995422774&ln=2.1819662534424&z=15&df=2016-06-29+12:00&dt=2016-07-01+18:00&ft=1";

            var uris = this.FindUri(input);

            Assert.AreEqual(1, uris.Count);
            Assert.AreEqual(input, uris[0]);
        }

        private List<string> FindUri(string input)
        {
            return RegexHelper.FindUris(input);
        }

        private string FindPhoneNumber(string input)
        {
            return RegexHelper.FindPhoneNumber(this.platformService, input);
        }
    }
}

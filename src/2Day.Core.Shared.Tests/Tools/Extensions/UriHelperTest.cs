using System;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Tools.Extensions
{
    [TestClass]
    public class UriHelperTest
    {
        [TestMethod]
        public void Get_scheme_1()
        {
            var result = SafeUri.Get("https://abc.contoso.com").GetSchemeAndHost();

            Assert.AreEqual("https://abc.contoso.com", result);
        }

        [TestMethod]
        public void Get_scheme_2()
        {
            var result = SafeUri.Get("https://abc.contoso.com/").GetSchemeAndHost();

            Assert.AreEqual("https://abc.contoso.com", result);
        }

        [TestMethod]
        public void Get_scheme_3()
        {
            var result = SafeUri.Get("https://abc.contoso.com").GetSchemeAndHost();

            Assert.AreEqual("https://abc.contoso.com", result);
        }

        [TestMethod]
        public void Get_scheme_4()
        {
            var result = SafeUri.Get("https://abc.contoso.com/").GetSchemeAndHost();

            Assert.AreEqual("https://abc.contoso.com", result);
        }

        [TestMethod]
        public void Get_scheme_5()
        {
            var result = SafeUri.Get("https://abc.contoso.com/test").GetSchemeAndHost();

            Assert.AreEqual("https://abc.contoso.com", result);
        }

        [TestMethod]
        public void Get_scheme_6()
        {
            var result = SafeUri.Get("https://abc.contoso.com/test/").GetSchemeAndHost();

            Assert.AreEqual("https://abc.contoso.com", result);
        }

        [TestMethod]
        public void Get_scheme_7()
        {
            var result = SafeUri.Get("https://abc.contoso.com?test=true").GetSchemeAndHost();

            Assert.AreEqual("https://abc.contoso.com", result);
        }

        [TestMethod]
        public void Get_scheme_8()
        {
            var result = SafeUri.Get("abc.contoso.com#test", UriKind.RelativeOrAbsolute).GetSchemeAndHost();

            Assert.AreEqual("https://abc.contoso.com", result);
        }
    }
}

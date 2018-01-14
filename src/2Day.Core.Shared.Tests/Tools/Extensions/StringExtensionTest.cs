using System.Text;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.Tools.Extensions
{
    [TestClass]
    public class StringExtensionTest
    {
        [TestMethod]
        public void EscapeXml_1()
        {
            string input = "<hello>";

            Assert.AreEqual("&lt;hello&gt;", input.ToEscapedXml());
        }

        [TestMethod]
        public void EscapeXml_2()
        {
            string input = string.Empty;

            Assert.AreEqual(string.Empty, input.ToEscapedXml());
        }

        [TestMethod]
        public void EscapeXml_3()
        {
            string input = null;

            Assert.AreEqual(string.Empty, input.ToEscapedXml());
        }

        [TestMethod]
        public void EscapeXml_4()
        {
            string input = ((char)'\xD7FF').ToString();

            Assert.AreEqual(input, input.ToEscapedXml());
        }

        [TestMethod]
        public void EscapeXml_5()
        {
            string input = ((char)'\xD7FF' + 1).ToString();

            Assert.AreEqual(input, input.ToEscapedXml());
        }

        [TestMethod]
        public void EscapeXml_6()
        {
            string input = "Название задания";

            Assert.AreEqual(input, input.ToEscapedXml());
        }

        [TestMethod]
        public void EscapeXml_7()
        {
            string input = "Δημήτρης Φόλλας";

            Assert.AreEqual(input, input.ToEscapedXml());
        }

        [TestMethod]
        public void TryGetHyperlink_1()
        {
            string input = null;

            Assert.AreEqual(null, input.TryGetHyperlink());
        }

        [TestMethod]
        public void TryGetHyperlink_2()
        {
            string input = "http://www.google.fr";

            Assert.AreEqual("http://www.google.fr", input.TryGetHyperlink());
        }

        [TestMethod]
        public void TryGetHyperlink_3()
        {
            string input = "    http://www.google.fr    ";

            Assert.AreEqual("http://www.google.fr", input.TryGetHyperlink());
        }

        [TestMethod]
        public void TryGetHyperlink_4()
        {
            string input = "Link: http://www.google.fr    ";

            Assert.AreEqual("http://www.google.fr", input.TryGetHyperlink());
        }

        [TestMethod]
        public void TryGetHyperlink_5()
        {
            string input = "Link:http://www.google.fr    ";

            Assert.AreEqual("http://www.google.fr", input.TryGetHyperlink());
        }

        [TestMethod]
        public void TryGetHyperlink_6()
        {
            string input = "www.google.fr";

            Assert.AreEqual("www.google.fr", input.TryGetHyperlink());
        }

        [TestMethod]
        public void TryGetHyperlink_7()
        {
            string input = "      ";

            Assert.AreEqual(null, input.TryGetHyperlink());
        }

        [TestMethod]
        public void TryGetHyperlink_8()
        {
            string input = "Link1: www.google.fr Link2: www.bing.com";

            Assert.AreEqual("www.google.fr", input.TryGetHyperlink());
        }

        [TestMethod]
        public void HasHtml_1()
        {
            Assert.IsFalse(string.Empty.HasHtml());
        }

        [TestMethod]
        public void HasHtml_2()
        {
            Assert.IsFalse("hello".HasHtml());
        }

        [TestMethod]
        public void HasHtml_3()
        {
            Assert.IsFalse("hello > you".HasHtml());
        }

        [TestMethod]
        public void HasHtml_4()
        {
            Assert.IsFalse("hello < you".HasHtml());
        }

        [TestMethod]
        public void HasHtml_5()
        {
            Assert.IsFalse("hello < you >".HasHtml());
        }

        [TestMethod]
        public void HasHtml_6()
        {
            Assert.IsFalse("hello<1>".HasHtml());
        }
        
        [TestMethod]
        public void HasHtml_7()
        {
            Assert.IsTrue("hello <you> <p>".HasHtml());
        }

        [TestMethod]
        public void HasHtml_8()
        {
            Assert.IsFalse("hello <1>".HasHtml());
        }

        [TestMethod]
        public void HasHtml_9()
        {
            Assert.IsTrue("hello <h1>".HasHtml());
        }

        [TestMethod]
        public void HasHtml_10()
        {
            Assert.IsTrue("hello <h1 >".HasHtml());
        }

        [TestMethod]
        public void HasHtml_11()
        {
            string note = @"hello ""world""";

            Assert.IsFalse(note.HasHtml());
        }

        [TestMethod]
        public void EscapeLongDataString()
        {
            StringBuilder builder= new StringBuilder();
            for (int i = 0; i < 15000; i++)
            {
                builder.Append("a<?:/");
            }

            string content = builder.ToString();

            // we don't expect no exception :-)
            string escaped = content.EscapeLongDataString();
        }
    }
}

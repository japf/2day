using Chartreuse.Today.Exchange.Ews.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Ews
{
    [TestClass]
    public class EwsTaskSchemaTest
    {
        [TestMethod]
        public void BuildSetFieldValueXml_body_1_value_no_tag()
        {
            string xml = EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.Body, "value1");

            Assert.AreEqual("<t:Body>value1</t:Body>", xml);
        }

        [TestMethod]
        public void BuildSetFieldValueXml_body_2_values_no_tag()
        {
            string xml = EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.Body, new [] { "value1", "value2" });

            Assert.AreEqual("<t:Body>value1</t:Body>", xml);
        }

        [TestMethod]
        public void BuildSetFieldValueXml_body_1_value_tag()
        {
            string xml = EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.Body, "<value1>");

            Assert.AreEqual("<t:Body>&lt;value1&gt;</t:Body>", xml);
        }

        [TestMethod]
        public void BuildSetFieldValueXml_body_2_values_tag()
        {
            string xml = EwsTaskSchema.BuildSetFieldValueXml(EwsFieldUri.Body, new[] { "<value1>", "<value2>" });
            Assert.AreEqual("<t:Body>&lt;value1&gt;</t:Body>", xml);
        }
    }
}

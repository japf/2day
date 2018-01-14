using Chartreuse.Today.Exchange.ActiveSync.Commands;
using Chartreuse.Today.Exchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Eas
{
    [TestClass]
    public class SyncCommandParameterTest
    {
        [TestMethod]
        public void Handle_special_character()
        {
            // setup
            var changeset = new ExchangeChangeSet();
            changeset.AddedTasks.Add(new ExchangeTask { Subject = "title /{ test" });
            changeset.AddedTasks.Add(new ExchangeTask { Subject = "title {} test" });
            changeset.AddedTasks.Add(new ExchangeTask { Subject = "title #d.3 test" });

            // act
            var parameter = new SyncCommandParameter("syncKey", "folderId", changeset);
            string xml = parameter.BuildXml("command");

            // check
            Assert.IsFalse(string.IsNullOrWhiteSpace(xml));
        }

        [TestMethod]
        public void Encode_number()
        {
            EncodingTest("hello 1234567890");
        }

        [TestMethod]
        public void Encode_russian()
        {
            EncodingTest("Название задания");
        }

        [TestMethod]
        public void Encode_misc()
        {
            EncodingTest("Hello, world <> &é\"'(-è_çà)=~#{[|`\\^@]} ^$ù*,;:!¨£%µ?./§ \n new line",
                       "Hello, world &lt;&gt; &amp;é\"'(-è_çà)=~#{[|`\\^@]} ^$ù*,;:!¨£%µ?./§ \r\n new line");
        }

        [TestMethod]
        public void Escape_invalid_xml_character()
        {
            // non-reg test for the following exception (Windows 8 only apparently)
            // ActiveSync: System.ArgumentException: ' ', hexadecimal value 0x0B, is an invalid character. 
            // at System.Xml.XmlEncodedRawTextWriter.InvalidXmlChar(Int32 ch, Char* pDst, Boolean entitize) 
            // ...
            // at Chartreuse.Today.Exchange.ActiveSync.Commands.SyncCommandParameter.CreateTaskXml(ExchangeTask task, StringBuilder builder, Boolean isAdd) 
            // at Chartreuse.Today.Exchange.ActiveSync.Commands.SyncCommandParameter.AppendAddedTasks(StringBuilder xmlBuilder) at 

            // setup
            var changeset = new ExchangeChangeSet();
            changeset.AddedTasks.Add(new ExchangeTask { Subject = "hello " + (char)0x0B + "world !" });

            // act
            var parameter = new SyncCommandParameter("syncKey", "folderId", changeset);
            string xml = parameter.BuildXml("command");

            // check
            Assert.IsFalse(string.IsNullOrWhiteSpace(xml));
            Assert.IsTrue(xml.Contains("hello world !"));
        }

        private static void EncodingTest(string input, string output = null)
        {
            // setup
            var changeset = new ExchangeChangeSet();
            changeset.AddedTasks.Add(new ExchangeTask { Subject = input });

            // act
            var parameter = new SyncCommandParameter("syncKey", "folderId", changeset);
            string xml = parameter.BuildXml("command");

            // check
            const string subjectFormat = "<tasks:Subject>{0}</tasks:Subject>";
            string subject = string.Format(subjectFormat, output ?? input);
            Assert.IsTrue(xml.Contains(subject));
        }
    }
}
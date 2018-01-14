using System.Xml.Linq;
using Chartreuse.Today.Exchange.ActiveSync.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Eas
{
    [TestClass]
    public class FolderSyncCommandResultTest
    {
        [TestMethod]
        public void Manage_no_status_no_sync_key()
        {
            var command = new CustomFolderSyncCommandResult();

            command.Parse(@"
                <FolderSync xmlns:folderhierarchy='FolderHierarchy'>
                </FolderSync>"
                );

            Assert.AreEqual(null, command.Status);
            Assert.AreEqual(null, command.SyncKey);
        }

        [TestMethod]
        public void Manage_status()
        {
            var command = new CustomFolderSyncCommandResult();

            command.Parse(@"
                <FolderSync xmlns:folderhierarchy='FolderHierarchy'>
                    <Status xmlns:folderhierarchy='FolderHierarchy'>110</Status>
                </FolderSync>"
                );

            Assert.AreEqual("110", command.Status);
            Assert.AreEqual(null, command.SyncKey);
        }

        [TestMethod]
        public void Manage_sync_key()
        {
            var command = new CustomFolderSyncCommandResult();

            command.Parse(@"
                <FolderSync xmlns:folderhierarchy='FolderHierarchy'>
                    <SyncKey xmlns:folderhierarchy='FolderHierarchy'>99</SyncKey>
                </FolderSync>"
                );

            Assert.AreEqual(null, command.Status);
            Assert.AreEqual("99", command.SyncKey);
        }

        [TestMethod]
        public void Manage_status_and_sync_key()
        {
            var command = new CustomFolderSyncCommandResult();

            command.Parse(@"
                <FolderSync xmlns:folderhierarchy='FolderHierarchy'>
                    <Status xmlns:folderhierarchy='FolderHierarchy'>110</Status>
                    <SyncKey xmlns:folderhierarchy='FolderHierarchy'>99</SyncKey>
                </FolderSync>"
                );

            Assert.AreEqual("110", command.Status);
            Assert.AreEqual("99", command.SyncKey);
        }

        private class CustomFolderSyncCommandResult : FolderSyncCommandResult
        {
            public void Parse(string xml)
            {
                this.ParseResponseCore("FolderSync", XDocument.Parse(xml), null);
            }
        }
    }
}
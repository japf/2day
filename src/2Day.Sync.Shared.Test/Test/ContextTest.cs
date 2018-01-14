using System;
using System.Linq;
#if SYNC_TOODLEDO || SYNC_VERCORS
using System.Threading.Tasks;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.TestCase
{
    [TestClass]
    public class ContextTest : TestCaseBase
    {
        [TestMethod]
        public async Task Context_rename()
        {
            if (!this.Runner.SupportContext)
            {
                Assert.Inconclusive();
                return;
            }

            var context = this.CreateContext("context");

            await this.SyncDelta();

            context.Name = "new name";

            await this.SyncFull();

            AssertEx.ContainsContexts(this.Workbook, "new name");
        }

        [TestMethod]
        public async Task Context_delete()
        {
            if (!this.Runner.SupportContext)
            {
                Assert.Inconclusive();
                return;
            }

            var context1 = this.CreateContext("context1");
            var context2 = this.CreateContext("context2");

            await this.SyncDelta();

            AssertEx.ContainsContexts(this.Workbook, "context1", "context2");

            this.RemoveContext(context1);

            await this.SyncFull();

            AssertEx.ContainsContexts(this.Workbook, "context2");
        }

        [TestMethod]
        public async Task Context_duplicate()
        {
            if (!this.Runner.SupportContext)
            {
                Assert.Inconclusive();
                return;
            }

            var context = this.CreateContext("context");

            await this.SyncFull();

            // remove and re-create the same folder
            // that will cause an UpdateFolder request while this folder already exists
            this.Workbook.RemoveContext(context.Name);
            context = this.CreateContext("context");

            await this.SyncDelta();

            context.Name = "new name";

            await this.SyncDelta();
            await this.SyncFull();

            Assert.AreEqual(1, this.Workbook.Contexts.Count);
            Assert.AreEqual(context.Name, this.Workbook.Contexts[0].Name);
        }

        [TestMethod]
        public async Task Context_scenario()
        {
            if (!this.Runner.SupportContext)
            {
                Assert.Inconclusive();
                return;
            }

            var context1 = this.CreateContext("context1");

            await this.SyncDelta();

            // check remote has context1
            var contexts = await this.Runner.RemoteGetAllContexts();
            AssertEx.ContainsContexts(contexts, "context1");

            // add context2 in remote
            await this.Runner.RemoteAddContext("context2");

            await this.SyncDelta();

            AssertEx.ContainsContexts(this.Workbook, "context1", "context2");

            this.RemoveContext(context1);
            var context3 = this.CreateContext("context3");

            // remove context2 in remote
            await this.Runner.RemoteDeleteContext("context2");
            // add context4 in remote
            await this.Runner.RemoteAddContext("context4");

            await this.SyncDelta();

            AssertEx.ContainsContexts(this.Workbook, "context3", "context4");

            context3.Name = "context3_renamed";

            await this.SyncDelta();

            // make sure context3 has been renamed remotely
            var remoteContexts = await this.Runner.RemoteGetAllContexts();
            Assert.IsTrue(remoteContexts.Contains("context3_renamed"));

            // for testing purpose
            // make several edits "too fast" breaks test on CI because the timestamp is unchanged
            await Task.Delay(TimeSpan.FromSeconds(5));

            // rename context 4 remotely
            await this.Runner.RemoteEditContext(this.Workbook.Contexts.First(f => f.Name == "context4").SyncId, "context4_renamed");

            await this.SyncDelta();

            AssertEx.ContainsContexts(this.Workbook, "context3_renamed", "context4_renamed");

            this.RemoveContext(context3);

            await this.SyncDelta();

            // check remote has only context4_renamed
            remoteContexts = await this.Runner.RemoteGetAllContexts();
            Assert.AreEqual(1, remoteContexts.Count);
            Assert.IsTrue(remoteContexts.Contains("context4_renamed"));

            // delete remote context4_renamed
            await this.Runner.RemoteDeleteContext("context4_renamed");

            // for testing purpose
            // make several edits "too fast" breaks test on CI because the timestamp is unchanged
            await Task.Delay(TimeSpan.FromSeconds(5));

            await this.SyncDelta();

            AssertEx.ContainsContexts(this.Workbook);
        }
    }
}
#endif
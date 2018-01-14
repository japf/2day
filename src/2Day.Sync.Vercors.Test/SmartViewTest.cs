using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.TestCase.Vercors
{
    [TestClass]
    public class SmartViewTest : TestCaseBase
    {
        [TestMethod]
        public async Task Smartview_simple()
        {
            var smartview = this.CreateSmartView("smartview");
            smartview.Rules = "(Priority is Low)";

            await this.SyncFull();
            
            Assert.AreEqual(1, this.Workbook.SmartViews.Count);
            Assert.AreEqual(smartview.Name, this.Workbook.SmartViews.ElementAt(0).Name);
            Assert.AreEqual("(Priority is Low)", this.Workbook.SmartViews.ElementAt(0).Rules);
        }

        [TestMethod]
        public async Task Smartview_duplicate()
        {
            var smartview = this.CreateSmartView("smartview");

            await this.SyncFull();

            // remove and re-create the same smartview
            // that will cause an UpdateSmartView request while this folder already exists
            // in 2Day Cloud :-)
            this.Workbook.RemoveSmartView(smartview.Name);
            smartview = this.CreateSmartView("smartview");

            await this.SyncDelta();

            smartview.Name = "new name";

            await this.SyncDelta();
            await this.SyncFull();

            Assert.AreEqual(1, this.Workbook.SmartViews.Count);
            Assert.AreEqual(smartview.Name, this.Workbook.SmartViews.ElementAt(0).Name);
        }

        [TestMethod]
        public async Task Smartview_remove()
        {
            var smartview = this.CreateSmartView("smartview");

            await this.SyncDelta();

            this.Workbook.RemoveSmartView(smartview.Name);

            await this.SyncFull();

            Assert.AreEqual(0, this.Workbook.SmartViews.Count);
        }

        [TestMethod]
        public async Task Smartview_remove_remote()
        {
            var smartview = this.CreateSmartView("smartview");

            await this.SyncDelta();

            await this.Runner.RemoteDeleteSmartView(smartview.Name);

            await this.SyncDelta();

            Assert.AreEqual(0, this.Workbook.SmartViews.Count);
        }

        [TestMethod]
        public async Task Smartview_edit_remote()
        {
            var smartview = this.CreateSmartView("smartview");
            smartview.Rules = "(Priority is Low)";

            await this.SyncDelta();

            await this.Runner.RemoteEditSmartView(smartview.SyncId, "new name", "(Due DoesNotExist 0)");

            await this.SyncDelta();

            Assert.AreEqual(1, this.Workbook.SmartViews.Count);
            Assert.AreEqual("new name", this.Workbook.SmartViews[0].Name);
            Assert.AreEqual("(Due DoesNotExist 0)", this.Workbook.SmartViews[0].Rules);

        }
    }
}
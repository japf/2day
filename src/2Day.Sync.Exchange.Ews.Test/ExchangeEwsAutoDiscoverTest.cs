using System.Threading.Tasks;
using Chartreuse.Today.Exchange;
using Chartreuse.Today.Exchange.Providers;
using Chartreuse.Today.Sync.Test.Runners;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Exchange.Ews.Test
{
    [TestClass]
    public class ExchangeEwsAutoDiscoverTest : TestCaseBase<ExchangeEwsSynchronizationProvider, ExchangeEwsTestRunner>
    {
        [TestMethod]
        public async Task Detect_bad_uri()
        {
            this.Workbook.Settings.SetValue(ExchangeSettings.ExchangeServerUri, "https://outlook.office365.com/ews/exchange.asmx/ews/exchange.asmx");

            bool result = await this.Provider.CheckLoginAsync();

            Assert.IsTrue(result);

            var newUri = this.Workbook.Settings.GetValue<string>(ExchangeSettings.ExchangeServerUri).ToLowerInvariant();
            Assert.AreEqual("https://outlook.office365.com/ews/exchange.asmx", newUri);
        }
    }
}
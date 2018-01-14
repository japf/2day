using System.Threading.Tasks;
using Chartreuse.Today.Exchange.Ews.AutoDiscover;
using Chartreuse.Today.Exchange.Model;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Ews
{
    [TestClass]
    public class EwsAutoDiscoverTest : EwsTestBase
    {
        [TestMethod]
        public async Task Discover_server_uri_Exchange()
        {
            var engine = new AutoDiscoverEngine();

            var result = await engine.AutoDiscoverAsync("test.exchange@eclyps.info", "username", "interdit", ExchangeServerVersion.ExchangeOffice365);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Discover_server_uri_Office365()
        {
            if (TestExchangeSyncHelper.Version != ExchangeServerVersion.ExchangeOffice365)
            {
                Assert.Inconclusive("Supported only with Office 365");
            }

            var engine = new AutoDiscoverEngine();

            var result = await engine.AutoDiscoverAsync(this.settings.Email, this.settings.Username, this.settings.Password, ExchangeServerVersion.ExchangeOffice365);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ServerUri);
            Assert.AreEqual("https://outlook.office365.com/ews/exchange.asmx", result.ServerUri.ToString().ToLowerInvariant());
        }
    }
}

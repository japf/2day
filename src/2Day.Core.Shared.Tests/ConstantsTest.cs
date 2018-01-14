using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests
{
    /// <summary>
    /// Very simple unit tests to make sure we don't push development-mode endpoint :-)
    /// </summary>
    [TestClass]
    public class ConstantsTest
    {
        [TestMethod]
        public void Exchange_endpoint_is_correct()
        {
            Assert.AreEqual("https://2day-exchange.azurewebsites.net/", Constants.AzureExchangeServiceAdress);      
        }

        [TestMethod]
        public void Office365_endpoint_is_correct()
        {
            Assert.AreEqual("https://outlook.office365.com", Constants.Office365Endpoint);      
        }
    }
}

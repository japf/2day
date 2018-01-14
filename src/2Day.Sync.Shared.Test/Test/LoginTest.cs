using System.Threading.Tasks;
using Chartreuse.Today.Sync.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if SYNC_EXCHANGEEWS || SYNC_EXCHANGEWEBAPI
using Chartreuse.Today.Exchange;
using Chartreuse.Today.Exchange.Model;
#endif

namespace Chartreuse.Today.Sync.Test.TestCase
{
    [TestClass]
    public class LoginTest : TestCaseBase
    {
        [TestMethod]
        public async Task Success()
        {
            var result = await this.Provider.CheckLoginAsync();

            Assert.IsTrue(result);
        }

#if SYNC_EXCHANGEEWS || SYNC_EXCHANGEWEBAPI
        [TestMethod]
        public async Task Wrong_credentials()
        {
            var oldPassword = this.Runner.Password;
            this.Runner.Password = "wrongPassword";

            string errorMessage = null;
            this.Provider.OperationFailed += (s, e) =>
            {
                errorMessage = e.Item;
            };

            var result = await this.Provider.CheckLoginAsync();

            this.Runner.Password = oldPassword;
            
            Assert.IsFalse(result);
            Assert.IsNotNull(errorMessage);
            Assert.IsTrue(errorMessage.Contains(ExchangeAuthorizationStatus.UserCredentialsInvalid.ToReadableString()));
        }
#endif
    }
}
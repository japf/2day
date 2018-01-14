using Chartreuse.Today.App.Shared.ViewModel.Sync;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tests.Impl;
using Chartreuse.Today.Exchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests.ViewModel.Sync
{
    [TestClass]
    public class ExchangeSettingsViewModelTest
    {
        private ExchangeSettingsViewModel viewmodel;

        [TestInitialize]
        public void Initialize()
        {
            var mockedObject = MockBasedFactory.Build<ExchangeSettingsViewModel>((IWorkbook)new Workbook(new TestDatabaseContext(), new TestSettings()));

            this.viewmodel = mockedObject.Object;
            this.viewmodel.ServerUri = null;
        }

        [TestMethod]
        public void Setting_office365_must_set_server_uri_if_it_is_empty()
        {
            // setup
            this.viewmodel.ExchangeVersion = ExchangeServerVersion.Exchange2010.GetString();

            // act
            this.viewmodel.ExchangeVersion = ExchangeServerVersion.ExchangeOffice365.GetString();

            // check
            Assert.AreEqual(Constants.Office365Endpoint, this.viewmodel.ServerUri);
        }

        [TestMethod]
        public void Setting_office365_must_not_set_server_uri_if_it_is_not_empty()
        {
            // setup
            this.viewmodel.ExchangeVersion = ExchangeServerVersion.Exchange2010.GetString();
            this.viewmodel.ServerUri = "https://contoso.com";

            // act
            this.viewmodel.ExchangeVersion = ExchangeServerVersion.ExchangeOffice365.GetString();

            // check
            Assert.AreEqual("https://contoso.com", this.viewmodel.ServerUri);
        }

        [TestMethod]
        public void Not_setting_office365_must_remove_server_uri_if_needed()
        {
            // setup
            this.viewmodel.ExchangeVersion = ExchangeServerVersion.ExchangeOffice365.GetString();

            // act
            this.viewmodel.ExchangeVersion = ExchangeServerVersion.Exchange2010.GetString();

            // check
            Assert.IsTrue(string.IsNullOrEmpty(this.viewmodel.ServerUri));
        }
    }
}

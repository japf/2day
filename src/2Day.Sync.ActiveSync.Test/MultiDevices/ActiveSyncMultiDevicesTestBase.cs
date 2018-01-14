using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tests.Impl;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Exchange;
using Chartreuse.Today.Exchange.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Chartreuse.Today.Sync.ActiveSync.Test.MultiDevices
{
    public abstract class ActiveSyncMultiDevicesTestBase
    {
        private readonly string email;
        private readonly string password;
        private readonly string server;

        protected readonly TestCryptoService cryptoService;

        public TestContext TestContext { get; set; }

        protected ActiveSyncMultiDevicesTestBase(string email, string password, string server)
        {
            this.email = email;
            this.password = password;
            this.server = server;
            this.cryptoService = new TestCryptoService();
        }

        protected SynchronizationManager BuildSynchronizationManager(string deviceId, IWorkbook workbook)
        {
            var manager = new SynchronizationManager(new TestPlatformService(), new Mock<ITrackingManager>().Object, "test", false);

            manager.RegisterProvider(SynchronizationService.ActiveSync, () => new ActiveSyncSynchronizationProvider(manager, this.cryptoService, deviceId));
            manager.AttachWorkbook(workbook);
            manager.ActiveService = SynchronizationService.ActiveSync;

            workbook.Settings.SetValue(ExchangeSettings.ActiveSyncEmail, this.email);
            workbook.Settings.SetValue(ExchangeSettings.ActiveSyncPassword, this.cryptoService.Encrypt(this.password));
            workbook.Settings.SetValue(ExchangeSettings.ActiveSyncServerUri, this.server);

            return manager;
        }

        protected IWorkbook CreateWorkbook()
        {
            return new Workbook(new TestDatabaseContext(), new TestSettings());
        }
    }
}
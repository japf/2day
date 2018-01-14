using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tests.Impl;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Sync.Test.Runners;
using Moq;
using Newtonsoft.Json;

namespace Chartreuse.Today.Sync.Test.Tools
{
    public abstract class TestRunnerBase
    {
        private readonly SynchronizationService service;
        private readonly string providerDataFileName;
        private readonly IWorkbook workbook;
        private readonly ISynchronizationManager manager;
        private readonly ICryptoService cryptoService;
        private readonly TestDatabaseContext databaseContext;

        public ISynchronizationManager Manager
        {
            get { return this.manager; }
        }

        public IWorkbook Workbook
        {
            get { return this.workbook; }
        }

        public ICryptoService CryptoService
        {
            get { return this.cryptoService; }
        }

        public abstract string Password { get; set; }

        public TestDatabaseContext DatabaseContext
        {
            get { return this.databaseContext; }
        }

        public abstract bool SupportContext { get; }

        public abstract bool SupportAlarm { get; }

        public abstract bool SupportFolder { get; }

        public abstract bool SupportTag { get; }

        protected TestRunnerBase(SynchronizationService service, string testName, string providerDataFileName)
        {
            if (testName == null)
                throw new ArgumentNullException("testName");
            if (providerDataFileName == null)
                throw new ArgumentNullException("providerDataFileName");

            this.service = service;
            this.providerDataFileName = providerDataFileName;

            TestLogHandler.Initialize(string.Format("{0}-trace-{1}", this.service.ToString().ToLowerInvariant(), testName));

            this.databaseContext = new TestDatabaseContext();
            this.workbook = new Workbook(this.DatabaseContext, new TestSettings());
            this.manager = new SynchronizationManager(new TestPlatformService(), new Mock<ITrackingManager>().Object, "test", false);

            this.cryptoService = new TestCryptoService();

#if SYNC_TOODLEDO
            this.manager.RegisterProvider(SynchronizationService.ToodleDo, () => new Chartreuse.Today.ToodleDo.ToodleDoSynchronizationProvider(this.manager, this.CryptoService));
#elif SYNC_EXCHANGEWEBAPI
            this.manager.RegisterProvider(SynchronizationService.Exchange, () => new Chartreuse.Today.Exchange.Providers.ExchangeSynchronizationProvider(this.manager, this.CryptoService));
#elif SYNC_EXCHANGEEWS
            this.manager.RegisterProvider(SynchronizationService.ExchangeEws, () => new Chartreuse.Today.Exchange.Providers.ExchangeEwsSynchronizationProvider(this.manager, this.CryptoService));
#elif SYNC_ACTIVESYNC
            this.manager.RegisterProvider(SynchronizationService.ActiveSync, () => new Chartreuse.Today.Exchange.Providers.ActiveSyncSynchronizationProvider(this.manager, this.CryptoService, ActiveSyncTestRunner.DeviceId));
            this.manager.RegisterProvider(SynchronizationService.OutlookActiveSync, () => new Chartreuse.Today.Exchange.Providers.ActiveSyncSynchronizationProvider(this.manager, this.CryptoService, ActiveSyncTestRunner.DeviceId));
#elif SYNC_VERCORS
            TestSettings.Instance.SetValue(CoreSettings.SyncAuthToken, Runners.VercorsTestRunner.Token);
            this.manager.RegisterProvider(SynchronizationService.Vercors, () => new Today.Vercors.Shared.VercorsSynchronizationProvider(this.manager, this.CryptoService, new Chartreuse.Today.Shared.Sync.Vercors.VercorsService()));
#endif

            this.manager.AttachWorkbook(this.workbook);

            this.Workbook.Settings.SetValue(CoreSettings.AutoDeleteFrequency, AutoDeleteFrequency.Never);

            this.Manager.ActiveService = this.service;
        }

        public async Task BeforeTest(bool remoteDeleteAll = true)
        {
            this.LoadProviderData();

            this.BeforeTestCore();

            this.workbook.RemoveAll();
            var currentService = this.Manager.ActiveService;
            this.Manager.Reset(clearSettings: false);
            this.Manager.ActiveService = SynchronizationService.None;
            this.Manager.ActiveService = currentService;

            if (remoteDeleteAll)
            {
                await this.RemoteDeleteAllTasks();
                await this.RemoteDeleteAllContexts();
                await this.RemoteDeleteAllFolders();
                await this.RemoteDeleteAllSmartViews();
            }

            await this.BeforeTestCoreAsync();
        }

        public void AfterTest()
        {
            this.AfterTestCore();

            this.SaveProviderData();
        }

        protected virtual void LoadProviderData()
        {
            if (File.Exists(this.providerDataFileName))
            {
                // delete provider data if it's older than 30 minutes
                var fileInfo = new FileInfo(this.providerDataFileName);
                if ((DateTime.Now - fileInfo.LastWriteTime).TotalMinutes > 30)
                {
                    File.Delete(this.providerDataFileName);
                    return;
                }

                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(this.providerDataFileName));
                if (data != null)
                {
                    ((SynchronizationMetadata) this.Manager.Metadata).ProviderDatas = data;
                }
            }
        }

        protected virtual void SaveProviderData()
        {
            File.WriteAllText(this.providerDataFileName, JsonConvert.SerializeObject(this.Manager.Metadata.ProviderDatas));
        }

        public abstract void BeforeTestCore();

        public virtual void AfterTestCore()
        {
            
        }

        protected virtual Task BeforeTestCoreAsync()
        {
            return Task.FromResult(0);
        }

        public abstract Task RemoteDeleteAllTasks();

        public virtual Task RemoteDeleteAllContexts()
        {
            return Task.FromResult(0);
        }

        public virtual Task RemoteDeleteAllFolders()
        {
            return Task.FromResult(0);
        }

        public virtual Task RemoteDeleteAllSmartViews()
        {
            return Task.FromResult(0);
        }

        public virtual Task RemoteDeleteSmartView(string name)
        {
            return Task.FromResult(0);
        }

        public virtual Task RemoteEditSmartView(string id, string newName, string newRules)
        {
            return Task.FromResult(0);
        }

        public virtual Task RemoteAddFolder(string name)
        {
            return Task.FromResult(0);
        }

        public virtual Task<List<string>> RemoteGetAllFolders()
        {
            return Task.FromResult(new List<string>());
        }

        public virtual Task RemoteDeleteFolder(string name)
        {
            return Task.FromResult(0);
        }

        public virtual Task RemoteEditFolder(string id, string newName)
        {
            return Task.FromResult(0);
        }

        public virtual Task RemoteAddContext(string name)
        {
            return Task.FromResult(0);
        }

        public virtual Task<List<string>> RemoteGetAllContexts()
        {
            return Task.FromResult(new List<string>());
        }

        public virtual Task RemoteDeleteContext(string name)
        {
            return Task.FromResult(0);
        }

        public virtual Task RemoteEditContext(string id, string newName)
        {
            return Task.FromResult(0);
        }

        public abstract Task RemoteAddTask(ITask task);
        public abstract Task RemoteEditTask(ITask task);
        public abstract Task RemoteDeleteTask(ITask task);
        public abstract Task RemoteDeleteTasks(IEnumerable<ITask> tasks);
    }

    public abstract class TestRunnerBase<TSyncProvider> : TestRunnerBase
    {
        protected TSyncProvider Provider
        {
            get { return (TSyncProvider)this.Manager.ActiveProvider; }
        }

        protected TestRunnerBase(SynchronizationService service, string testName, string providerDataFileName)
            : base(service, testName, providerDataFileName)
        {
        }
    }
}

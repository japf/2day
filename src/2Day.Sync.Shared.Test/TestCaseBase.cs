using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Sync.Test.Runners;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Tools
{
    [TestClass]
    public abstract class TestCaseBase
    {
        private static int taskId;

        private SynchronizationService service;
        private IWorkbook workbook;
        private ISynchronizationManager synchronizationManager;

        private TestRunnerBase testRunner;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public virtual void Initialize()
        {
            this.testRunner = this.CreateTestRunner(this.TestContext.TestName);

            this.workbook = this.testRunner.Workbook;
            Ioc.RegisterInstance<IWorkbook, IWorkbook>(this.workbook);
            this.synchronizationManager = this.testRunner.Manager;
            this.synchronizationManager.OperationFailed += (s, e) => Assert.Fail("Synchronization failed: " + e.Message);

            this.testRunner.BeforeTest().Wait();

            this.OnTestInitialize();
        }

        public TestRunnerBase CreateTestRunner(string testName)
        {
#if SYNC_TOODLEDO
                this.service = SynchronizationService.ToodleDo;
                return new ToodleDoTestRunner(testName);
#endif
#if SYNC_EXCHANGEWEBAPI
                this.service = SynchronizationService.Exchange;
                return new ExchangeTestRunner(this.TestContext, testName);
#endif
#if SYNC_EXCHANGEEWS
                this.service = SynchronizationService.ExchangeEws;
                return new ExchangeEwsTestRunner(this.TestContext, testName);
#endif
#if SYNC_ACTIVESYNC
                this.service = SynchronizationService.OutlookActiveSync;
                return new ActiveSyncTestRunner(testName);
#endif
#if SYNC_VERCORS
                this.service = SynchronizationService.Vercors;
                return new VercorsTestRunner(testName);
#endif
        }

        protected virtual void OnTestInitialize()
        {
            
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (this.testRunner != null)
                this.testRunner.AfterTest();
        }

        protected IWorkbook Workbook
        {
            get { return this.workbook; }
        }

        protected TestRunnerBase Runner
        {
            get { return this.testRunner; }
        }

        protected ISynchronizationManager Manager
        {
            get { return this.synchronizationManager; }
        }

        protected ISynchronizationProvider Provider
        {
            get { return this.synchronizationManager.ActiveProvider; }
        }

        protected ICryptoService CryptoService
        {
            get { return this.testRunner.CryptoService; }
        }

        protected SynchronizationService Service
        {
            get { return this.service; }
        }

        protected IFolder CreateFolder(string name)
        {
            return this.workbook.AddFolder(name);
        }

        protected void RemoveFolder(IFolder folder)
        {
            this.Workbook.RemoveFolder(folder.Name);
        }

        protected IContext CreateContext(string name)
        {
            return this.workbook.AddContext(name);
        }

        protected void RemoveContext(IContext context)
        {
            this.Workbook.RemoveContext(context.Name);
        }

        protected ISmartView CreateSmartView(string name)
        {
            return this.workbook.AddSmartView(name, "(priority is star)");
        }

        protected void RemoveSmartView(ISmartView SmartView)
        {
            this.Workbook.RemoveSmartView(SmartView.Name);
        }

        protected ITask CreateTask(string name, IFolder folder)
        {
            var task = this.workbook.CreateTask();

            task.Id = taskId++;
            task.Title = name;
            task.Added = DateTime.Now;
            task.Folder = folder;

            return task;
        }

        public async Task SyncFull()
        {
            await this.SyncAsync(true);
        }

        public async Task SyncDelta()
        {
            await this.SyncAsync(false);
        }

        private async Task SyncAsync(bool clearWorkbook)
        {
            // wait 1 second to make sure edit timestamp will be different as they are stored in Unix format with second precision
            // otherwise, there could be edge cases where we add & remove an item during the same Unix timestamp
            await Task.Delay(TimeSpan.FromSeconds(1));
            await this.Manager.Sync();

            if (clearWorkbook)
            {
                // todo: merge with other locations...
                this.workbook.RemoveAll();

                var currentService = this.Manager.ActiveService;

                this.Manager.Reset(clearSettings: false);

                this.Manager.ActiveService = currentService;

                await Task.Delay(TimeSpan.FromSeconds(1));
                await this.Manager.Sync();
            }
        }
    }

    public class TestCaseBase<TProvider> : TestCaseBase where TProvider : ISynchronizationProvider
    {
        protected new TProvider Provider
        {
            get { return (TProvider)this.Manager.ActiveProvider; }
        }
    }

    public class TestCaseBase<TProvider, TRunner> : TestCaseBase 
        where TProvider : ISynchronizationProvider
        where TRunner : TestRunnerBase
    {
        protected new TProvider Provider
        {
            get { return (TProvider)this.Manager.ActiveProvider; }
        }

        protected new TRunner Runner
        {
            get { return (TRunner)base.Runner; }
        }
    }
}
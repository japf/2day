using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Sync.Test.Tools;
using Chartreuse.Today.ToodleDo;
using Chartreuse.Today.ToodleDo.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Sync.Test.Runners
{
    public class ToodleDoTestRunner : TestRunnerBase<ToodleDoSynchronizationProvider>
    {
        private static int accountIndex = 0;

        private static readonly List<Tuple<string, string>> accounts = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("user@domain.com", "password")
        }; 

        private ToodleDoService service;

        public override string Password
        {
            get { return this.Workbook.Settings.GetValue<string>(ToodleDoSettings.ToodleDoPassword); }
            set { this.Workbook.Settings.SetValue(ToodleDoSettings.ToodleDoPassword, value); }
        }

        public override bool SupportContext
        {
            get { return true; }
        }

        public override bool SupportAlarm
        {
            get { return false; }
        }

        public override bool SupportFolder
        {
            get { return true; }
        }

        public override bool SupportTag
        {
            get { return true; }
        }

        public ToodleDoTestRunner(string testName) 
            : base(SynchronizationService.ToodleDo, testName, "toodledo-data" + accountIndex + ".json")
        {
            string email = accounts[accountIndex].Item1;
            LogService.Log("ToodleDoTestRunner", $"Using account {email}");

            this.Workbook.Settings.SetValue(ToodleDoSettings.ToodleDoLogin, accounts[accountIndex].Item1);
            this.Workbook.Settings.SetValue(ToodleDoSettings.ToodleDoPassword, this.CryptoService.Encrypt(accounts[accountIndex].Item2));

            accountIndex++;
            if (accountIndex > accounts.Count - 1)
                accountIndex = 0;

            ToodleDoSynchronizationProvider.FetchNonCompletedAtFirstSync = false;
        }
        
        public override void BeforeTestCore()
        {
            this.service = this.Provider.ToodleDoService;

            string login= this.Workbook.Settings.GetValue<string>(ToodleDoSettings.ToodleDoLogin);
            string password = this.CryptoService.Decrypt(this.Workbook.Settings.GetValue<byte[]>(ToodleDoSettings.ToodleDoPassword));

            this.service.OnWebException += (s, e) =>
            {
                Assert.Fail($"OnWebException Login: {login} Password: {password} Message: {e.Item.Message}");
            };

            this.service.OnApiError += (s, e) =>
            {
                //if (e.Item.HasError && e.Item.ErrorId != ToodleDoService.ErrorIdFolderAlreadyExists)
                //    Assert.Fail($"OnApiError Login: {login} Password: {password} Message: {e.Item.Error}");
            };
            
            Task.WaitAll(this.Manager.Sync());
        }

        public override async Task RemoteDeleteAllTasks()
        {
            var tasks = await this.RemoteGetAllTasks();
            await this.service.DeleteTasks(tasks.Select(t => t.Id));
        }

        public override async Task RemoteDeleteAllContexts()
        {
            var contexts = await this.RemoteGetAllContextsCore();
            foreach (var context in contexts)
                await this.service.DeleteContext(context.Id);
        }

        public override async Task RemoteDeleteAllFolders()
        {
            var folders = await this.RemoteGetAllFoldersCore();
            foreach (var folder in folders)
                await this.service.DeleteFolder(folder.Id);
        }

        public override async Task<List<string>> RemoteGetAllFolders()
        {
            var folders = await this.RemoteGetAllFoldersCore();

            return folders.Select(f => f.Name).ToList();
        }

        public override async Task RemoteAddFolder(string name)
        {
            await this.service.AddFolder(name);
        }

        public override async Task RemoteDeleteFolder(string name)
        {
            var folders = await this.RemoteGetAllFoldersCore();
            var folder = folders.First(f => f.Name == name);

            await this.service.DeleteFolder(folder.Id);
        }

        public override async Task<List<string>> RemoteGetAllContexts()
        {
            var contexts = await this.RemoteGetAllContextsCore();
            return contexts.Select(f => f.Name).ToList();
        }

        public override async Task RemoteEditFolder(string id, string newName)
        {
            await this.service.UpdateFolder(id, newName);
        }

        public override async Task RemoteAddContext(string name)
        {
            await this.service.AddContext(name);
        }

        public override async Task RemoteDeleteContext(string name)
        {
            var folders = await this.RemoteGetAllContextsCore();
            var folder = folders.First(f => f.Name == name);

            await this.service.DeleteContext(folder.Id);
        }

        public override async Task RemoteEditContext(string id, string newName)
        {
            await this.service.UpdateContext(id, newName);
        }

        public override async Task RemoteAddTask(ITask task)
        {
            await this.service.AddTask(new ToodleDoTask(task));
        }

        public override async Task RemoteEditTask(ITask task)
        {
            await this.service.UpdateTask(task.SyncId, new ToodleDoTask(task), TaskProperties.All);
        }

        public override async Task RemoteDeleteTask(ITask task)
        {
            await this.service.DeleteTask(task.SyncId);
        }

        public override async Task RemoteDeleteTasks(IEnumerable<ITask> tasks)
        {
            await this.service.DeleteTasks(tasks.Select(t => t.SyncId));
        }

        private async Task<List<ToodleDoTask>> RemoteGetAllTasks()
        {
            var r = await this.service.GetTasks(false);

            return r.ToList();
        }

        private async Task<List<ToodleDoContext>> RemoteGetAllContextsCore()
        {
            var r = await this.service.GetContexts();
            if (r.HasError)
                Assert.Fail("GetToodleDoContexts error: " + r.Error);

            return r.Data.ToList();
        }

        private async Task<List<ToodleDoFolder>> RemoteGetAllFoldersCore()
        {
            var r = await this.service.GetFolders();
            if (r.HasError)
                Assert.Fail("GetToodleDoFolders error: " + r.Error);

            return r.Data.ToList();
        }
    }
}

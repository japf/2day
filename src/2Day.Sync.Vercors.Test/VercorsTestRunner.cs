using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Sync.Test.Tools;
using Chartreuse.Today.Sync.Vercors.Test;
using Chartreuse.Today.Vercors.Shared;
using Chartreuse.Today.Vercors.Shared.Model;

namespace Chartreuse.Today.Sync.Test.Runners
{
    public class VercorsTestRunner : TestRunnerBase<VercorsSynchronizationProvider>
    {
        private static readonly string token;
        private IVercorsService service;

        public override string Password
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override bool SupportContext
        {
            get { return true; }
        }

        public override bool SupportAlarm
        {
            get { return true; }
        }

        public override bool SupportFolder
        {
            get { return true; }
        }

        public override bool SupportTag
        {
            get { return true; }
        }

        public static string Token
        {
            get { return token; }
        }

        public static string UserId
        {
            get { return "1-continuous-tester"; }
        }

        static VercorsTestRunner()
        {
            token = AmsHelper.GetSecurityToken(
                TimeSpan.FromMinutes(30), 
                "Custom", 
                UserId, 
                "masterKey2DayCloud"); // master key is not published in the open source version of 2Day
        }

        public VercorsTestRunner(string testName) 
            : base(SynchronizationService.Vercors, testName, "provider-vercors-data.json")
        {
        }

        public override void BeforeTestCore()
        {
            this.service = this.Provider.VercorsService;
            this.service.LoginAsync(true).Wait();
        }

        public override async Task RemoteDeleteAllTasks()
        {
            var tasks = await this.GetAllTasks();
            await this.service.DeleteTasks(tasks);   
        }

        public override async Task RemoteDeleteAllContexts()
        {
            var contexts = await this.service.GetContexts();
            foreach (var context in contexts)
            {
                await this.service.DeleteContext(context);
            }
        }

        public override async Task RemoteDeleteAllFolders()
        {
            var folders = await this.GetAllFolders();
            foreach (var f in folders)
                await this.service.DeleteFolder(f);
        }

        public override async Task RemoteDeleteAllSmartViews()
        {
            var smartviews = await this.GetAllSmartViews();
            foreach (var sv in smartviews)
                await this.service.DeleteSmartView(sv);
        }

        public override async Task<List<string>> RemoteGetAllFolders()
        {
            var folders = await this.service.GetFolders();
            return folders.Select(f => f.Name).ToList();
        }

        public override async Task RemoteAddFolder(string name)
        {
            await this.service.AddFolder(new VercorsFolder {Name = name});
        }

        public override async Task RemoteDeleteFolder(string name)
        {
            var folders = await this.service.GetFolders();
            var folder = folders.SingleOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (folder == null)
                throw new NotSupportedException("Remote folder not found");

            await this.service.DeleteFolder(folder);
        }

        public override async Task RemoteEditFolder(string id, string newName)
        {
            await this.service.UpdateFolder(new VercorsFolder {ItemId = int.Parse(id), Name = newName});
        }

        public override async Task RemoteEditSmartView(string id, string newName, string newRules)
        {
            await this.service.UpdateSmartView(new VercorsSmartView { ItemId = id, Name = newName, Rules = newRules });
        }

        public override async Task<List<string>> RemoteGetAllContexts()
        {
            var contexts = await this.service.GetContexts();
            return contexts.Select(f => f.Name).ToList();
        }

        public override async Task RemoteAddContext(string name)
        {
            await this.service.AddContext(new VercorsContext { Name = name });
        }

        public override async Task RemoteDeleteContext(string name)
        {
            var contexts = await this.service.GetContexts();
            var context = contexts.SingleOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (context == null)
                throw new NotSupportedException("Remote context not found");

            await this.service.DeleteContext(context);
        }

        public override async Task RemoteEditContext(string id, string newName)
        {
            await this.service.UpdateContext(new VercorsContext { ItemId = int.Parse(id), Name = newName });
        }

        public override async Task RemoteAddTask(ITask task)
        {
            await this.service.AddTask(new VercorsTask(this.Workbook, task));
        }

        public override async Task RemoteEditTask(ITask task)
        {
            await this.service.UpdateTask(new VercorsTask(this.Workbook, task));
        }

        public override async Task RemoteDeleteTask(ITask task)
        {
            await this.service.DeleteTasks(new[] { new VercorsTask(this.Workbook, task)  });                
        }

        public override async Task RemoteDeleteTasks(IEnumerable<ITask> tasks)
        {
            await this.service.DeleteTasks(tasks.Select(t => new VercorsTask(this.Workbook, t)));
        }

        public override async Task RemoteDeleteSmartView(string name)
        {
            var smartviews = await this.service.GetSmartViews();
            var smartview = smartviews.SingleOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (smartview == null)
                throw new NotSupportedException("Remote smartview not found");

            await this.service.DeleteSmartView(smartview);
        }

        private async Task<List<VercorsTask>> GetAllTasks()
        {
            var result = await this.service.GetTasks();

            return result.ToList();
        }

        private async Task<List<VercorsFolder>> GetAllFolders()
        {
            var result = await this.service.GetFolders();

            return result;
        }

        private async Task<List<VercorsSmartView>> GetAllSmartViews()
        {
            var result = await this.service.GetSmartViews();

            return result;
        }
    }
}

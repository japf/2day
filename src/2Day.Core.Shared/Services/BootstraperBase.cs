using System;
using System.Linq;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Shared.Services
{
    public abstract class BootstraperBase<TRootFrame>
    {
        protected readonly string version;

        protected BootstraperBase(string version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            this.version = version;
        }

        protected IWorkbook InitializeWorkbook(IPersistenceLayer persistence, IPlatformService platformService)
        {
            IWorkbook workbook = null;

            if (persistence.HasSave)
            {
                try
                {
                    workbook = persistence.Open();
                }
                catch (Exception ex)
                {
                    TrackingManagerHelper.Exception(ex, "Initialize workbook");
                    platformService.DeleteFileAsync(persistence.DatabaseFilename).Wait(1000);
                }
            }

            if (workbook == null)
            {
                persistence.Initialize();
                workbook = this.CreateDefaultWorkbook(persistence.Context, platformService);
                persistence.Save();
            }

            // important: read view from persistence and not from workbook as they are not loaded yet in the workbook !
            if (persistence.Context.Views.Count() != DefaultDataCreator.ViewCount)
                CreateDefaultViews(workbook);
            
            workbook.Initialize();
            Ioc.RegisterInstance<IWorkbook, Workbook>((Workbook)workbook);

            return workbook;
        }

        protected abstract IWorkbook CreateWorkbook(IDatabaseContext context);

        private IWorkbook CreateDefaultWorkbook(IDatabaseContext context, IPlatformService platformService)
        {
            var workbook = this.CreateWorkbook(context);

            DefaultDataCreator.SetupDefaultOptions(workbook);
            CreateDefaultFolders(workbook);
            CreateDefaultTasks(workbook, platformService);

            return workbook;
        }

        private static void CreateDefaultFolders(IWorkbook workbook)
        {
            DefaultDataCreator.CreateFolders(workbook);
        }

        private static void CreateDefaultViews(IWorkbook workbook)
        {
            DefaultDataCreator.CreateViews(workbook);
        }

        private static void CreateDefaultTasks(IWorkbook workbook, IPlatformService platformService)
        {
            DefaultDataCreator.CreateDefaultTasks(workbook, platformService.DeviceFamily);
        }

        public virtual void Close()
        {
        }

        public abstract Task<IWorkbook> ConfigureAsync(TRootFrame rootFrame);
    }
}

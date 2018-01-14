using Chartreuse.Today.Core.Shared.IO;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Universal.Model;

namespace Chartreuse.Today.Core.Universal.IO
{
    public class WinPersistenceLayer : IPersistenceLayer
    {
        private readonly WinDatabaseContext context;

        public IDatabaseContext Context
        {
            get { return this.context; }
        }

        public bool HasSave
        {
            get { return this.context.DatabaseExists(); }
        }

        public string DatabaseFilename 
        {
            get { return "2day.db"; }
        }

        public WinPersistenceLayer() : this(false)
        { 
        }

        public WinPersistenceLayer(bool automaticSave = true)
        {
            this.context = new WinDatabaseContext(this.DatabaseFilename, automaticSave);

            if (!Ioc.HasType<IDatabaseContext>())
                Ioc.RegisterInstance<IDatabaseContext, WinDatabaseContext>(this.context);
        }
        
        public void Save()
        {
            this.context.SendChanges();
        }

        public IWorkbook Open(bool tryUpgrade = false)
        {
            if (!this.context.DatabaseExists())
            {
                return null;
            }
            else
            {
                this.context.InitializeDatabase();
                return new Workbook(this.context, WinSettings.Instance);
            }
        }

        public void Initialize()
        {
            this.context.InitializeDatabase();
        }

        public void CloseDatabase()
        {
            this.context.CloseConnection();
        }

        public void OpenDatabase()
        {
            this.context.OpenConnection();
        }        
    }
}

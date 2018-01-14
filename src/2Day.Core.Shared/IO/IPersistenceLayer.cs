using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Shared.IO
{
    public interface IPersistenceLayer
    {
        bool HasSave { get; }
        IDatabaseContext Context { get; }

        void Save();
        IWorkbook Open(bool tryUpgrade = false);

        void Initialize();

        string DatabaseFilename { get; }
        void CloseDatabase();
        void OpenDatabase();
    }
}

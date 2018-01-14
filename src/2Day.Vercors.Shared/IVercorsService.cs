using System.Collections.Generic;
using System.Threading.Tasks;
using Chartreuse.Today.Vercors.Shared.Model;

namespace Chartreuse.Today.Vercors.Shared
{
    public interface IVercorsService
    {
        string LoginInfo { get; }

        Task<string> DeleteAccount();

        Task<VercorsTask> AddTask(VercorsTask vercorsTask);
        Task<IEnumerable<VercorsTask>> AddTasks(IEnumerable<VercorsTask> vercorsTasks);
        Task<IEnumerable<VercorsTask>> DeleteTasks(IEnumerable<VercorsTask> vercorsTasks);
        Task<VercorsTask> UpdateTask(VercorsTask vercorsTask);
        Task<IEnumerable<VercorsTask>> UpdateTasks(IEnumerable<VercorsTask> vercorsTasks);
        Task<List<VercorsTask>> GetTasks(long? afterTimestamp = null);
        Task<List<VercorsDeletedTask>> GetDeletedTasks(long afterTimestamp);

        Task<VercorsFolder> AddFolder(VercorsFolder vercorsFolder);
        Task<bool> DeleteFolder(VercorsFolder vercorsFolder);
        Task<VercorsFolder> UpdateFolder(VercorsFolder vercorsFolder);
        Task<List<VercorsFolder>> GetFolders();

        Task<VercorsContext> AddContext(VercorsContext vercorsContext);
        Task<bool> DeleteContext(VercorsContext vercorsContext);
        Task<VercorsContext> UpdateContext(VercorsContext vercorsContext);
        Task<List<VercorsContext>> GetContexts();

        Task<VercorsSmartView> AddSmartView(VercorsSmartView vercorsSmartView);
        Task<bool> DeleteSmartView(VercorsSmartView vercorsSmartView);
        Task<VercorsSmartView> UpdateSmartView(VercorsSmartView vercorsSmartView);
        Task<List<VercorsSmartView>> GetSmartViews();

        Task<VercorsAccount> GetUserAccount();
        Task<VercorsAccount> AddUserAccount(VercorsAccount vercorsAccount);

        Task<bool> LoginAsync(bool silent);
        bool Logout();
        
        void Cancel();
    }
}

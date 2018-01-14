using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Sync
{
    public interface IAdvancedSyncronizationManager
    {
        Task<bool> AdvancedSync();
    }
}
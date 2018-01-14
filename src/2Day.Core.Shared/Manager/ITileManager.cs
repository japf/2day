using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Shared.Manager
{
    public interface ITileManager
    {
        bool IsQuickAddTileEnabled { get; }

        Task<bool> PinAsync(ITask task);
        Task<bool> PinAsync(IAbstractFolder abstractFolder);
        Task<bool> PinQuickAdd();

        bool IsPinned(ITask task);
        bool IsPinned(IAbstractFolder folder);

        Task<bool> UnpinAsync(ITask task);
        Task<bool> UnpinAsync(IAbstractFolder folder);
        Task<bool> UnpinQuickAdd();

        void UpdateTiles();
    }
}
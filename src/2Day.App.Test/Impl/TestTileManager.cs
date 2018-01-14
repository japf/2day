using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Universal.Manager;

namespace Chartreuse.Today.App.Test.Impl
{
    public class TestTileManager : ITileManager
    {
        public bool IsQuickAddTileEnabled { get; }
        public string QuickAddTileId { get; }
        public Task<bool> PinAsync(ITask task)
        {
            return null;
        }

        public Task<bool> PinAsync(IAbstractFolder abstractFolder)
        {
            return null;
        }

        public Task<bool> PinQuickAdd()
        {
            return null;
        }

        public bool IsPinned(ITask task)
        {
            return false;
        }

        public bool IsPinned(IAbstractFolder folder)
        {
            return false;
        }

        public Task<bool> UnpinAsync(ITask task)
        {
            return null;
        }

        public Task<bool> UnpinAsync(IAbstractFolder folder)
        {
            return null;
        }

        public Task<bool> UnpinQuickAdd()
        {
            return null;
        }

        public IAbstractFolder GetAbstractFolder(string tileId)
        {
            return null;
        }

        public ITask GetTask(string tileId)
        {
            return null;
        }

        public void UpdateTiles()
        {
        }
    }
}
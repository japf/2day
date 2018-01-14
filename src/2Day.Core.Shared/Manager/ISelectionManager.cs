using System.Collections.Generic;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Shared.Manager
{
    public interface ISelectionManager
    {
        IList<ITask> SelectedTasks { get; }

        void SelectAll();

        void ClearSelection();

        Task DeleteSelectionAsync();

        void ToogleTaskSelection(IList<ITask> tasks);
    }
}

using System.Diagnostics;
using System.Windows.Input;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    [DebuggerDisplay("Name: {Name} Usage: {UsageCount}")]
    public class ItemCountViewModel
    {
        public ItemCountViewModel(string name, int usageCount, ICommand actionCommand = null)
        {
            this.Name = name;
            this.UsageCount = usageCount;

            if (actionCommand != null)
                this.ActionCommand = actionCommand;
        }

        public string Name { get; }

        public int UsageCount { get; }

        public ICommand ActionCommand { get; }
    }
}
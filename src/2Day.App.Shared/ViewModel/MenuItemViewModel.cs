using System.Windows.Input;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Collection;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public abstract class MenuItemViewModel : ViewModelBase
    {
        public abstract string Name { get; }

        public virtual bool IsSelected { get; set; }

        public virtual SmartCollection<ITask> SmartCollection
        {
            get { return null; }
        }

        public virtual object CollectionView
        {
            get { return null; }
        }

        public virtual IAbstractFolder Folder
        {
            get { return null; }
        }

        public virtual bool HasContextualCommand
        {
            get { return false; }
        }

        public virtual ICommand ContextualCommand
        {
            get { return null; }
        }
    }
}
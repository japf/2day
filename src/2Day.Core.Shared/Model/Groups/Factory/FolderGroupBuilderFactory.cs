namespace Chartreuse.Today.Core.Shared.Model.Groups.Factory
{
    public class FolderGroupBuilderFactory : GroupBuilderFactoryBase
    {

        public FolderGroupBuilderFactory(IAbstractFolder folder, ISettings settings)
            : base(folder, settings, true)
        {
        }

        protected override string GroupByCore(ITask task)
        {
            return task.Folder.Name;
        }

        protected override int CompareCore(ITask t1, ITask t2)
        {
            if (t1.Folder == t2.Folder || (t1.Folder.Order == t2.Folder.Order))
                return 0;
            else if (t1.Folder.Order < t2.Folder.Order)
                return -1;
            else
                return 1;
        }

        protected override object OrderByCore(ITask task)
        {
            return task.Folder.Name;
        }
    }
}
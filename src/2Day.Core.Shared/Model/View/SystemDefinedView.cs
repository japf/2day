
namespace Chartreuse.Today.Core.Shared.Model.View
{
    public abstract class SystemDefinedView : ViewBase, ISystemView
    {
        private readonly ISystemView view;

        public ViewKind ViewKind
        {
            get { return this.view.ViewKind; }
        }

        public bool IsEnabled
        {
            get
            {
                return this.view.IsEnabled;
            }
            set
            {
                if (this.view.IsEnabled != value)
                {
                    this.view.IsEnabled = value;
                    this.RaisePropertyChanged("IsEnabled");
                }
            }
        }

        protected SystemDefinedView(IWorkbook workbook, ISystemView view)
            : base(workbook, view)
        {
            this.view = view;

            this.Ready();
        }

        protected override bool ShouldTrackFolder(IFolder folder)
        {
            // special folder "completed" takes all folder whatever the settings they have
            // for other folders, the "ShowInViews" option must be set to true

            // 05.14.15 add a 'folder is null' because it causes a crash in the SL version of 2Day when deleting
            // a task from a folder that has the 'show in all views' option set to off
            return folder != null && (this.view.ViewKind == ViewKind.Completed || (folder.ShowInViews.HasValue && folder.ShowInViews.Value));
        }
    }
}
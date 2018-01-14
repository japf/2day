using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Model.View;
using Chartreuse.Today.Core.Shared.Tests.Impl;

namespace Chartreuse.Today.Core.Shared.Tests.Model.View
{
    public abstract class ViewTestBase
    {
        private readonly IWorkbook workbook;
        private readonly IView view;
        private readonly IFolder folder;

        protected ViewTestBase(ViewKind viewKind)
        {
            this.workbook = new Workbook(new TestDatabaseContext(), new TestSettings());
            this.folder = this.workbook.AddFolder("f1");
            this.view = ViewFactory.BuildView(this.Workbook, new SystemView { ViewKind = viewKind });
        }

        public IView View
        {
            get { return this.view; }
        }

        public IWorkbook Workbook
        {
            get { return this.workbook; }
        }

        public IFolder Folder
        {
            get { return this.folder; }
        }
    }
}
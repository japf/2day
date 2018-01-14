using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Chartreuse.Today.App.ViewModel;
using Chartreuse.Today.Core.Shared.Manager;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.App.Manager.UI
{
    public class GridViewSelectionManager : ISelectionManager
    {
        private readonly IWorkbook workbook;
        private readonly MainPageViewModel viewmodel;

        private readonly GridView gridViewTasks;

        public IList<ITask> SelectedTasks
        {
            get { return this.gridViewTasks.SelectedItems.OfType<ITask>().ToList(); }
        }

        public GridViewSelectionManager(IWorkbook workbook, MainPageViewModel viewmodel, GridView gridViewTasks)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (gridViewTasks == null)
                throw new ArgumentNullException(nameof(gridViewTasks));

            this.workbook = workbook;
            this.viewmodel = viewmodel;
            this.gridViewTasks = gridViewTasks;
        }

        public void SelectAll()
        {
            if (this.viewmodel.SelectedFolderItem != null)
                this.ToogleTaskSelection(this.viewmodel.SelectedFolderItem.Folder.Tasks.ToList());
        }

        public void ClearSelection()
        {
            this.gridViewTasks.SelectedItems.Clear();
        }

        public async Task DeleteSelectionAsync()
        {
            using (this.workbook.WithTransaction())
            {
                var selectedTasks = this.SelectedTasks;
                if (selectedTasks.Count > 0)
                {
                    ModelHelper.SoftDelete(selectedTasks);                    
                }
            }
        }

        public void ToogleTaskSelection(IList<ITask> tasks)
        {
            this.gridViewTasks.SelectedItems.Clear();
            if (!tasks.ContainsSameContentAs(this.SelectedTasks))
            {
                foreach (var selectedTask in tasks)
                    this.gridViewTasks.SelectedItems.Add(selectedTask);
            }
        }
    }
}

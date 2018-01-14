using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Views;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Impl;
using Chartreuse.Today.Core.Shared.Tools.Navigation;

namespace Chartreuse.Today.App.Manager.Shortcut
{
    public class CopyPasteShortcut : KeyboardShortcutBase
    {
        private readonly IWorkbook workbook;
        private List<int> selectedTasksIds;

        public CopyPasteShortcut(Frame rootFrame, INavigationService navigationService, IWorkbook workbook)
            : base(rootFrame, navigationService)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            this.workbook = workbook;
        }

        public override bool CanExecute(KeyRoutedEventArgs e)
        {
            return this.IsModifierKeyDown(VirtualKey.Control) && (e.Key == VirtualKey.C || e.Key == VirtualKey.V) && !this.IsFromTextControl(e) && this.GetCurrentPageAs<MainPage>() != null;
        }

        public override bool Execute(KeyRoutedEventArgs e)
        {
            var page = this.GetCurrentPageAs<MainPage>();
            if (page != null)
            {
                if (e.Key == VirtualKey.C)
                {
                    // copy
                    this.selectedTasksIds = page.SelectionManager.SelectedTasks.Select(t => t.Id).ToList();
                    return true;
                }
                else if (e.Key == VirtualKey.V && this.selectedTasksIds != null && this.selectedTasksIds.Count > 0)
                {
                    foreach (int id in this.selectedTasksIds)
                    {
                        var existingTask = this.workbook.Tasks.FirstOrDefault(t => t.Id == id);
                        if (existingTask != null)
                        {
                            existingTask.CopyToNewTask();                            
                        }
                    }

                    return false;
                }
            }

            return false;
        }
    }
}
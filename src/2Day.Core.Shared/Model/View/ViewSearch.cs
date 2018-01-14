using System;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    public class ViewSearch : ViewBase
    {
        private string searchText;

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                if (this.searchText != value)
                {
                    this.searchText = value;
                    this.Name = string.Format(StringResources.MainPage_SearchDescriptionFormat, this.searchText);
                }
            }
        }

        public ViewSearch(IWorkbook workbook) : base(workbook, new SystemViewOrfan())
        {
            this.Ready();
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            var search = GetTaskSearchPredicate();

            return (task) => search(task, this.SearchText);
        }

        public static Func<ITask, string, bool> GetTaskSearchPredicate()
        {
            Func<ITask, string, bool> predicate = (task, text) =>
            {
                // check for task.Folder == null is needed because when a task is being deleted we will 
                // hit this code with a null folder
                if (task == null || task.Folder == null)
                    return false;

                string search = text != null ? text.ToLower() : null;
                string searchTrim = null;
                if (search != null)
                {
                    // remove trailing '.' which is added with speech recognition
                    searchTrim = search.Trim().TrimEnd('.');
                }

                string title = string.IsNullOrEmpty(task.Title) ? string.Empty : task.Title.ToLower();
                string note = string.IsNullOrEmpty(task.Note) ? string.Empty : task.Note.ToLower();
                string context = task.Context == null ? string.Empty : task.Context.Name.ToLower();
                string tags = string.IsNullOrEmpty(task.Tags) ? string.Empty : task.Tags.ToLower();

                

                return !string.IsNullOrEmpty(search)
                       && (
                           title.Contains(search) ||
                           note.Contains(search) ||
                           context.Contains(search) ||
                           tags.Contains(search) ||

                           title.RemoveDiacritics().Contains(search) ||
                           note.RemoveDiacritics().Contains(search) ||
                           context.RemoveDiacritics().Contains(search) ||
                           tags.RemoveDiacritics().Contains(search) ||

                           title.Contains(searchTrim) ||
                           note.Contains(searchTrim) ||
                           context.Contains(searchTrim) ||
                           tags.Contains(searchTrim)
                          );
            };

            return (task, text) => predicate(task, text);
        }        
    }
}

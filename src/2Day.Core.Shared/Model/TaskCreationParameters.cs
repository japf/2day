using System;

namespace Chartreuse.Today.Core.Shared.Model
{
    public class TaskCreationParameters
    {
        public static readonly TaskCreationParameters None = new TaskCreationParameters();

        public string Title { get; set; }

        public DateTime? Due { get; set; }
        public TaskPriority? Priority { get; set; }
        public string Tag { get; set; }
        
        public IFolder Folder { get; set; }
        public IContext Context { get; set; }

        public bool QuickAdd { get; set; }
    }
}

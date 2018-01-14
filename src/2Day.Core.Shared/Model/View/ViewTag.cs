using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Chartreuse.Today.Core.Shared.Icons;

namespace Chartreuse.Today.Core.Shared.Model.View
{
    [DebuggerDisplay("{Name}")]
    public class ViewTag : ViewBase, ITag
    {
        private readonly IViewTag viewOwner;

        public new int IconId
        {
            get { return FontIconHelper.IconIdTag; }
        }

        public new string Name
        {
            get { return base.Name; }
            set { }
        }

        public override bool CanReceiveTasks { get { return true; } }

        public ViewTag(IWorkbook workbook, IViewTag view) : base(workbook, view, view.Name)
        {
            this.viewOwner = view;

            this.Ready();
        }

        public override void MoveTasks(IEnumerable<ITask> tasks)
        {
            foreach (var task in tasks)
            {
                var tags = task.ReadTags();
                if (tags.All(t => !t.Equals(this.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    tags.Add(this.Name);
                    task.WriteTags(tags);
                }
            }
        }

        protected override Predicate<ITask> BuildTaskPredicateCore()
        {
            return (task) => !string.IsNullOrEmpty(task.Tags) && task.Tags.Contains(this.Name);
        }

        public IView Owner
        {
            get { return this.viewOwner; }
        }
    }
}

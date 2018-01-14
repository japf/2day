using System;
using System.Linq;
using System.Xml.Linq;

using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.ToodleDo
{
    public class ToodleDoTask : ISyncItem, ISyncTask
    {
        public string Name
        {
            get { return this.Title; }
        }

        public string Id { get;set; }                
        public string Title { get;set; }
        public string ParentId { get;set; }
        public string FolderId { get; set; }
        public string ContextId { get; set; }        
        public DateTime Added {get;set;}        
        public DateTime Modified  {get;set;}
        public DateTime? Due { get; set; }
        public DateTime? Start { get; set; }      
        public DateTime? Completed {get;set;}

        public string Repeat {get;set;}
        public int RepeatFrom { get; set; }   
        public string Note { get; set; }
        public int Priority {get;set;}
        public string Tags { get; set; }
        
        public TaskPriority EnumPriority
        {
            get
            {
                switch (this.Priority)
                {
                    case -1:
                        return TaskPriority.None;
                    case 0:
                        return TaskPriority.Low;
                    case 1:
                        return TaskPriority.Medium;
                    case 2:
                        return TaskPriority.High;
                    case 3:
                        return TaskPriority.Star;
                    default:
                        return TaskPriority.None;
                }
            }
        }
        
        public override string ToString()
        {
            return Title;
        }

        public ToodleDoTask()
        {
        }

        public ToodleDoTask(ITask task)
        {
            this.Id = task.SyncId;
            this.Title = task.Title;
            this.Note = task.Note;
            this.FolderId = task.Folder.SyncId;
            if (task.Context != null)
                this.ContextId = task.Context.SyncId;
            this.Added = task.Added;
            this.Tags = task.Tags;

            this.Modified = task.Modified;

            if (task.Due.HasValue)
                this.Due = task.Due.Value;

            if (task.Start.HasValue)
                this.Start = task.Start.Value;

            if (task.Completed.HasValue)
                this.Completed = task.Completed;

            this.Repeat = ToodleDoRecurrencyHelpers.GetToodleDoRecurrency(task.CustomFrequency);
            this.RepeatFrom = task.UseFixedDate ? 0 : 1;

            this.Priority = (int)task.Priority - 1;

            if (task.ParentId != null)
            {
                var parent = task.Folder.Tasks.FirstOrDefault(t => t.Id == task.ParentId);
                if (parent != null && parent.SyncId != null)
                {
                    this.ParentId = parent.SyncId;
                }
            }
        }

        public ToodleDoTask(XElement x)
        {
            this.Id = x.Element("id").Value;
            this.Title = (string) x.Element("title");
            this.FolderId = x.Element("folder").Value;
            this.ContextId = x.Element("context").Value;
            this.Priority = (int) x.Element("priority");
            this.Note = (string) x.Element("note");
            this.Tags = (string) x.Element("tag");

            this.Added = x.GetElement("added").TryParseTimestamp();
            this.Modified = x.GetElement("modified").TryParseTimestamp();
            this.Completed = x.GetElement("completed").TryParseNullableTimestamp();
            if (this.Completed.HasValue)
                this.Completed = this.Completed.Value;

            this.Repeat = (string)x.Element("repeat");
            this.RepeatFrom = (int)x.Element("repeatfrom");

            this.ParentId = x.GetElement("parent");
            if (this.ParentId == string.Empty || this.ParentId == "0")
                this.ParentId = null;

            var duedate = x.GetElement("duedate").TryParseNullableTimestamp(convertLocal: false);
            var duetime = x.GetElement("duetime").TryParseNullableTimestamp(convertLocal: false);
            if (duedate.HasValue)
            {
                // remove 12 hours because the date component returned by ToodleDo is always set to noon
                this.Due = duedate.Value.AddHours(-12);
                if (duetime.HasValue)
                    this.Due = this.Due.Value.AddSeconds(duetime.Value.TimeOfDay.TotalSeconds);
            }

            var startdate = x.GetElement("startdate").TryParseNullableTimestamp(convertLocal: false);
            var starttime = x.GetElement("starttime").TryParseNullableTimestamp(convertLocal: false);
            if (startdate.HasValue)
            {
                // remove 12 hours because the date component returned by ToodleDo is always set to noon
                this.Start = startdate.Value.AddHours(-12);
                if (starttime.HasValue)
                    this.Start = this.Start.Value.AddSeconds(starttime.Value.TimeOfDay.TotalSeconds);
            }
        }

        public void UpdateTask(ITask target, IWorkbook workbook, Func<IFolder> createDefaultFolder)
        {
            target.Title = this.Title;
            target.Note = this.Note;
            target.Due = this.Due;
            target.Start = this.Start;
            target.Completed = this.Completed;
            target.Priority = this.EnumPriority;
            target.Tags = this.Tags;
            target.Modified = DateTime.Now;

            target.UseFixedDate = this.RepeatFrom == 0;
            target.CustomFrequency = ToodleDoRecurrencyHelpers.Get2DayRecurrency(this.Repeat);

            IFolder folder = workbook.Folders.FirstOrDefault(f => f.SyncId == this.FolderId);
            if (folder == null)
                folder = createDefaultFolder();

            IContext context = workbook.Contexts.FirstOrDefault(c => c.SyncId == this.ContextId);
            target.Context = context;

            if (this.ParentId != null)
            {
                var parent = workbook.Tasks.FirstOrDefault(t => t.SyncId == this.ParentId);
                if (parent != null && !parent.Children.Contains(target))
                {
                    parent.AddChild(target);
                }
            }

            target.Folder = folder;            
        }
    }
}
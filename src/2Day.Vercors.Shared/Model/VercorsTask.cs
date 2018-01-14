using System;
using System.Diagnostics;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Sync;
using Chartreuse.Today.Core.Shared.Tools;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace Chartreuse.Today.Vercors.Shared.Model
{
    [DataTable("task")]
    [DebuggerDisplay("Title: {Title} FolderId: {FolderId} Priority: {Priority} Due: {Due}")]
    public class VercorsTask : ISyncTask
    {
        public string Name
        {
            get { return this.Title; }
        }

        [JsonIgnore]
        public string Id
        {
            get { return this.ItemId.ToString(); }
        }

        [JsonProperty(PropertyName = "id")]
        public int ItemId { get; set; }

        [JsonProperty(PropertyName = "editTimestamp")]
        public long EditTimestamp { get; set; }

        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "note")]
        public string Note { get; set; }

        [JsonProperty(PropertyName = "modified")]
        public DateTime Modified { get; set; }

        [JsonProperty(PropertyName = "priority")]
        public TaskPriority Priority { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public string Tags { get; set; }

        [JsonProperty(PropertyName = "due")]
        public DateTime? Due { get; set; }

        [JsonProperty(PropertyName = "start")]
        public DateTime? Start { get; set; }

        [JsonProperty(PropertyName = "completed")]
        public DateTime? Completed { get; set; }

        [JsonProperty(PropertyName = "frequencyType")]
        public FrequencyType FrequencyType { get; set; }

        [JsonProperty(PropertyName = "frequencyValue")]
        public string FrequencyValue { get; set; }

        [JsonProperty(PropertyName = "useFixedDate")]
        public bool UseFixedDate { get; set; }

        [JsonProperty(PropertyName = "alarm")]
        public DateTime? Alarm { get; set; }

        [JsonProperty(PropertyName = "progress")]
        public double? Progress { get; set; }

        [JsonProperty(PropertyName = "folderId")]
        public int FolderId { get; set; }

        [JsonProperty(PropertyName = "contextId")]
        public int ContextId { get; set; }

        [JsonProperty(PropertyName = "parentId")]
        public int? ParentId { get; set; }

        public VercorsTask()
        {
        }

        public VercorsTask(bool fillSampleValues = false)
        {
            if (fillSampleValues)
            {
                this.Identifier = " ";
                this.Title = " ";
                this.Note = " ";
                this.Tags = " ";
                this.Due = DateTime.Now;
                this.Start = DateTime.Now;
                this.Modified = DateTime.Now;
                this.Completed = DateTime.Now;
                this.Alarm = DateTime.Now;
                this.FrequencyValue = " ";
                this.Progress = 0.0;
                this.UseFixedDate = false;
            }
        }

        public VercorsTask(IWorkbook workbook, ITask task)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            int syncId = 0;
            int.TryParse(task.SyncId, out syncId);

            this.ItemId = syncId;
            this.Title = task.Title;
            this.Note = task.Note;
            this.Priority = task.Priority;
            this.Tags = task.Tags;
            this.Modified = task.Modified;
            this.Due = task.Due;
            this.Start = task.Start;
            this.Completed = task.Completed;
            this.FrequencyType = task.FrequencyType.HasValue ? task.FrequencyType.Value : FrequencyType.Once;
            this.FrequencyValue = task.FrequencyValue;
            this.UseFixedDate = task.UseFixedDate;
            this.Alarm = task.Alarm;
            this.Progress = task.Progress;

            if (task.ParentId.HasValue)
            {
                // find the parent task
                var parent = workbook.Tasks.FirstOrDefault(t => t.Id == task.ParentId.Value);
                if (parent != null)
                {
                    if (!string.IsNullOrWhiteSpace(parent.SyncId))
                        this.ParentId = int.Parse(parent.SyncId);
                    else
                        LogService.Log("VercorsTask", $"Task {task.Title}/{task.Id} has a parent with {parent.Title}/{task.ParentId.Value} but this parent doesn't have a sync id.");
                }
            }

            if (task.Folder != null && !string.IsNullOrWhiteSpace(task.Folder.SyncId))
                this.FolderId = int.Parse(task.Folder.SyncId);

            if (task.Context != null && !string.IsNullOrWhiteSpace(task.Context.SyncId))
                this.ContextId = int.Parse(task.Context.SyncId);
        }

        public void UpdateTarget(ITask task, IWorkbook workbook, Func<IFolder> getDefaultFolder)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (getDefaultFolder == null)
                throw new ArgumentNullException(nameof(getDefaultFolder));

            task.Title = this.Title;
            task.SyncId = this.Id;
            task.Note = this.Note;
            task.Priority = this.Priority;
            task.Tags = this.Tags;
            task.Modified = this.Modified;
            task.Due = this.Due;
            task.Start = this.Start;
            task.Completed = this.Completed;
            task.FrequencyType = this.FrequencyType;
            task.FrequencyValue = this.FrequencyValue;
            task.Alarm = this.Alarm;
            task.Progress = this.Progress;
            task.UseFixedDate = this.UseFixedDate;

            if (this.ParentId.HasValue)
            {
                // find parent
                var parent = workbook.Tasks.FirstOrDefault(t => t.SyncId == this.ParentId.Value.ToString());
                if (parent != null)
                    parent.AddChild(task);
                else
                    LogService.Log(LogLevel.Warning, "VercorsTask", $"Unable to find parent task with id {this.ParentId.Value}");
            }

            var folder = workbook.Folders.FirstOrDefault(f => f.SyncId == this.FolderId.ToString());
            if (folder == null)
                folder = getDefaultFolder();
            task.Folder = folder;

            task.Context = workbook.Contexts.FirstOrDefault(f => f.SyncId == this.ContextId.ToString());
        }
    }
}
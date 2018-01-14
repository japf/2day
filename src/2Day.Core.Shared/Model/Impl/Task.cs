using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model.Recurrence;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using SQLite.Net.Attributes;

namespace Chartreuse.Today.Core.Shared.Model.Impl
{
    [DebuggerDisplay("Name: {Title} Folder: {Folder.Name} Priority: {Priority} Due: {Due}")]
    [Table("Tasks")]
    public class Task : ModelEntityBase, ITask
    {
        #region fields

        private string title;
        private string note;
        private string tags;
        private string displayNote;
        private int? parentId;

        private DateTime added;
        private DateTime modified;
        private DateTime? due;
        private DateTime? start;
        private DateTime? completed;

        private ICustomFrequency customFrequency;
        private bool useFixedDate;
        private string syncId;

        private TaskPriority priority;

        private TaskAction action;
        private string actionName;
        private string actionValue;

        private DateTime? alarm;

        private double? progress;

        private string customFrequencyValue = null;

        private int? contextId;
        private IContext context;

        private int? folderId;
        private IFolder folder;

        private string childrenDescriptor;

        #endregion

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int? ParentId
        {
            get
            {
                return this.parentId;
            }
            set
            {
                if (this.parentId != value)
                {
                    this.parentId = value;
                    this.RaisePropertyChanged("ParentId");
                }
            }
        }

        [Ignore]
        public ObservableCollection<ITask> Children { get; set; }

        public string ChildrenDescriptor
        {
            get
            {
                if (this.Children.Count == 0)
                {
                    return null;
                }
                else if (this.childrenDescriptor == null && this.Children.Count > 0)
                {
                    int total = this.Children.Count, totalCompleted = 0;
                    for (int i = 0; i < total; i++)
                    {
                        if (this.Children[i].IsCompleted)
                            totalCompleted++;
                    }

                    this.childrenDescriptor = $"{totalCompleted} / {total}";
                }

                return this.childrenDescriptor;
            }
        }

        public string SyncId
        {
            get
            {
                return this.syncId;
            }
            set
            {
                if (this.syncId != value)
                {                    
                    this.syncId = value;
                    this.RaisePropertyChanged("SyncId");                    
                }
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.RaisePropertyChanged("Title");
                }
            }
        }
        
        public TaskPriority Priority
        {
            get
            {
                return this.priority;
            }
            set
            {
                if (this.priority != value)
                {
                    this.priority = value;
                    this.RaisePropertyChanged("Priority");
                }
            }
        }

        public string Note
        {
            get
            {
                return this.note;
            }
            set
            {
                if (this.note != value)
                {                    
                    this.note = value;

                    if (!this.note.HasHtml())
                        this.displayNote = this.note;
                    else
                        this.displayNote = string.Empty;

                    this.RaisePropertyChanged("Note", "DisplayNote", "HasDisplayNote");
                }
            }
        }

        public string DisplayNote
        {
            get { return this.displayNote; }
        }

        public bool HasDisplayNote
        {
            get { return !string.IsNullOrEmpty(this.displayNote); }
        }

        public string Tags
        {
            get
            {
                return this.tags;
            }
            set
            {
                if (this.tags != value)
                {
                    this.tags = value;
                    this.RaisePropertyChanged("Tags", "HasTags");

                }
            }
        }

        public bool HasTags
        {
            get { return !string.IsNullOrEmpty(this.tags); }
        }

        public DateTime Added
        {
            get
            {
                return this.added;
            }
            set
            {
                if (this.added != value)
                {
                    this.added = value;
                    this.RaisePropertyChanged("Added");
                }
            }
        }

        public DateTime Modified
        {
            get
            {
                return this.modified;
            }
            set
            {
                if (this.modified != value)
                {
                    this.modified = value;
                    this.RaisePropertyChanged("Modified");
                }
            }
        }

        public DateTime? Due
        {
            get
            {
                return this.due;
            }
            set
            {
                if (this.due != value)
                {
                    this.due = value;
                    this.RaisePropertyChanged("Due", "IsLate");
                }
            }
        }

        public DateTime? Start
        {
            get
            {
                return this.start;
            }
            set
            {
                if (this.start != value)
                {
                    this.start = value;
                    this.RaisePropertyChanged("Start");
                }
            }
        }

        public DateTime? Completed
        {
            get
            {
                return this.completed;
            }
            set
            {
                if (this.completed != value)
                {
                    this.completed = value;
                    this.RaisePropertyChanged("Completed", "IsCompleted");
                }
            }
        }

        [Ignore]
        public bool IsCompleted
        {
            get
            {
                return this.completed.HasValue;
            }
            set
            {
                // update the Modified datetime before updating the other properties because
                // other change might trigger a sort based on the modification date
                this.Modified = DateTime.Now;

                if (value && !this.IsCompleted)
                {
                    // change from "not completed" to "completed"
                    this.completed = DateTime.Now;
                    this.RaisePropertyChanged("Completed", "IsCompleted", "IsLate");

                    if (this.IsPeriodic)
                    {
                        RecurringTaskHelper.CreateNewTask(this);

                        // make the current task no more periodic
                        this.CustomFrequency = null;
                        this.RaisePropertyChanged("FrequencyRate", "FrequencyScale", "IsPeriodic");
                    }
                    else
                    {
                        if (this.alarm.HasValue)
                            this.Alarm = null;
                    }

                }
                else if (!value && this.IsCompleted)
                {
                    // change from "completed" to "not completed"
                    this.completed = null;
                    this.RaisePropertyChanged("Completed", "IsCompleted", "IsLate");
                }
            }
        }

        [Ignore]
        public bool HasRecurringOrigin { get; set; }

        public bool IsLate
        {
            get { return !this.IsCompleted && this.Due.HasValue && this.Due.Value.Date < DateTime.Now.Date; }
        }

        public bool IsPeriodic
        {
            get { return this.customFrequency != null && this.customFrequency.FrequencyType != Recurrence.FrequencyType.Once; }
        }

        public bool UseFixedDate
        {
            get
            {
                return this.useFixedDate;
            }
            set
            {
                if (this.useFixedDate != value)
                {
                    this.useFixedDate = value;
                    this.RaisePropertyChanged("UseFixedDate");
                }
            }
        }

        [Ignore]
        public FrequencyType? FrequencyType
        {
            get { return (this.CustomFrequency != null ? this.CustomFrequency.FrequencyType : Recurrence.FrequencyType.Once); }
            set
            {
                var notNullValue = (value ?? Recurrence.FrequencyType.Once);
                //Setter must be used only by the database manager
                if (this.CustomFrequency == null || this.CustomFrequency.FrequencyType != notNullValue)
                {
                    this.CustomFrequency = FrequencyFactory.GetCustomFrequency(notNullValue);
                }
                if (this.customFrequencyValue != null)
                {
                    this.CustomFrequency.Value = this.customFrequencyValue;
                    this.customFrequencyValue = null;

                }
            }
        }

        [Column("FrequencyType")]
        public FrequencyType FrequencyTypeImpl
        {
            get { return this.FrequencyType.Value; }
            set { this.FrequencyType = value; }
        }

        public string FrequencyValue
        {
            get { return (this.CustomFrequency != null ? this.CustomFrequency.Value : String.Empty); }
            set
            {
                //Setter must be used only by the database manager
                if (this.CustomFrequency != null)
                    this.CustomFrequency.Value = value;
                else
                {
                    this.customFrequencyValue = value;
                }
            }
        }

        public string FrequencyDescription
        {
            get
            {
                if (!this.IsPeriodic)
                    return string.Empty;

                return this.CustomFrequency.DisplayValue;
            }
        }

        [Ignore]
        public ICustomFrequency CustomFrequency
        {
            get
            {
                return this.customFrequency;
            }
            set
            {
                // caution here: we use .Equals because the method is overriden to detect "same" custom frequency
                // we do that because we do not want to raise "false" change notification
                if ((this.customFrequency == null && value != null) || (this.customFrequency != null && !this.customFrequency.Equals(value)))
                {
                    this.customFrequency = value;
                    this.RaisePropertyChanged("CustomFrequency", "FrequencyType", "FrequencyValue", "IsPeriodic", "FrequencyDescription");
                }
            }
        }

        public TaskAction Action
        {
            get
            {
                return this.action;
            }
            set
            {
                if (this.action != value)
                {
                    this.action = value;
                    this.RaisePropertyChanged("Action");
                }
            }
        }

        public string ActionName
        {
            get
            {
                return this.actionName;
            }
            set
            {
                if (this.actionName != value)
                {
                    this.actionName = value;
                    this.RaisePropertyChanged("ActionName");
                }
            }
        }

        public string ActionValue
        {
            get
            {
                return this.actionValue;
            }
            set
            {
                if (this.actionValue != value)
                {
                    this.actionValue = value;
                    this.RaisePropertyChanged("ActionValue");
                }
            }
        }

        public DateTime? Alarm
        {
            get
            {
                return this.alarm;
            }
            set
            {
                if (this.alarm != value)
                {
                    this.alarm = value;
                    this.RaisePropertyChanged("Alarm", "AlarmTime");
                }
            }
        }

        public string AlarmTime
        {
            get
            {
                if (this.alarm.HasValue)
                    return this.alarm.Value.ToString("M") + " " + this.alarm.Value.ToString("t");

                return string.Empty;
            }
        }

        public double? Progress
        {
            get
            {
                return this.progress;
            }
            set
            {
                if (this.progress != value)
                {
                    this.progress = value;
                    this.RaisePropertyChanged("Progress", "HasProgress");
                }
            }
        }

        public bool HasProgress
        {
            get { return this.progress != null; }
        }

        [Ignore]
        public IFolder Folder   
        {
            get
            {
                return this.folder;
            }
            set
            {
                if (this.folder != value)
                {
                    if (this.folder != null)
                        ((Folder)this.folder).UnregisterTask(this);

                    // setting folder to null is allowed for soft delete only
                    this.folder = value;
                    if (this.folder != null)
                    {
                        this.folderId = this.folder.Id;
                        ((Folder) this.folder).RegisterTask(this);
                    }

                    this.RaisePropertyChanged("Folder");
                }
            }
        }

        public int FolderId
        {
            get
            {
                if (this.folderId.HasValue)
                    return this.folderId.Value;
                else
                    return this.folder != null ? this.folder.Id : -1;
            }
            set
            {
                if (this.folder != null)
                    throw new NotSupportedException();
                else
                    this.folderId = value;
            }
        }

        [Ignore]
        public IContext Context
        {
            get
            {
                return this.context;
            }
            set
            {
                if (this.context != value)
                {
                    if (this.context != null)
                        ((Context)this.context).UnregisterTask(this);

                    this.context = value;
                    if (this.context != null)
                    {
                        ((Context)this.context).RegisterTask(this);
                        this.contextId = this.context.Id;
                    }
                    else
                    {
                        this.contextId = null;
                    }
                                        
                    this.RaisePropertyChanged("Context");
                }
            }
        }

        public int ContextId
        {
            get
            {
                if (this.contextId.HasValue)
                    return this.contextId.Value;
                else
                    return this.context != null ? this.context.Id : -1;
            }
            set
            {
                if (this.context != null)
                    throw new NotSupportedException();
                else
                    this.contextId = value;
            }
        }

        [Ignore]
        public bool IsBeingEdited { get; set; }

        public Task()
        {
            this.Children = new ObservableCollection<ITask>();
        }

        public void Delete()
        {
            if (this.Children.Count > 0)
            {
                foreach (var subtask in this.Children.ToList())
                    this.Folder.RemoveTask(subtask);
            }
            this.Folder.RemoveTask(this);
        }

        public void AddChild(ITask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (!this.Children.Contains(task))
            {
                // check if the child task already has a parent
                if (task.ParentId != null && task.Folder != null)
                {
                    // find the exiting parent task
                    var currentParent = task.Folder.Tasks.FirstOrDefault(t => t.Id == task.ParentId);
                    if (currentParent != null)
                        currentParent.RemoveChild(task);
                }

                this.Children.Add(task);
                task.ParentId = this.Id;
                task.PropertyChanged += this.OnChildTaskPropertyChanged;

                this.childrenDescriptor = null;
                this.RaisePropertyChanged("ChildrenDescriptor");
            }
        }

        public void RemoveChild(ITask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (this.Children.Contains(task))
            {
                this.Children.Remove(task);
                task.ParentId = null;
                task.PropertyChanged -= this.OnChildTaskPropertyChanged;

                this.childrenDescriptor = null;
                this.RaisePropertyChanged("ChildrenDescriptor");
            }
        }

        public override string ToString()
        {
            return this.title;
        }

        private void OnChildTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsCompleted")
            {
                this.childrenDescriptor = null;
                this.RaisePropertyChanged("ChildrenDescriptor");
            }
        }
    }
}

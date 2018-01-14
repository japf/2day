using System;
using System.Collections.ObjectModel;
using Chartreuse.Today.Core.Shared.Model.Recurrence;

namespace Chartreuse.Today.Core.Shared.Model
{
    public interface ITask : IModelEntity
    {
        int Id { get; set; }

        int? ParentId { get; set; }
        ObservableCollection<ITask> Children { get; set; }
        string ChildrenDescriptor { get; }

        string SyncId { get; set; }

        string Title { get; set; }
        IFolder Folder { get; set; }
        IContext Context { get; set; }

        bool IsBeingEdited { get; set; }
        bool IsCompleted { get; set; }
		bool IsLate { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this task has been generated from a recurring pattern
        /// (ie. when a user completes a recurring task the app creates a new task, this new task has this flag sets to true)
        /// </summary>
        bool HasRecurringOrigin { get; set; }

        DateTime Added { get; set; }
        DateTime Modified { get; set; }
        DateTime? Start { get; set; }
        DateTime? Due { get; set; }
        DateTime? Completed { get; set; }

        ICustomFrequency CustomFrequency { get; set; }
        bool UseFixedDate { get; set; }
        bool IsPeriodic { get; }

        FrequencyType? FrequencyType { get; set; }
        string FrequencyValue { get; set; }
        string FrequencyDescription { get; }

        TaskPriority Priority { get; set; }
        string Note { get; set; }
        string DisplayNote { get; }
        bool HasDisplayNote { get; }

        string Tags { get; set; }
        bool HasTags { get; }

        TaskAction Action { get; set; }
        string ActionName { get; set; }
        string ActionValue { get; set; }

        DateTime? Alarm { get; set; }
        string AlarmTime { get; }

        double? Progress { get; set; }
        bool HasProgress { get; }

        void Delete();

        void AddChild(ITask task);
        void RemoveChild(ITask task);
    }
}

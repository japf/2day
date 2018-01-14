using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Chartreuse.Today.Exchange.Model
{
	[DataContract]
    [KnownType(typeof(ExchangeTask))]
    [KnownType(typeof(ExchangeTaskImportance))]
    [DebuggerDisplay("Subject: {Subject} Category: {Category} Importance: {Importance} Due: {Due} Completed: {Completed} Recurring: {IsRecurring}")]
	public class ExchangeTask
	{
	    [DataMember]
		public string Subject
		{
			get;
			set;
		}

	    [DataMember]
	    public ExchangeTaskImportance Importance
	    {
	        get; 
            set;
	    }

		[DataMember]
		public DateTime? Completed
		{
			get;
			set;
		}

        [DataMember]
        public DateTime? Start
        {
            get;
            set;
        }

		[DataMember]
		public DateTime? Due
		{
			get;
			set;
		}

        [DataMember]
        public DateTime? Alarm
        {
            get;
            set;
        }

		[DataMember]
		public int LocalId
		{
			get;
			set;
		}

        [DataMember]
        public string Id
        {
            get;
            set;
        }

	    [DataMember]
	    public string Category
	    {
	        get; 
            set;
	    }

        [DataMember]
        public string Note
        {
            get;
            set;
        }

	    [DataMember(IsRequired = false)]
	    public double ProgressPercent
	    {
	        get; 
            set;
	    }

        [DataMember]
        public bool IsRecurring
        {
            get;
            set;
        }

        [DataMember]
        public ExchangeRecurrencePattern RecurrenceType
        {
            get;
            set;
        }

	    [DataMember]
	    public int Interval
	    {
	        get; 
            set;
	    }

	    [DataMember]
	    public ExchangeDayOfWeek DaysOfWeek
	    {
	        get; 
            set;
	    }

        [DataMember]
        public ExchangeDayOfWeekIndex DayOfWeekIndex
        {
            get;
            set;
        }

        [DataMember]
        public int DayOfMonth
        {
            get;
            set;
        }

        [DataMember]
        public int Month
        {
            get;
            set;
        }

	    [DataMember]
	    public ExchangeTaskProperties? Properties
	    {
	        get; 
            set;
	    }

        public DateTime Created { get; set; }

	    public bool UseFixedDate { get; set; }

	    protected bool Equals(ExchangeTask other)
        {
            return string.Equals(this.Subject, other.Subject) 
                && this.Importance == other.Importance 
                && this.Completed.Equals(other.Completed)
                && this.Due.Equals(other.Due)
                && this.Start.Equals(other.Start) 
                && this.LocalId == other.LocalId 
                && string.Equals(this.Id, other.Id) 
                && string.Equals(this.Category, other.Category) 
                && string.Equals(this.Note, other.Note) 
                && this.ProgressPercent.Equals(other.ProgressPercent) 
                && this.IsRecurring.Equals(other.IsRecurring) 
                && this.RecurrenceType == other.RecurrenceType 
                && this.Interval == other.Interval 
                && this.DaysOfWeek == other.DaysOfWeek 
                && this.DayOfMonth == other.DayOfMonth 
                && this.Month == other.Month

            && this.Created == other.Created;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((ExchangeTask)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (this.Subject != null ? this.Subject.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)this.Importance;
                hashCode = (hashCode * 397) ^ this.Completed.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Due.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Start.GetHashCode();
                hashCode = (hashCode * 397) ^ this.LocalId;
                hashCode = (hashCode * 397) ^ (this.Id != null ? this.Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Category != null ? this.Category.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Note != null ? this.Note.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.ProgressPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ this.IsRecurring.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)this.RecurrenceType;
                hashCode = (hashCode * 397) ^ this.Interval;
                hashCode = (hashCode * 397) ^ (int)this.DaysOfWeek;
                hashCode = (hashCode * 397) ^ this.DayOfMonth;
                hashCode = (hashCode * 397) ^ this.Month;

                hashCode = (hashCode * 397) ^ this.Created.GetHashCode();

                return hashCode;
            }
        }

        public static bool operator ==(ExchangeTask left, ExchangeTask right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ExchangeTask left, ExchangeTask right)
        {
            return !Equals(left, right);
        }
	}
}

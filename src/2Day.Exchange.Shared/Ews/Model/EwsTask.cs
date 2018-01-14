using System;
using System.Diagnostics;

namespace Chartreuse.Today.Exchange.Ews.Model
{
    [DebuggerDisplay("Subject: {Subject} Due: {DueDate}")]
    public class EwsTask
    {
        public EwsItemType Type { get; set; }

        public EwsFields Changes { get; set; }

        public string Id { get; set; }

        public string ChangeKey { get; set; }

        public string ParentFolderId { get; set; }

        public string ParentFolderChangeKey { get; set; }

        public string Subject { get; set; }

        public EwsBodyType BodyType { get; set; }

        public string Body { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? ReminderDate { get; set; }
        
        public DateTime? StartDate { get; set; }

        public DateTime? OrdinalDate { get; set; }

        public bool Complete { get; set; }

        public DateTime? CompleteDate { get; set; }

        public bool IsRecurring { get; set; }

        public bool IsDeadOccurence { get; set; }

        public EwsRecurrence Recurrence { get; set; }

        public string SubOrdinalDate { get; set; }

        public bool ReminderIsSet { get; set; }

        public EwsImportance Importance { get; set; }

        public string[] Categories { get; set; }

        public double PercentComplete { get; set; }

        public EwsTaskStatus Status { get; set; }

        public EwsTask()
        {
            this.Categories = new string[] {};
        }

        public void ParseRecurrence(string xml)
        {
            if (xml == null) 
                throw new ArgumentNullException("xml");

            this.Recurrence = EwsRecurrence.CreateFromXml(xml);
        }

        public string BuildRecurrenceXml()
        {
            if (this.Recurrence == null)
                return string.Empty;

            return this.Recurrence.BuildXml();
        }
    }
}
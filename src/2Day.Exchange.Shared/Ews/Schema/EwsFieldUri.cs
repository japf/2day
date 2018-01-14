namespace Chartreuse.Today.Exchange.Ews.Schema
{
    public enum EwsFieldUri
    {
        ItemClass,
        Subject,
        Importance,
        Categories,
        Body,
        Recurrence,
        ParentFolderId,
        ReminderIsSet,
        ReminderDueBy,
        Attachments,
        
        CommonStart,
        CommonEnd,

        TagSubject,
        TagSubjectPrefix,
        TagIconIndex,
        NormalizedSubject,
        FlagRequest,

        TaskTitle,
        TaskDueDate,
        TaskStartDate,
        TaskOrdinalDate,
        TaskSubOrdinalDate,
        TaskCompletedDate,
        TaskComplete,
        TaskIsRecurring,
        TaskIsDeadOccurence,
        TaskPercentComplete,
        TaskStatus,

        ReplyRequested,
        ResponseRequested,

        TagFlagStatus,
        TagFlagCompleteTime,
        TagToDoItemFlags,
        ReminderSet,
        TagFollowupIcon
    }
}
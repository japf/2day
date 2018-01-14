namespace Chartreuse.Today.Exchange.Ews.Schema
{
    // list at : https://raw.githubusercontent.com/inverse-inc/openchange/master/libmapi/conf/mapi-named-properties
    // full protocol docs: [MS-OXOCFG]: Configuration Information Protocol
    public static class EwsExtendedPropertyIds
    {
        // PidLidToDoOrdinalDate
        // Contains the current time, in UTC, which is used to determine the sort order of objects in a consolidated to-do list.
        // http://msdn.microsoft.com/en-us/library/ee124268(v=exchg.80).aspx
        public const int TaskOrdinalDate =      0x85A0; // SystemTime  34208

        // PidLidToDoSubOrdinal
        // Contains the numerals 0 through 9 that are used to break a tie when the PidLidToDoOrdinalDate property (section 2.344) is used to perform a sort of objects.
        // http://msdn.microsoft.com/en-us/library/ee159170(v=exchg.80).aspx
        public const int TaskSubOrdinalDate =   0x85A1; // String      34209

        // PidTagNormalizedSubject
        // Contains the normalized subject of the message
        public const int NormalizedSubject =    0x0E1D; // String       3613

        // PidTagSubject
        // http://msdn.microsoft.com/en-us/library/office/cc815720(v=office.15).aspx
        // Contains the full subject of a message
        public const int TagSubject =           0x0037; // String       0055          

        // PidTagSubjectPrefix
        // http://msdn.microsoft.com/en-us/library/office/cc765597(v=office.15).aspx
        // Contains the prefix for the subject of the message
        public const int TagSubjectPrefix =     0x003D; // String       0061

        // PidLidFlagRequest
        // http://msdn.microsoft.com/en-us/library/office/cc815496(v=office.15).aspx
        // Represents the status of a meeting request
        public const int FlagRequest =          0x8530; // String       34096

        // PidTagIconIndex
        // http://msdn.microsoft.com/en-us/library/office/cc815472(v=office.15).aspx
        // Contains a number that indicates which icon to use when you display a group of e-mail objects.
        public const int TagIconIndex =         0x1080; // Long         4224

        public const int TagFlagStatus =        0x1090; // Long         4240
        public const int TagFlagCompleteTime =  0x1091; // Long         4241
        public const int TagFollowupIcon =      0x1095; // Long         4245

        public const int ReplyRequested =       0x0C17; // Boolean      3095
        public const int ResponseRequested =    0x0063; // Boolean      99
        public const int TagToDoItemFlags =     0x0E2B; // Long         3627 

        public const int TaskTitle =            0x85A4; // String       34212

        public const int TaskStatus =           0x8101; // Int          33025
        public const int TaskPercentComplete =  0x8102; // Double       33026

        /*
         * PidLidTaskStartDate
         * https://msdn.microsoft.com/en-us/library/office/cc815922.aspx
         * 
         * The PidLidTaskStartDate property ( [MS-OXPROPS] section 2.333) specifies the date on which the user expects work on the task to begin. 
         * The date is in the user's local time zone. The task has no start date if this property is unset or is set to 0x5AE980E0 (1,525,252,320). 
         * If the task has a start date, the value MUST have a time component of 12:00 midnight, and the PidLidTaskDueDate property (section 2.2.2.2.5) 
         * and the PidLidCommonStart property (section 2.2.2.1.3) MUST also be set.
         */
        public const int TaskStartDate =        0x8104; // SystemTime   33028

        /*
         * This property indicates the start time for an item. It must be less than or equal to the value of the dispidCommonEnd (PidLidCommonEnd)
         * property. The value of this property must be the Coordinated Universal Time (UTC) equivalent of the dispidTaskStartDate (PidLidTaskStartDate) 
         * property. 
         */
        public const int CommonStart = 0x8516; // SystemTime   34070

        /*
         * PidLidTaskDueDate 
         * https://msdn.microsoft.com/en-us/library/office/cc839641.aspx
         * 
         * The PidLidTaskDueDate property ([MS-OXPROPS] section 2.314) specifies the date by which the user expects work on the task to be complete.
         * The date is in the user's local time zone. The task has no due date if this property is unset or is set to 0x5AE980E0 (1,525,252,320). 
         * However, a due date is optional only if no start date is indicated in the PidLidTaskStartDate property (section 2.2.2.2.4). 
         * If the task has a due date, the value MUST have a time component of 12:00 midnight, and the PidLidCommonEnd property (section 2.2.2.1.4) 
         * MUST also be set. If the PidLidTaskStartDate property has a start date, then the value of this property MUST be greater than or equal to 
         * the value of the PidLidTaskStartDate property.
         */
        public const int TaskDueDate =          0x8105; // SystemTime   33029

        /*
         * This property indicates the end time for an item. It must be greater than or equal to the value of the dispidCommonStart (PidLidCommonStart) property.
         * This value must be the Coordinated Universal Time (UTC) equivalent of the dispidTaskDueDate (PidLidTaskDueDate) property. 
         */
        public const int CommonEnd = 0x8517; // SystemTime   34071

        /*
         * PidLidTaskDateCompleted
         * https://msdn.microsoft.com/en-us/library/office/cc815753.aspx
         * 
         * The PidLidTaskDateCompleted property ([MS-OXPROPS] section 2.312) specifies the date when the user completed work on the task. 
         * This property can be left unset; if set, this property MUST have a time component of 12:00 midnight in the local time zone, 
         * converted to UTC.
         */
        public const int TaskCompletedDate =    0x810F; // SystemTime   33039

        /*
         * PidLidTaskComplete
         * https://msdn.microsoft.com/en-us/library/office/cc839514.aspx
         * 
         * The PidLidTaskComplete property ([MS-OXPROPS] section 2.310) indicates whether the task has been completed. 
         * The client sets this property to nonzero (TRUE) when the task has been completed; otherwise, this property is set to zero (FALSE). 
         */
        public const int TaskComplete =         0x811C; // Boolean      33052

        /*
         * PidLidTaskRecurrence
         * https://msdn.microsoft.com/en-us/library/office/cc839935.aspx
         * 
         * The PidLidTaskRecurrence property ([MS-OXPROPS] section 2.330) contains a RecurrencePattern structure, as specified in [MS-OXOCAL]
         * section 2.2.1.44.1, that provides information about recurring tasks. Both the DeletedInstanceCount field and the ModifiedInstanceCount
         * field of the RecurrencePattern structure MUST be set zero. 
         */
        public const int TaskRecurrence =       0x8116; // Binary       33046

        public const int TaskIsRecurring =      0x8126; // Boolean      33062

        public const int TaskIsDeadOccurrence = 0x8109; // Boolean      33033

        /*
         * The PidLidReminderSet property ([MS-OXPROPS] section 2.222) specifies whether a reminder is set on the Message object.
         * If a Recurring Calendar object has the PidLidReminderSet property set to TRUE, the client can override this value for exceptions. 
         * For details, see the definition of the PidLidAppointmentRecur property in [MS-OXOCAL] section 2.2.1.44.
         * If the PidLidReminderSet property is set to FALSE on a Recurring Calendar object, reminders are disabled for the entire series, 
         * including exceptions. For Recurring Task objects, the PidLidReminderSet property cannot be overridden by exceptions. 
         * For details, see [MS-OXOTASK] section 2.2.2.2.6. 
         */
        public const int ReminderSet =          0x8503; // Boolean      34051
    }

    public enum PidLidTaskStatusEnum
    {
        NotStarted,
        InProgress,
        Completed,
    }

    public enum PidTagTaskStatusEnum
    {
        NotFlagged,
        Complete,
        Flagged,
    }

    public enum PidTagFollowupIconEnum
    {
        Followup = 6,
    }

}
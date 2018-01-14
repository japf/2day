using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Chartreuse.Today.Exchange.ActiveSync.Encoder
{
    internal enum GlobalTokens
    {
        SWITCH_PAGE = 0x00,
        END = 0x01,
        ENTITY = 0x02,
        STR_I = 0x03,
        LITERAL = 0x04,
        EXT_I_0 = 0x40,
        EXT_I_1 = 0x41,
        EXT_I_2 = 0x42,
        PI = 0x43,
        LITERAL_C = 0x44,
        EXT_T_0 = 0x80,
        EXT_T_1 = 0x81,
        EXT_T_2 = 0x82,
        STR_T = 0x83,
        LITERAL_A = 0x84,
        EXT_0 = 0xC0,
        EXT_1 = 0xC1,
        EXT_2 = 0xC2,
        OPAQUE = 0xC3,
        LITERAL_AC = 0xC4
    }

    internal class ASWBXML
    {
        private const byte versionByte = 0x03;
        private const byte publicIdentifierByte = 0x01;
        private const byte characterSetByte = 0x6A; // UTF-8
        private const byte stringTableLengthByte = 0x00;

        private XDocument xmlDoc = new XDocument();
        private ASWBXMLCodePage[] codePages;
        private int currentCodePage = 0;
        private int defaultCodePage = -1;

        public ASWBXML()
        {
            // Load up code pages
            // Currently there are 25 code pages as per MS-ASWBXML
            this.codePages = new ASWBXMLCodePage[25];

            #region Code Page Initialization

            // Code Page 0: AirSync

            #region AirSync Code Page

            this.codePages[0] = new ASWBXMLCodePage();
            this.codePages[0].Namespace = "AirSync";
            this.codePages[0].Xmlns = "airsync";

            this.codePages[0].AddToken(0x05, "Sync");
            this.codePages[0].AddToken(0x06, "Responses");
            this.codePages[0].AddToken(0x07, "Add");
            this.codePages[0].AddToken(0x08, "Change");
            this.codePages[0].AddToken(0x09, "Delete");
            this.codePages[0].AddToken(0x0A, "Fetch");
            this.codePages[0].AddToken(0x0B, "SyncKey");
            this.codePages[0].AddToken(0x0C, "ClientId");
            this.codePages[0].AddToken(0x0D, "ServerId");
            this.codePages[0].AddToken(0x0E, "Status");
            this.codePages[0].AddToken(0x0F, "Collection");
            this.codePages[0].AddToken(0x10, "Class");
            this.codePages[0].AddToken(0x12, "CollectionId");
            this.codePages[0].AddToken(0x13, "GetChanges");
            this.codePages[0].AddToken(0x14, "MoreAvailable");
            this.codePages[0].AddToken(0x15, "WindowSize");
            this.codePages[0].AddToken(0x16, "Commands");
            this.codePages[0].AddToken(0x17, "Options");
            this.codePages[0].AddToken(0x18, "FilterType");
            this.codePages[0].AddToken(0x1B, "Conflict");
            this.codePages[0].AddToken(0x1C, "Collections");
            this.codePages[0].AddToken(0x1D, "ApplicationData");
            this.codePages[0].AddToken(0x1E, "DeletesAsMoves");
            this.codePages[0].AddToken(0x20, "Supported");
            this.codePages[0].AddToken(0x21, "SoftDelete");
            this.codePages[0].AddToken(0x22, "MIMESupport");
            this.codePages[0].AddToken(0x23, "MIMETruncation");
            this.codePages[0].AddToken(0x24, "Wait");
            this.codePages[0].AddToken(0x25, "Limit");
            this.codePages[0].AddToken(0x26, "Partial");
            this.codePages[0].AddToken(0x27, "ConversationMode");
            this.codePages[0].AddToken(0x28, "MaxItems");
            this.codePages[0].AddToken(0x29, "HeartbeatInterval");

            #endregion

            // Code Page 1: Contacts

            #region Contacts Code Page

            this.codePages[1] = new ASWBXMLCodePage();
            this.codePages[1].Namespace = "Contacts";
            this.codePages[1].Xmlns = "contacts";

            this.codePages[1].AddToken(0x05, "Anniversary");
            this.codePages[1].AddToken(0x06, "AssistantName");
            this.codePages[1].AddToken(0x07, "AssistantPhoneNumber");
            this.codePages[1].AddToken(0x08, "Birthday");
            this.codePages[1].AddToken(0x0C, "Business2PhoneNumber");
            this.codePages[1].AddToken(0x0D, "BusinessAddressCity");
            this.codePages[1].AddToken(0x0E, "BusinessAddressCountry");
            this.codePages[1].AddToken(0x0F, "BusinessAddressPostalCode");
            this.codePages[1].AddToken(0x10, "BusinessAddressState");
            this.codePages[1].AddToken(0x11, "BusinessAddressStreet");
            this.codePages[1].AddToken(0x12, "BusinessFaxNumber");
            this.codePages[1].AddToken(0x13, "BusinessPhoneNumber");
            this.codePages[1].AddToken(0x14, "CarPhoneNumber");
            this.codePages[1].AddToken(0x15, "Categories");
            this.codePages[1].AddToken(0x16, "Category");
            this.codePages[1].AddToken(0x17, "Children");
            this.codePages[1].AddToken(0x18, "Child");
            this.codePages[1].AddToken(0x19, "CompanyName");
            this.codePages[1].AddToken(0x1A, "Department");
            this.codePages[1].AddToken(0x1B, "Email1Address");
            this.codePages[1].AddToken(0x1C, "Email2Address");
            this.codePages[1].AddToken(0x1D, "Email3Address");
            this.codePages[1].AddToken(0x1E, "FileAs");
            this.codePages[1].AddToken(0x1F, "FirstName");
            this.codePages[1].AddToken(0x20, "Home2PhoneNumber");
            this.codePages[1].AddToken(0x21, "HomeAddressCity");
            this.codePages[1].AddToken(0x22, "HomeAddressCountry");
            this.codePages[1].AddToken(0x23, "HomeAddressPostalCode");
            this.codePages[1].AddToken(0x24, "HomeAddressState");
            this.codePages[1].AddToken(0x25, "HomeAddressStreet");
            this.codePages[1].AddToken(0x26, "HomeFaxNumber");
            this.codePages[1].AddToken(0x27, "HomePhoneNumber");
            this.codePages[1].AddToken(0x28, "JobTitle");
            this.codePages[1].AddToken(0x29, "LastName");
            this.codePages[1].AddToken(0x2A, "MiddleName");
            this.codePages[1].AddToken(0x2B, "MobilePhoneNumber");
            this.codePages[1].AddToken(0x2C, "OfficeLocation");
            this.codePages[1].AddToken(0x2D, "OtherAddressCity");
            this.codePages[1].AddToken(0x2E, "OtherAddressCountry");
            this.codePages[1].AddToken(0x2F, "OtherAddressPostalCode");
            this.codePages[1].AddToken(0x30, "OtherAddressState");
            this.codePages[1].AddToken(0x31, "OtherAddressStreet");
            this.codePages[1].AddToken(0x32, "PagerNumber");
            this.codePages[1].AddToken(0x33, "RadioPhoneNumber");
            this.codePages[1].AddToken(0x34, "Spouse");
            this.codePages[1].AddToken(0x35, "Suffix");
            this.codePages[1].AddToken(0x36, "Title");
            this.codePages[1].AddToken(0x37, "WebPage");
            this.codePages[1].AddToken(0x38, "YomiCompanyName");
            this.codePages[1].AddToken(0x39, "YomiFirstName");
            this.codePages[1].AddToken(0x3A, "YomiLastName");
            this.codePages[1].AddToken(0x3C, "Picture");
            this.codePages[1].AddToken(0x3D, "Alias");
            this.codePages[1].AddToken(0x3E, "WeightedRank");

            #endregion

            // Code Page 2: Email

            #region Email Code Page

            this.codePages[2] = new ASWBXMLCodePage();
            this.codePages[2].Namespace = "Email";
            this.codePages[2].Xmlns = "email";

            this.codePages[2].AddToken(0x0F, "DateReceived");
            this.codePages[2].AddToken(0x11, "DisplayTo");
            this.codePages[2].AddToken(0x12, "Importance");
            this.codePages[2].AddToken(0x13, "MessageClass");
            this.codePages[2].AddToken(0x14, "Subject");
            this.codePages[2].AddToken(0x15, "Read");
            this.codePages[2].AddToken(0x16, "To");
            this.codePages[2].AddToken(0x17, "Cc");
            this.codePages[2].AddToken(0x18, "From");
            this.codePages[2].AddToken(0x19, "ReplyTo");
            this.codePages[2].AddToken(0x1A, "AllDayEvent");
            this.codePages[2].AddToken(0x1B, "Categories");
            this.codePages[2].AddToken(0x1C, "Category");
            this.codePages[2].AddToken(0x1D, "DtStamp");
            this.codePages[2].AddToken(0x1E, "EndTime");
            this.codePages[2].AddToken(0x1F, "InstanceType");
            this.codePages[2].AddToken(0x20, "BusyStatus");
            this.codePages[2].AddToken(0x21, "Location");
            this.codePages[2].AddToken(0x22, "MeetingRequest");
            this.codePages[2].AddToken(0x23, "Organizer");
            this.codePages[2].AddToken(0x24, "RecurrenceId");
            this.codePages[2].AddToken(0x25, "Reminder");
            this.codePages[2].AddToken(0x26, "ResponseRequested");
            this.codePages[2].AddToken(0x27, "Recurrences");
            this.codePages[2].AddToken(0x28, "Recurrence");
            this.codePages[2].AddToken(0x29, "Type");
            this.codePages[2].AddToken(0x2A, "Until");
            this.codePages[2].AddToken(0x2B, "Occurrences");
            this.codePages[2].AddToken(0x2C, "Interval");
            this.codePages[2].AddToken(0x2D, "DayOfWeek");
            this.codePages[2].AddToken(0x2E, "DayOfMonth");
            this.codePages[2].AddToken(0x2F, "WeekOfMonth");
            this.codePages[2].AddToken(0x30, "MonthOfYear");
            this.codePages[2].AddToken(0x31, "StartTime");
            this.codePages[2].AddToken(0x32, "Sensitivity");
            this.codePages[2].AddToken(0x33, "TimeZone");
            this.codePages[2].AddToken(0x34, "GlobalObjId");
            this.codePages[2].AddToken(0x35, "ThreadTopic");
            this.codePages[2].AddToken(0x39, "InternetCPID");
            this.codePages[2].AddToken(0x3A, "Flag");
            this.codePages[2].AddToken(0x3B, "Status");
            this.codePages[2].AddToken(0x3C, "ContentClass");
            this.codePages[2].AddToken(0x3D, "FlagType");
            this.codePages[2].AddToken(0x3E, "CompleteTime");
            this.codePages[2].AddToken(0x3F, "DisallowNewTimeProposal");

            #endregion

            // Code Page 3: AirNotify

            #region AirNotify Code Page

            this.codePages[3] = new ASWBXMLCodePage();
            this.codePages[3].Namespace = "";
            this.codePages[3].Xmlns = "";

            #endregion

            // Code Page 4: Calendar

            #region Calendar Code Page

            this.codePages[4] = new ASWBXMLCodePage();
            this.codePages[4].Namespace = "Calendar";
            this.codePages[4].Xmlns = "calendar";

            this.codePages[4].AddToken(0x05, "TimeZone");
            this.codePages[4].AddToken(0x06, "AllDayEvent");
            this.codePages[4].AddToken(0x07, "Attendees");
            this.codePages[4].AddToken(0x08, "Attendee");
            this.codePages[4].AddToken(0x09, "Email");
            this.codePages[4].AddToken(0x0A, "Name");
            this.codePages[4].AddToken(0x0D, "BusyStatus");
            this.codePages[4].AddToken(0x0E, "Categories");
            this.codePages[4].AddToken(0x0F, "Category");
            this.codePages[4].AddToken(0x11, "DtStamp");
            this.codePages[4].AddToken(0x12, "EndTime");
            this.codePages[4].AddToken(0x13, "Exception");
            this.codePages[4].AddToken(0x14, "Exceptions");
            this.codePages[4].AddToken(0x15, "Deleted");
            this.codePages[4].AddToken(0x16, "ExceptionStartTime");
            this.codePages[4].AddToken(0x17, "Location");
            this.codePages[4].AddToken(0x18, "MeetingStatus");
            this.codePages[4].AddToken(0x19, "OrganizerEmail");
            this.codePages[4].AddToken(0x1A, "OrganizerName");
            this.codePages[4].AddToken(0x1B, "Recurrence");
            this.codePages[4].AddToken(0x1C, "Type");
            this.codePages[4].AddToken(0x1D, "Until");
            this.codePages[4].AddToken(0x1E, "Occurrences");
            this.codePages[4].AddToken(0x1F, "Interval");
            this.codePages[4].AddToken(0x20, "DayOfWeek");
            this.codePages[4].AddToken(0x21, "DayOfMonth");
            this.codePages[4].AddToken(0x22, "WeekOfMonth");
            this.codePages[4].AddToken(0x23, "MonthOfYear");
            this.codePages[4].AddToken(0x24, "Reminder");
            this.codePages[4].AddToken(0x25, "Sensitivity");
            this.codePages[4].AddToken(0x26, "Subject");
            this.codePages[4].AddToken(0x27, "StartTime");
            this.codePages[4].AddToken(0x28, "UID");
            this.codePages[4].AddToken(0x29, "AttendeeStatus");
            this.codePages[4].AddToken(0x2A, "AttendeeType");
            this.codePages[4].AddToken(0x33, "DisallowNewTimeProposal");
            this.codePages[4].AddToken(0x34, "ResponseRequested");
            this.codePages[4].AddToken(0x35, "AppointmentReplyTime");
            this.codePages[4].AddToken(0x36, "ResponseType");
            this.codePages[4].AddToken(0x37, "CalendarType");
            this.codePages[4].AddToken(0x38, "IsLeapMonth");
            this.codePages[4].AddToken(0x39, "FirstDayOfWeek");
            this.codePages[4].AddToken(0x3A, "OnlineMeetingConfLink");
            this.codePages[4].AddToken(0x3B, "OnlineMeetingExternalLink");

            #endregion

            // Code Page 5: Move

            #region Move Code Page

            this.codePages[5] = new ASWBXMLCodePage();
            this.codePages[5].Namespace = "Move";
            this.codePages[5].Xmlns = "move";

            this.codePages[5].AddToken(0x05, "MoveItems");
            this.codePages[5].AddToken(0x06, "Move");
            this.codePages[5].AddToken(0x07, "SrcMsgId");
            this.codePages[5].AddToken(0x08, "SrcFldId");
            this.codePages[5].AddToken(0x09, "DstFldId");
            this.codePages[5].AddToken(0x0A, "Response");
            this.codePages[5].AddToken(0x0B, "Status");
            this.codePages[5].AddToken(0x0C, "DstMsgId");

            #endregion

            // Code Page 6: ItemEstimate

            #region ItemEstimate Code Page

            this.codePages[6] = new ASWBXMLCodePage();
            this.codePages[6].Namespace = "GetItemEstimate";
            this.codePages[6].Xmlns = "getitemestimate";

            this.codePages[6].AddToken(0x05, "GetItemEstimate");
            this.codePages[6].AddToken(0x06, "Version");
            this.codePages[6].AddToken(0x07, "Collections");
            this.codePages[6].AddToken(0x08, "Collection");
            this.codePages[6].AddToken(0x09, "Class");
            this.codePages[6].AddToken(0x0A, "CollectionId");
            this.codePages[6].AddToken(0x0B, "DateTime");
            this.codePages[6].AddToken(0x0C, "Estimate");
            this.codePages[6].AddToken(0x0D, "Response");
            this.codePages[6].AddToken(0x0E, "Status");

            #endregion

            // Code Page 7: FolderHierarchy

            #region FolderHierarchy Code Page

            this.codePages[7] = new ASWBXMLCodePage();
            this.codePages[7].Namespace = "FolderHierarchy";
            this.codePages[7].Xmlns = "folderhierarchy";

            this.codePages[7].AddToken(0x07, "DisplayName");
            this.codePages[7].AddToken(0x08, "ServerId");
            this.codePages[7].AddToken(0x09, "ParentId");
            this.codePages[7].AddToken(0x0A, "Type");
            this.codePages[7].AddToken(0x0C, "Status");
            this.codePages[7].AddToken(0x0E, "Changes");
            this.codePages[7].AddToken(0x0F, "Add");
            this.codePages[7].AddToken(0x10, "Delete");
            this.codePages[7].AddToken(0x11, "Update");
            this.codePages[7].AddToken(0x12, "SyncKey");
            this.codePages[7].AddToken(0x13, "FolderCreate");
            this.codePages[7].AddToken(0x14, "FolderDelete");
            this.codePages[7].AddToken(0x15, "FolderUpdate");
            this.codePages[7].AddToken(0x16, "FolderSync");
            this.codePages[7].AddToken(0x17, "Count");

            #endregion

            // Code Page 8: MeetingResponse

            #region MeetingResponse Code Page

            this.codePages[8] = new ASWBXMLCodePage();
            this.codePages[8].Namespace = "MeetingResponse";
            this.codePages[8].Xmlns = "meetingresponse";

            this.codePages[8].AddToken(0x05, "CalendarId");
            this.codePages[8].AddToken(0x06, "CollectionId");
            this.codePages[8].AddToken(0x07, "MeetingResponse");
            this.codePages[8].AddToken(0x08, "RequestId");
            this.codePages[8].AddToken(0x09, "Request");
            this.codePages[8].AddToken(0x0A, "Result");
            this.codePages[8].AddToken(0x0B, "Status");
            this.codePages[8].AddToken(0x0C, "UserResponse");
            this.codePages[8].AddToken(0x0E, "InstanceId");

            #endregion

            // Code Page 9: Tasks

            #region Tasks Code Page

            this.codePages[9] = new ASWBXMLCodePage();
            this.codePages[9].Namespace = "Tasks";
            this.codePages[9].Xmlns = "tasks";

            this.codePages[9].AddToken(0x08, "Categories");
            this.codePages[9].AddToken(0x09, "Category");
            this.codePages[9].AddToken(0x0A, "Complete");
            this.codePages[9].AddToken(0x0B, "DateCompleted");
            this.codePages[9].AddToken(0x0C, "DueDate");
            this.codePages[9].AddToken(0x0D, "UtcDueDate");
            this.codePages[9].AddToken(0x0E, "Importance");
            this.codePages[9].AddToken(0x0F, "Recurrence");
            this.codePages[9].AddToken(0x10, "Type");
            this.codePages[9].AddToken(0x11, "Start");
            this.codePages[9].AddToken(0x12, "Until");
            this.codePages[9].AddToken(0x13, "Occurrences");
            this.codePages[9].AddToken(0x14, "Interval");
            this.codePages[9].AddToken(0x15, "DayOfMonth");
            this.codePages[9].AddToken(0x16, "DayOfWeek");
            this.codePages[9].AddToken(0x17, "WeekOfMonth");
            this.codePages[9].AddToken(0x18, "MonthOfYear");
            this.codePages[9].AddToken(0x19, "Regenerate");
            this.codePages[9].AddToken(0x1A, "DeadOccur");
            this.codePages[9].AddToken(0x1B, "ReminderSet");
            this.codePages[9].AddToken(0x1C, "ReminderTime");
            this.codePages[9].AddToken(0x1D, "Sensitivity");
            this.codePages[9].AddToken(0x1E, "StartDate");
            this.codePages[9].AddToken(0x1F, "UtcStartDate");
            this.codePages[9].AddToken(0x20, "Subject");
            this.codePages[9].AddToken(0x22, "OrdinalDate");
            this.codePages[9].AddToken(0x23, "SubOrdinalDate");
            this.codePages[9].AddToken(0x24, "CalendarType");
            this.codePages[9].AddToken(0x25, "IsLeapMonth");
            this.codePages[9].AddToken(0x26, "FirstDayOfWeek");

            #endregion

            // Code Page 10: ResolveRecipients

            #region ResolveRecipients Code Page

            this.codePages[10] = new ASWBXMLCodePage();
            this.codePages[10].Namespace = "ResolveRecipients";
            this.codePages[10].Xmlns = "resolverecipients";

            this.codePages[10].AddToken(0x05, "ResolveRecipients");
            this.codePages[10].AddToken(0x06, "Response");
            this.codePages[10].AddToken(0x07, "Status");
            this.codePages[10].AddToken(0x08, "Type");
            this.codePages[10].AddToken(0x09, "Recipient");
            this.codePages[10].AddToken(0x0A, "DisplayName");
            this.codePages[10].AddToken(0x0B, "EmailAddress");
            this.codePages[10].AddToken(0x0C, "Certificates");
            this.codePages[10].AddToken(0x0D, "Certificate");
            this.codePages[10].AddToken(0x0E, "MiniCertificate");
            this.codePages[10].AddToken(0x0F, "Options");
            this.codePages[10].AddToken(0x10, "To");
            this.codePages[10].AddToken(0x11, "CertificateRetrieval");
            this.codePages[10].AddToken(0x12, "RecipientCount");
            this.codePages[10].AddToken(0x13, "MaxCertificates");
            this.codePages[10].AddToken(0x14, "MaxAmbiguousRecipients");
            this.codePages[10].AddToken(0x15, "CertificateCount");
            this.codePages[10].AddToken(0x16, "Availability");
            this.codePages[10].AddToken(0x17, "StartTime");
            this.codePages[10].AddToken(0x18, "EndTime");
            this.codePages[10].AddToken(0x19, "MergedFreeBusy");
            this.codePages[10].AddToken(0x1A, "Picture");
            this.codePages[10].AddToken(0x1B, "MaxSize");
            this.codePages[10].AddToken(0x1C, "Data");
            this.codePages[10].AddToken(0x1D, "MaxPictures");

            #endregion

            // Code Page 11: ValidateCert

            #region ValidateCert Code Page

            this.codePages[11] = new ASWBXMLCodePage();
            this.codePages[11].Namespace = "ValidateCert";
            this.codePages[11].Xmlns = "validatecert";

            this.codePages[11].AddToken(0x05, "ValidateCert");
            this.codePages[11].AddToken(0x06, "Certificates");
            this.codePages[11].AddToken(0x07, "Certificate");
            this.codePages[11].AddToken(0x08, "CertificateChain");
            this.codePages[11].AddToken(0x09, "CheckCRL");
            this.codePages[11].AddToken(0x0A, "Status");

            #endregion

            // Code Page 12: Contacts2

            #region Contacts2 Code Page

            this.codePages[12] = new ASWBXMLCodePage();
            this.codePages[12].Namespace = "Contacts2";
            this.codePages[12].Xmlns = "contacts2";

            this.codePages[12].AddToken(0x05, "CustomerId");
            this.codePages[12].AddToken(0x06, "GovernmentId");
            this.codePages[12].AddToken(0x07, "IMAddress");
            this.codePages[12].AddToken(0x08, "IMAddress2");
            this.codePages[12].AddToken(0x09, "IMAddress3");
            this.codePages[12].AddToken(0x0A, "ManagerName");
            this.codePages[12].AddToken(0x0B, "CompanyMainPhone");
            this.codePages[12].AddToken(0x0C, "AccountName");
            this.codePages[12].AddToken(0x0D, "NickName");
            this.codePages[12].AddToken(0x0E, "MMS");

            #endregion

            // Code Page 13: Ping

            #region Ping Code Page

            this.codePages[13] = new ASWBXMLCodePage();
            this.codePages[13].Namespace = "Ping";
            this.codePages[13].Xmlns = "ping";

            this.codePages[13].AddToken(0x05, "Ping");
            this.codePages[13].AddToken(0x06, "AutdState"); // Per MS-ASWBXML, this tag is not used by protocol
            this.codePages[13].AddToken(0x07, "Status");
            this.codePages[13].AddToken(0x08, "HeartbeatInterval");
            this.codePages[13].AddToken(0x09, "Folders");
            this.codePages[13].AddToken(0x0A, "Folder");
            this.codePages[13].AddToken(0x0B, "Id");
            this.codePages[13].AddToken(0x0C, "Class");
            this.codePages[13].AddToken(0x0D, "MaxFolders");

            #endregion

            // Code Page 14: Provision

            #region Provision Code Page

            this.codePages[14] = new ASWBXMLCodePage();
            this.codePages[14].Namespace = "Provision";
            this.codePages[14].Xmlns = "provision";

            this.codePages[14].AddToken(0x05, "Provision");
            this.codePages[14].AddToken(0x06, "Policies");
            this.codePages[14].AddToken(0x07, "Policy");
            this.codePages[14].AddToken(0x08, "PolicyType");
            this.codePages[14].AddToken(0x09, "PolicyKey");
            this.codePages[14].AddToken(0x0A, "Data");
            this.codePages[14].AddToken(0x0B, "Status");
            this.codePages[14].AddToken(0x0C, "RemoteWipe");
            this.codePages[14].AddToken(0x0D, "EASProvisionDoc");
            this.codePages[14].AddToken(0x0E, "DevicePasswordEnabled");
            this.codePages[14].AddToken(0x0F, "AlphanumericDevicePasswordRequired");
            this.codePages[14].AddToken(0x10, "RequireStorageCardEncryption");
            this.codePages[14].AddToken(0x11, "PasswordRecoveryEnabled");
            this.codePages[14].AddToken(0x13, "AttachmentsEnabled");
            this.codePages[14].AddToken(0x14, "MinDevicePasswordLength");
            this.codePages[14].AddToken(0x15, "MaxInactivityTimeDeviceLock");
            this.codePages[14].AddToken(0x16, "MaxDevicePasswordFailedAttempts");
            this.codePages[14].AddToken(0x17, "MaxAttachmentSize");
            this.codePages[14].AddToken(0x18, "AllowSimpleDevicePassword");
            this.codePages[14].AddToken(0x19, "DevicePasswordExpiration");
            this.codePages[14].AddToken(0x1A, "DevicePasswordHistory");
            this.codePages[14].AddToken(0x1B, "AllowStorageCard");
            this.codePages[14].AddToken(0x1C, "AllowCamera");
            this.codePages[14].AddToken(0x1D, "RequireDeviceEncryption");
            this.codePages[14].AddToken(0x1E, "AllowUnsignedApplications");
            this.codePages[14].AddToken(0x1F, "AllowUnsignedInstallationPackages");
            this.codePages[14].AddToken(0x20, "MinDevicePasswordComplexCharacters");
            this.codePages[14].AddToken(0x21, "AllowWiFi");
            this.codePages[14].AddToken(0x22, "AllowTextMessaging");
            this.codePages[14].AddToken(0x23, "AllowPOPIMAPEmail");
            this.codePages[14].AddToken(0x24, "AllowBluetooth");
            this.codePages[14].AddToken(0x25, "AllowIrDA");
            this.codePages[14].AddToken(0x26, "RequireManualSyncWhenRoaming");
            this.codePages[14].AddToken(0x27, "AllowDesktopSync");
            this.codePages[14].AddToken(0x28, "MaxCalendarAgeFilter");
            this.codePages[14].AddToken(0x29, "AllowHTMLEmail");
            this.codePages[14].AddToken(0x2A, "MaxEmailAgeFilter");
            this.codePages[14].AddToken(0x2B, "MaxEmailBodyTruncationSize");
            this.codePages[14].AddToken(0x2C, "MaxEmailHTMLBodyTruncationSize");
            this.codePages[14].AddToken(0x2D, "RequireSignedSMIMEMessages");
            this.codePages[14].AddToken(0x2E, "RequireEncryptedSMIMEMessages");
            this.codePages[14].AddToken(0x2F, "RequireSignedSMIMEAlgorithm");
            this.codePages[14].AddToken(0x30, "RequireEncryptionSMIMEAlgorithm");
            this.codePages[14].AddToken(0x31, "AllowSMIMEEncryptionAlgorithmNegotiation");
            this.codePages[14].AddToken(0x32, "AllowSMIMESoftCerts");
            this.codePages[14].AddToken(0x33, "AllowBrowser");
            this.codePages[14].AddToken(0x34, "AllowConsumerEmail");
            this.codePages[14].AddToken(0x35, "AllowRemoteDesktop");
            this.codePages[14].AddToken(0x36, "AllowInternetSharing");
            this.codePages[14].AddToken(0x37, "UnapprovedInROMApplicationList");
            this.codePages[14].AddToken(0x38, "ApplicationName");
            this.codePages[14].AddToken(0x39, "ApprovedApplicationList");
            this.codePages[14].AddToken(0x3A, "Hash");

            #endregion

            // Code Page 15: Search

            #region Search Code Page

            this.codePages[15] = new ASWBXMLCodePage();
            this.codePages[15].Namespace = "Search";
            this.codePages[15].Xmlns = "search";

            this.codePages[15].AddToken(0x05, "Search");
            this.codePages[15].AddToken(0x07, "Store");
            this.codePages[15].AddToken(0x08, "Name");
            this.codePages[15].AddToken(0x09, "Query");
            this.codePages[15].AddToken(0x0A, "Options");
            this.codePages[15].AddToken(0x0B, "Range");
            this.codePages[15].AddToken(0x0C, "Status");
            this.codePages[15].AddToken(0x0D, "Response");
            this.codePages[15].AddToken(0x0E, "Result");
            this.codePages[15].AddToken(0x0F, "Properties");
            this.codePages[15].AddToken(0x10, "Total");
            this.codePages[15].AddToken(0x11, "EqualTo");
            this.codePages[15].AddToken(0x12, "Value");
            this.codePages[15].AddToken(0x13, "And");
            this.codePages[15].AddToken(0x14, "Or");
            this.codePages[15].AddToken(0x15, "FreeText");
            this.codePages[15].AddToken(0x17, "DeepTraversal");
            this.codePages[15].AddToken(0x18, "LongId");
            this.codePages[15].AddToken(0x19, "RebuildResults");
            this.codePages[15].AddToken(0x1A, "LessThan");
            this.codePages[15].AddToken(0x1B, "GreaterThan");
            this.codePages[15].AddToken(0x1E, "UserName");
            this.codePages[15].AddToken(0x1F, "Password");
            this.codePages[15].AddToken(0x20, "ConversationId");
            this.codePages[15].AddToken(0x21, "Picture");
            this.codePages[15].AddToken(0x22, "MaxSize");
            this.codePages[15].AddToken(0x23, "MaxPictures");

            #endregion

            // Code Page 16: GAL

            #region GAL Code Page

            this.codePages[16] = new ASWBXMLCodePage();
            this.codePages[16].Namespace = "GAL";
            this.codePages[16].Xmlns = "gal";

            this.codePages[16].AddToken(0x05, "DisplayName");
            this.codePages[16].AddToken(0x06, "Phone");
            this.codePages[16].AddToken(0x07, "Office");
            this.codePages[16].AddToken(0x08, "Title");
            this.codePages[16].AddToken(0x09, "Company");
            this.codePages[16].AddToken(0x0A, "Alias");
            this.codePages[16].AddToken(0x0B, "FirstName");
            this.codePages[16].AddToken(0x0C, "LastName");
            this.codePages[16].AddToken(0x0D, "HomePhone");
            this.codePages[16].AddToken(0x0E, "MobilePhone");
            this.codePages[16].AddToken(0x0F, "EmailAddress");
            this.codePages[16].AddToken(0x10, "Picture");
            this.codePages[16].AddToken(0x11, "Status");
            this.codePages[16].AddToken(0x12, "Data");

            #endregion

            // Code Page 17: AirSyncBase

            #region AirSyncBase Code Page

            this.codePages[17] = new ASWBXMLCodePage();
            this.codePages[17].Namespace = "AirSyncBase";
            this.codePages[17].Xmlns = "airsyncbase";

            this.codePages[17].AddToken(0x05, "BodyPreference");
            this.codePages[17].AddToken(0x06, "Type");
            this.codePages[17].AddToken(0x07, "TruncationSize");
            this.codePages[17].AddToken(0x08, "AllOrNone");
            this.codePages[17].AddToken(0x0A, "Body");
            this.codePages[17].AddToken(0x0B, "Data");
            this.codePages[17].AddToken(0x0C, "EstimatedDataSize");
            this.codePages[17].AddToken(0x0D, "Truncated");
            this.codePages[17].AddToken(0x0E, "Attachments");
            this.codePages[17].AddToken(0x0F, "Attachment");
            this.codePages[17].AddToken(0x10, "DisplayName");
            this.codePages[17].AddToken(0x11, "FileReference");
            this.codePages[17].AddToken(0x12, "Method");
            this.codePages[17].AddToken(0x13, "ContentId");
            this.codePages[17].AddToken(0x14, "ContentLocation");
            this.codePages[17].AddToken(0x15, "IsInline");
            this.codePages[17].AddToken(0x16, "NativeBodyType");
            this.codePages[17].AddToken(0x17, "ContentType");
            this.codePages[17].AddToken(0x18, "Preview");
            this.codePages[17].AddToken(0x19, "BodyPartPreference");
            this.codePages[17].AddToken(0x1A, "BodyPart");
            this.codePages[17].AddToken(0x1B, "Status");

            #endregion

            // Code Page 18: Settings

            #region Settings Code Page

            this.codePages[18] = new ASWBXMLCodePage();
            this.codePages[18].Namespace = "Settings";
            this.codePages[18].Xmlns = "settings";

            this.codePages[18].AddToken(0x05, "Settings");
            this.codePages[18].AddToken(0x06, "Status");
            this.codePages[18].AddToken(0x07, "Get");
            this.codePages[18].AddToken(0x08, "Set");
            this.codePages[18].AddToken(0x09, "Oof");
            this.codePages[18].AddToken(0x0A, "OofState");
            this.codePages[18].AddToken(0x0B, "StartTime");
            this.codePages[18].AddToken(0x0C, "EndTime");
            this.codePages[18].AddToken(0x0D, "OofMessage");
            this.codePages[18].AddToken(0x0E, "AppliesToInternal");
            this.codePages[18].AddToken(0x0F, "AppliesToExternalKnown");
            this.codePages[18].AddToken(0x10, "AppliesToExternalUnknown");
            this.codePages[18].AddToken(0x11, "Enabled");
            this.codePages[18].AddToken(0x12, "ReplyMessage");
            this.codePages[18].AddToken(0x13, "BodyType");
            this.codePages[18].AddToken(0x14, "DevicePassword");
            this.codePages[18].AddToken(0x15, "Password");
            this.codePages[18].AddToken(0x16, "DeviceInformation");
            this.codePages[18].AddToken(0x17, "Model");
            this.codePages[18].AddToken(0x18, "IMEI");
            this.codePages[18].AddToken(0x19, "FriendlyName");
            this.codePages[18].AddToken(0x1A, "OS");
            this.codePages[18].AddToken(0x1B, "OSLanguage");
            this.codePages[18].AddToken(0x1C, "PhoneNumber");
            this.codePages[18].AddToken(0x1D, "UserInformation");
            this.codePages[18].AddToken(0x1E, "EmailAddresses");
            this.codePages[18].AddToken(0x1F, "SMTPAddress");
            this.codePages[18].AddToken(0x20, "UserAgent");
            this.codePages[18].AddToken(0x21, "EnableOutboundSMS");
            this.codePages[18].AddToken(0x22, "MobileOperator");
            this.codePages[18].AddToken(0x23, "PrimarySmtpAddress");
            this.codePages[18].AddToken(0x24, "Accounts");
            this.codePages[18].AddToken(0x25, "Account");
            this.codePages[18].AddToken(0x26, "AccountId");
            this.codePages[18].AddToken(0x27, "AccountName");
            this.codePages[18].AddToken(0x28, "UserDisplayName");
            this.codePages[18].AddToken(0x29, "SendDisabled");
            this.codePages[18].AddToken(0x2B, "RightsManagementInformation");

            #endregion

            // Code Page 19: DocumentLibrary

            #region DocumentLibrary Code Page

            this.codePages[19] = new ASWBXMLCodePage();
            this.codePages[19].Namespace = "DocumentLibrary";
            this.codePages[19].Xmlns = "documentlibrary";

            this.codePages[19].AddToken(0x05, "LinkId");
            this.codePages[19].AddToken(0x06, "DisplayName");
            this.codePages[19].AddToken(0x07, "IsFolder");
            this.codePages[19].AddToken(0x08, "CreationDate");
            this.codePages[19].AddToken(0x09, "LastModifiedDate");
            this.codePages[19].AddToken(0x0A, "IsHidden");
            this.codePages[19].AddToken(0x0B, "ContentLength");
            this.codePages[19].AddToken(0x0C, "ContentType");

            #endregion

            // Code Page 20: ItemOperations

            #region ItemOperations Code Page

            this.codePages[20] = new ASWBXMLCodePage();
            this.codePages[20].Namespace = "ItemOperations";
            this.codePages[20].Xmlns = "itemoperations";

            this.codePages[20].AddToken(0x05, "ItemOperations");
            this.codePages[20].AddToken(0x06, "Fetch");
            this.codePages[20].AddToken(0x07, "Store");
            this.codePages[20].AddToken(0x08, "Options");
            this.codePages[20].AddToken(0x09, "Range");
            this.codePages[20].AddToken(0x0A, "Total");
            this.codePages[20].AddToken(0x0B, "Properties");
            this.codePages[20].AddToken(0x0C, "Data");
            this.codePages[20].AddToken(0x0D, "Status");
            this.codePages[20].AddToken(0x0E, "Response");
            this.codePages[20].AddToken(0x0F, "Version");
            this.codePages[20].AddToken(0x10, "Schema");
            this.codePages[20].AddToken(0x11, "Part");
            this.codePages[20].AddToken(0x12, "EmptyFolderContents");
            this.codePages[20].AddToken(0x13, "DeleteSubFolders");
            this.codePages[20].AddToken(0x14, "UserName");
            this.codePages[20].AddToken(0x15, "Password");
            this.codePages[20].AddToken(0x16, "Move");
            this.codePages[20].AddToken(0x17, "DstFldId");
            this.codePages[20].AddToken(0x18, "ConversationId");
            this.codePages[20].AddToken(0x19, "MoveAlways");

            #endregion

            // Code Page 21: ComposeMail

            #region ComposeMail Code Page

            this.codePages[21] = new ASWBXMLCodePage();
            this.codePages[21].Namespace = "ComposeMail";
            this.codePages[21].Xmlns = "composemail";

            this.codePages[21].AddToken(0x05, "SendMail");
            this.codePages[21].AddToken(0x06, "SmartForward");
            this.codePages[21].AddToken(0x07, "SmartReply");
            this.codePages[21].AddToken(0x08, "SaveInSentItems");
            this.codePages[21].AddToken(0x09, "ReplaceMime");
            this.codePages[21].AddToken(0x0B, "Source");
            this.codePages[21].AddToken(0x0C, "FolderId");
            this.codePages[21].AddToken(0x0D, "ItemId");
            this.codePages[21].AddToken(0x0E, "LongId");
            this.codePages[21].AddToken(0x0F, "InstanceId");
            this.codePages[21].AddToken(0x10, "Mime");
            this.codePages[21].AddToken(0x11, "ClientId");
            this.codePages[21].AddToken(0x12, "Status");
            this.codePages[21].AddToken(0x13, "AccountId");

            #endregion

            // Code Page 22: Email2

            #region Email2 Code Page

            this.codePages[22] = new ASWBXMLCodePage();
            this.codePages[22].Namespace = "Email2";
            this.codePages[22].Xmlns = "email2";

            this.codePages[22].AddToken(0x05, "UmCallerID");
            this.codePages[22].AddToken(0x06, "UmUserNotes");
            this.codePages[22].AddToken(0x07, "UmAttDuration");
            this.codePages[22].AddToken(0x08, "UmAttOrder");
            this.codePages[22].AddToken(0x09, "ConversationId");
            this.codePages[22].AddToken(0x0A, "ConversationIndex");
            this.codePages[22].AddToken(0x0B, "LastVerbExecuted");
            this.codePages[22].AddToken(0x0C, "LastVerbExecutionTime");
            this.codePages[22].AddToken(0x0D, "ReceivedAsBcc");
            this.codePages[22].AddToken(0x0E, "Sender");
            this.codePages[22].AddToken(0x0F, "CalendarType");
            this.codePages[22].AddToken(0x10, "IsLeapMonth");
            this.codePages[22].AddToken(0x11, "AccountId");
            this.codePages[22].AddToken(0x12, "FirstDayOfWeek");
            this.codePages[22].AddToken(0x13, "MeetingMessageType");

            #endregion

            // Code Page 23: Notes

            #region Notes Code Page

            this.codePages[23] = new ASWBXMLCodePage();
            this.codePages[23].Namespace = "Notes";
            this.codePages[23].Xmlns = "notes";

            this.codePages[23].AddToken(0x05, "Subject");
            this.codePages[23].AddToken(0x06, "MessageClass");
            this.codePages[23].AddToken(0x07, "LastModifiedDate");
            this.codePages[23].AddToken(0x08, "Categories");
            this.codePages[23].AddToken(0x09, "Category");

            #endregion

            // Code Page 24: RightsManagement

            #region RightsManagement Code Page

            this.codePages[24] = new ASWBXMLCodePage();
            this.codePages[24].Namespace = "RightsManagement";
            this.codePages[24].Xmlns = "rightsmanagement";

            this.codePages[24].AddToken(0x05, "RightsManagementSupport");
            this.codePages[24].AddToken(0x06, "RightsManagementTemplates");
            this.codePages[24].AddToken(0x07, "RightsManagementTemplate");
            this.codePages[24].AddToken(0x08, "RightsManagementLicense");
            this.codePages[24].AddToken(0x09, "EditAllowed");
            this.codePages[24].AddToken(0x0A, "ReplyAllowed");
            this.codePages[24].AddToken(0x0B, "ReplyAllAllowed");
            this.codePages[24].AddToken(0x0C, "ForwardAllowed");
            this.codePages[24].AddToken(0x0D, "ModifyRecipientsAllowed");
            this.codePages[24].AddToken(0x0E, "ExtractAllowed");
            this.codePages[24].AddToken(0x0F, "PrintAllowed");
            this.codePages[24].AddToken(0x10, "ExportAllowed");
            this.codePages[24].AddToken(0x11, "ProgrammaticAccessAllowed");
            this.codePages[24].AddToken(0x12, "Owner");
            this.codePages[24].AddToken(0x13, "ContentExpiryDate");
            this.codePages[24].AddToken(0x14, "TemplateID");
            this.codePages[24].AddToken(0x15, "TemplateName");
            this.codePages[24].AddToken(0x16, "TemplateDescription");
            this.codePages[24].AddToken(0x17, "ContentOwner");
            this.codePages[24].AddToken(0x18, "RemoveRightsManagementDistribution");

            #endregion

            #endregion
        }

        public void LoadXml(string xml)
        {
            XmlReader xmlReader = XmlReader.Create(new StringReader(xml));
            this.xmlDoc = XDocument.Load(xmlReader);
        }

        public string GetXml()
        {
            MemoryStream ms = new MemoryStream();
            XmlWriter xmlw = XmlWriter.Create(ms,new XmlWriterSettings(){ Indent = true, IndentChars="    ", Encoding = Encoding.UTF8});

            this.xmlDoc.WriteTo(xmlw);
            xmlw.Flush();
            ms.Position = 0;
            StreamReader sr = new StreamReader(ms, Encoding.UTF8);
            string result = sr.ReadToEnd();
            xmlw.Dispose();
            ms.Dispose();

            return result;
        }

        public void LoadBytes(byte[] byteWBXML)
        {
            ASWBXMLByteQueue bytes = new ASWBXMLByteQueue(byteWBXML);

            // Version is ignored
            byte version = bytes.Dequeue();

            // Public Identifier is ignored
            int publicIdentifier = bytes.DequeueMultibyteInt();

            // Character set
            // Currently only UTF-8 is supported, throw if something else
            int charset = bytes.DequeueMultibyteInt();
            if (charset != 0x6A)
                throw new InvalidDataException("ASWBXML only supports UTF-8 encoded XML.");

            // String table length
            // This should be 0, MS-ASWBXML does not use string tables
            int stringTableLength = bytes.DequeueMultibyteInt();
            if (stringTableLength != 0)
                throw new InvalidDataException("WBXML data contains a string table.");

            // Now we should be at the body of the data.
            // Add the declaration
            this.xmlDoc = new XDocument(new XDeclaration("1.0","utf-8","no"));

            XNode currentNode = this.xmlDoc;

            while (bytes.Count > 0)
            {
                byte currentByte = bytes.Dequeue();

                switch ((GlobalTokens) currentByte)
                {
                        // Check for a global token that we actually implement
                    case GlobalTokens.SWITCH_PAGE:
                        int newCodePage = (int) bytes.Dequeue();
                        if (newCodePage >= 0 && newCodePage < 25)
                        {
                            this.currentCodePage = newCodePage;
                        }
                        else
                        {
                            throw new InvalidDataException(string.Format("Unknown code page ID 0x{0:X} encountered in WBXML", currentByte));
                        }
                        break;
                    case GlobalTokens.END:
                        if (currentNode.Parent != null)
                        {
                            currentNode = currentNode.Parent;
                        }
                        else
                        {
                            if (bytes.Count > 0)
                                throw new InvalidDataException("END global token encountered out of sequence");
                        }
                        break;
                    case GlobalTokens.OPAQUE:
                        int CDATALength = bytes.DequeueMultibyteInt();
                        XCData newOpaqueNode = new XCData(bytes.DequeueString(CDATALength));
                        ((XContainer)currentNode).Add(newOpaqueNode);
                        break;
                    case GlobalTokens.STR_I:
                        XNode newTextNode = new XText(bytes.DequeueString());
                        ((XContainer)currentNode).Add(newTextNode);
                        break;
                        // According to MS-ASWBXML, these features aren't used
                    case GlobalTokens.ENTITY:
                    case GlobalTokens.EXT_0:
                    case GlobalTokens.EXT_1:
                    case GlobalTokens.EXT_2:
                    case GlobalTokens.EXT_I_0:
                    case GlobalTokens.EXT_I_1:
                    case GlobalTokens.EXT_I_2:
                    case GlobalTokens.EXT_T_0:
                    case GlobalTokens.EXT_T_1:
                    case GlobalTokens.EXT_T_2:
                    case GlobalTokens.LITERAL:
                    case GlobalTokens.LITERAL_A:
                    case GlobalTokens.LITERAL_AC:
                    case GlobalTokens.LITERAL_C:
                    case GlobalTokens.PI:
                    case GlobalTokens.STR_T:
                        throw new InvalidDataException(string.Format("Encountered unknown global token 0x{0:X}.", currentByte));

                        // If it's not a global token, it should be a tag
                    default:
                        bool hasAttributes = false;
                        bool hasContent = false;

                        hasAttributes = (currentByte & 0x80) > 0;
                        hasContent = (currentByte & 0x40) > 0;

                        byte token = (byte) (currentByte & 0x3F);

                        if (hasAttributes)
                            // Maybe use Trace.Assert here?
                            throw new InvalidDataException(string.Format("Token 0x{0:X} has attributes.", token));

                        string strTag = this.codePages[this.currentCodePage].GetTag(token);
                        if (strTag == null)
                        {
                            strTag = string.Format("UNKNOWN_TAG_{0,2:X}", token);
                        }

                        XElement newNode = new XElement(strTag,
                                                        new XAttribute(
                                                            XNamespace.Xmlns + this.codePages[this.currentCodePage].Xmlns, this.codePages[this.currentCodePage].Namespace)
                            );
                            ((XContainer)currentNode).Add(newNode);

                        if (hasContent)
                        {
                            currentNode = newNode;
                        }
                        break;
                }
            }
        }

        public byte[] GetBytes()
        {
            List<byte> byteList = new List<byte>();

            byteList.Add(versionByte);
            byteList.Add(publicIdentifierByte);
            byteList.Add(characterSetByte);
            byteList.Add(stringTableLengthByte);

            foreach (XElement node in this.xmlDoc.Nodes())
            {
                byteList.AddRange(this.EncodeNode(node));
            }

            return byteList.ToArray();
        }

        private byte[] EncodeNode(XNode node)
        {
            List<byte> byteList = new List<byte>();

            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    var element = ((XElement) node);
                    if (element.Attributes().Any())
                    {
                        this.ParseXmlnsAttributes(element);
                    }

                    if (this.SetCodePageByXmlns(element.GetPrefix()))
                    {
                        byteList.Add((byte) GlobalTokens.SWITCH_PAGE);
                        byteList.Add((byte) this.currentCodePage);
                    }

                    byte token = this.codePages[this.currentCodePage].GetToken(element.Name.LocalName);

                    bool hasChildren = element.Nodes().Any();
                    if (hasChildren)
                    {
                        token |= 0x40;
                    }

                    byteList.Add(token);
                    if (hasChildren)
                    {
                        if (element.HasElements)
                        {
                            foreach (XElement child in element.Elements())
                            {
                                    byteList.AddRange(this.EncodeNode(child));
                            }
                        }
                        else
                        {
                            // Here we will have a Text node
                            foreach (XNode child in element.Nodes())
                            {
                                byteList.AddRange(this.EncodeNode(child));
                            }
                        }
                    }
                    byteList.Add((byte) GlobalTokens.END);
                    
                    break;
                case XmlNodeType.Text:
                    byteList.Add((byte) GlobalTokens.STR_I);
                    byteList.AddRange(this.EncodeString(((XText)node).Value));
                    break;
                case XmlNodeType.CDATA:
                    byteList.Add((byte) GlobalTokens.OPAQUE);
                    byteList.AddRange(this.EncodeOpaque(((XCData)node).Value));
                    break;
                default:
                    break;
            }

            return byteList.ToArray();
        }

        private int GetCodePageByXmlns(string xmlns)
        {
            for (int i = 0; i < this.codePages.Length; i++)
            {
                if (this.codePages[i].Xmlns.ToUpper() == xmlns.ToUpper())
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetCodePageByNamespace(string nameSpace)
        {
            for (int i = 0; i < this.codePages.Length; i++)
            {
                if (this.codePages[i].Namespace.ToUpper() == nameSpace.ToUpper())
                {
                    return i;
                }
            }

            return -1;
        }

        private bool SetCodePageByXmlns(string xmlns)
        {
            if (xmlns == null || xmlns == "")
            {
                // Try default namespace
                if (this.currentCodePage != this.defaultCodePage)
                {
                    this.currentCodePage = this.defaultCodePage;
                    return true;
                }

                return false;
            }

            // Try current first
            if (this.codePages[this.currentCodePage].Xmlns.ToUpper() == xmlns.ToUpper())
            {
                return false;
            }

            for (int i = 0; i < this.codePages.Length; i++)
            {
                if (this.codePages[i].Xmlns.ToUpper() == xmlns.ToUpper())
                {
                    this.currentCodePage = i;
                    return true;
                }
            }

            throw new InvalidDataException(string.Format("Unknown Xmlns: {0}.", xmlns));
        }

        private void ParseXmlnsAttributes(XElement node)
        {
            
            foreach (XAttribute attribute in node.Attributes())
            {
                if (attribute.Name.LocalName.Equals("XMLNS:XSI") || attribute.Name.LocalName.ToUpper().Equals("XMLNS:XSD"))
                    continue;
                int codePage = this.GetCodePageByNamespace(attribute.Value);

                if (attribute.Name.LocalName.ToUpper() == "XMLNS")
                {
                    this.defaultCodePage = codePage;
                }
                else if (attribute.GetPrefix().ToUpper() == "XMLNS")
                {
                    this.codePages[codePage].Xmlns = attribute.Name.LocalName;
                }
            }
        }

        private byte[] EncodeString(string value)
        {
            List<byte> byteList = new List<byte>();

            //char[] charArray = value.ToCharArray();
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            
            for (int i = 0; i < bytes.Length; i++)
            {
                byteList.Add((byte) bytes[i]);
            }

            byteList.Add(0x00);

            return byteList.ToArray();
        }

        private byte[] EncodeOpaque(string value)
        {
            List<byte> byteList = new List<byte>();

            char[] charArray = value.ToCharArray();

            byteList.AddRange(this.EncodeMultiByteInteger(charArray.Length));

            for (int i = 0; i < charArray.Length; i++)
            {
                byteList.Add((byte) charArray[i]);
            }

            return byteList.ToArray();
        }

        private byte[] EncodeMultiByteInteger(int value)
        {
            List<byte> byteList = new List<byte>();

            int shiftedValue = value;

            while (value > 0)
            {
                byte addByte = (byte) (value & 0x7F);

                if (byteList.Count > 0)
                {
                    addByte |= 0x80;
                }

                byteList.Insert(0, addByte);

                value >>= 7;
            }

            return byteList.ToArray();
        }
    }
}
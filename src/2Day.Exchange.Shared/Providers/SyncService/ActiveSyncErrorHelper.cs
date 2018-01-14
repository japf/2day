using System;
using System.Collections.Generic;
using Chartreuse.Today.Exchange.ActiveSync.Exceptions;

namespace Chartreuse.Today.Exchange.Providers.SyncService
{
    public static class ActiveSyncErrorHelper
    {
        public static bool IsStatusProvisioningError(string status, Exception exception)
        {
            string statusCode = null;

            if (!string.IsNullOrWhiteSpace(status))
                statusCode = status;
            if (statusCode != null)
                return IsProvisioningErrorStatusCode(statusCode);

            if (exception != null)
            {
                if (exception is CommandException && ((CommandException)exception).Response != null)
                    statusCode = ((int) ((CommandException)exception).Response.StatusCode).ToString();
                if (statusCode != null)
                    return IsProvisioningErrorStatusCode(statusCode);
            }

            return false;
        }

        private static bool IsProvisioningErrorStatusCode(string statusCode)
        {
            return statusCode == "142" || statusCode == "143" || statusCode == "144" || statusCode == "145" || statusCode == "449";
        }

        public static string GetFolderSyncErrorMessage(string status)
        {
            if (string.IsNullOrEmpty(status))
                return string.Empty;

            int code = -1;
            
            if (int.TryParse(status, out code))
            {
                if (GeneralErrorCodes.ContainsKey(code))
                    return GeneralErrorCodes[code];
                else if (FolderSyncErrorCodes.ContainsKey(code))
                    return FolderSyncErrorCodes[code];
                else
                    return "UnknownError";
            }

            return status;
        }

        public static string GetSyncErrorMessage(string status)
        {
            if (string.IsNullOrEmpty(status))
                return string.Empty;

            int code = -1;

            if (int.TryParse(status, out code))
            {
                if (GeneralErrorCodes.ContainsKey(code))
                    return GeneralErrorCodes[code];
                else if (SyncErrorCodes.ContainsKey(code))
                    return SyncErrorCodes[code];
                else
                    return "UnknownError";
            }

            return status;
        }

        private static readonly Dictionary<int, string> SyncErrorCodes = new Dictionary<int, string>()
        {
            { 3  ,"Invalid synchronization key" },
            { 4  ,"Protocol error" },
            { 5  ,"Server error" },
            { 6  ,"Error in client/server conversion" },
            { 7  ,"Conflict matching the client and server object" },
            { 8  ,"Object not found" },
            { 12 ,"The Sync command cannot be completed" },
            { 13 ,"The Sync command request is not complete" },
            { 14 ,"Invalid Wait or HeartbeatInterval value" },
            { 15 ,"Invalid Sync command request." },
            { 16 ,"Retry" }
        };

        private static readonly Dictionary<int, string> FolderSyncErrorCodes = new Dictionary<int, string>()
        {
            { 6  ,"Server error" },
            { 9  ,"Synchronization key mismatch or invalid synchronization key" },
            { 10 ,"Incorrectly formatted request" },
            { 11 ,"An unknown error occurred" },
            { 12 ,"Code unknown" },
        };

        private static readonly Dictionary<int, string> GeneralErrorCodes = new Dictionary<int, string>()
        {
            { 101 ,"InvalidContent" },
            { 102 ,"InvalidWBXML" },
            { 103 ,"InvalidXML" },
            { 104 ,"InvalidDateTime" },
            { 105 ,"InvalidCombinationOfIDs" },
            { 106 ,"InvalidIDs" },
            { 107 ,"InvalidMIME" },
            { 108 ,"DeviceIdMissingOrInvalid" },
            { 109 ,"DeviceTypeMissingOrInvalid" },
            { 110 ,"ServerError" },
            { 111 ,"ServerErrorRetryLater" },
            { 112 ,"ActiveDirectoryAccessDenied" },
            { 113 ,"MailboxQuotaExceeded" },
            { 114 ,"MailboxServerOffline" },
            { 115 ,"SendQuotaExceeded" },
            { 116 ,"MessageRecipientUnresolved" },
            { 117 ,"MessageReplyNotAllowed" },
            { 118 ,"MessagePreviouslySent" },
            { 119 ,"MessageHasNoRecipient" },
            { 120 ,"MailSubmissionFailed" },
            { 121 ,"MessageReplyFailed" },
            { 122 ,"AttachmentIsTooLarge" },
            { 123 ,"UserHasNoMailbox" },
            { 124 ,"UserCannotBeAnonymous" },
            { 125 ,"UserPrincipalCouldNotBeFound" },
            { 126 ,"UserDisabledForSync" },
            { 127 ,"UserOnNewMailboxCannotSync" },
            { 128 ,"UserOnLegacyMailboxCannotSync" },
            { 129 ,"DeviceIsBlockedForThisUser" },
            { 130 ,"AccessDenied" },
            { 131 ,"AccountDisabled" },
            { 132 ,"SyncStateNotFound" },
            { 133 ,"SyncStateLocked" },
            { 134 ,"SyncStateCorrupt" },
            { 135 ,"SyncStateAlreadyExists" },
            { 136 ,"SyncStateVersionInvalid" },
            { 137 ,"CommandNotSupported" },
            { 138 ,"VersionNotSupported" },
            { 139 ,"DeviceNotFullyProvisionable" },
            { 140 ,"RemoteWipeRequested" },
            { 141 ,"LegacyDeviceOnStrictPolicy" },
            { 142 ,"DeviceNotProvisioned" },
            { 143 ,"PolicyRefresh" },
            { 144 ,"InvalidPolicyKey" },
            { 145 ,"ExternallyManagedDevicesNotAllowed" },
            { 146 ,"NoRecurrenceInCalendar" },
            { 147 ,"UnexpectedItemClass" },
            { 148 ,"RemoteServerHasNoSSL" },
            { 149 ,"InvalidStoredRequest" },
            { 150 ,"ItemNotFound" },
            { 151 ,"TooManyFolders" },
            { 152 ,"NoFoldersFound" },
            { 153 ,"ItemsLostAfterMove" },
            { 154 ,"FailureInMoveOperation" },
            { 155 ,"MoveCommandDisallowedForNonPersistentMoveAction" },
            { 156 ,"MoveCommandInvalidDestinationFolder" },
            { 160 ,"AvailabilityTooManyRecipients" },
            { 161 ,"AvailabilityDLLimitReached" },
            { 162 ,"AvailabilityTransientFailure" },
            { 163 ,"AvailabilityFailure" },
            { 164 ,"BodyPartPreferenceTypeNotSupported" },
            { 165 ,"DeviceInformationRequired" },
            { 166 ,"InvalidAccountId" },
            { 167 ,"AccountSendDisabled" },
            { 168 ,"IRM_FeatureDisabled" },
            { 169 ,"IRM_TransientError" },
            { 170 ,"IRM_PermanentError" },
            { 171 ,"IRM_InvalidTemplateID" },
            { 172 ,"IRM_OperationNotPermitted" },
            { 173 ,"NoPicture" },
            { 174 ,"PictureTooLarge" },
            { 175 ,"PictureLimitReached" },
            { 176 ,"BodyPart_ConversationTooLarge" },
            { 177 ,"MaximumDevicesReached" }
        };

    }
}

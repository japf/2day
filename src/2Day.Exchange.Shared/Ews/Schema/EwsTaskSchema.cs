using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Schema
{
    public static class EwsTaskSchema
    {
        private static readonly List<EwsPropertyDefinition> definitions;

        static EwsTaskSchema()
        {
            definitions = new List<EwsPropertyDefinition>
            {
                new EwsSimplePropertyDefinition(EwsFieldUri.ItemClass),
                new EwsSimplePropertyDefinition(EwsFieldUri.Subject),
                new EwsSimplePropertyDefinition(EwsFieldUri.Importance),
                new EwsSimpleArrayPropertyDefinition(EwsFieldUri.Categories),
                new EwsSimplePropertyDefinition(EwsFieldUri.Body),
                new EwsSimplePropertyDefinition(EwsFieldUri.ParentFolderId),
                new EwsSimplePropertyDefinition(EwsFieldUri.ReminderIsSet),
                new EwsSimplePropertyDefinition(EwsFieldUri.ReminderDueBy),
                new EwsSimplePropertyDefinition(EwsFieldUri.Attachments),
                new EwsSimplePropertyDefinition(EwsFieldUri.Recurrence, "Task"),

                new EwsDistinguishedPropertyDefinition(EwsFieldUri.TaskOrdinalDate, EwsExtendedPropertyIds.TaskOrdinalDate, EwsFieldType.SystemTime),
                new EwsDistinguishedPropertyDefinition(EwsFieldUri.TaskSubOrdinalDate, EwsExtendedPropertyIds.TaskSubOrdinalDate, EwsFieldType.String),
                
                new EwsExtendedPropertyDefinition(EwsFieldUri.CommonStart, EwsPropertySet.PropertySetCommon, EwsExtendedPropertyIds.CommonStart, EwsFieldType.SystemTime),
                new EwsExtendedPropertyDefinition(EwsFieldUri.CommonEnd, EwsPropertySet.PropertySetCommon, EwsExtendedPropertyIds.CommonEnd, EwsFieldType.SystemTime),
                new EwsExtendedPropertyDefinition(EwsFieldUri.CommonEnd, EwsPropertySet.PropertySetCommon, EwsExtendedPropertyIds.CommonEnd, EwsFieldType.SystemTime),
                new EwsExtendedPropertyDefinition(EwsFieldUri.FlagRequest, EwsPropertySet.PropertySetCommon, EwsExtendedPropertyIds.FlagRequest, EwsFieldType.String),

                new EwsExtendedPropertyDefinition(EwsFieldUri.TagSubject, null, EwsExtendedPropertyIds.TagSubject, EwsFieldType.String),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TagSubjectPrefix, null, EwsExtendedPropertyIds.TagSubjectPrefix, EwsFieldType.String),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TagIconIndex, null, EwsExtendedPropertyIds.TagIconIndex, EwsFieldType.Integer),
                new EwsExtendedPropertyDefinition(EwsFieldUri.NormalizedSubject, null, EwsExtendedPropertyIds.NormalizedSubject, EwsFieldType.String),

                new EwsExtendedPropertyDefinition(EwsFieldUri.TaskTitle, EwsPropertySet.PropertySetCommon, EwsExtendedPropertyIds.TaskTitle, EwsFieldType.String),
                
                new EwsExtendedPropertyDefinition(EwsFieldUri.TaskDueDate, EwsPropertySet.PropertySetTask, EwsExtendedPropertyIds.TaskDueDate, EwsFieldType.SystemTime),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TaskStartDate, EwsPropertySet.PropertySetTask, EwsExtendedPropertyIds.TaskStartDate, EwsFieldType.SystemTime),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TaskCompletedDate, EwsPropertySet.PropertySetTask, EwsExtendedPropertyIds.TaskCompletedDate, EwsFieldType.SystemTime),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TaskComplete, EwsPropertySet.PropertySetTask, EwsExtendedPropertyIds.TaskComplete, EwsFieldType.Boolean),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TaskIsRecurring, EwsPropertySet.PropertySetTask, EwsExtendedPropertyIds.TaskIsRecurring, EwsFieldType.Boolean),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TaskIsDeadOccurence, EwsPropertySet.PropertySetTask, EwsExtendedPropertyIds.TaskIsDeadOccurrence, EwsFieldType.Boolean),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TaskPercentComplete, EwsPropertySet.PropertySetTask, EwsExtendedPropertyIds.TaskPercentComplete, EwsFieldType.Double),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TaskStatus, EwsPropertySet.PropertySetTask, EwsExtendedPropertyIds.TaskStatus, EwsFieldType.Integer),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TaskIsDeadOccurence, EwsPropertySet.PropertySetTask, EwsExtendedPropertyIds.TaskIsDeadOccurrence, EwsFieldType.Boolean),

                new EwsExtendedPropertyDefinition(EwsFieldUri.ReplyRequested, null, EwsExtendedPropertyIds.ReplyRequested, EwsFieldType.Boolean),
                new EwsExtendedPropertyDefinition(EwsFieldUri.ResponseRequested, null, EwsExtendedPropertyIds.ResponseRequested, EwsFieldType.Boolean),

                new EwsExtendedPropertyDefinition(EwsFieldUri.TagFlagStatus, null, EwsExtendedPropertyIds.TagFlagStatus, EwsFieldType.Integer),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TagFlagCompleteTime, null, EwsExtendedPropertyIds.TagFlagCompleteTime, EwsFieldType.SystemTime),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TagToDoItemFlags, null, EwsExtendedPropertyIds.TagToDoItemFlags, EwsFieldType.Integer),
                new EwsExtendedPropertyDefinition(EwsFieldUri.ReminderSet, EwsPropertySet.PropertySetCommon, EwsExtendedPropertyIds.ReminderSet, EwsFieldType.Boolean),
                new EwsExtendedPropertyDefinition(EwsFieldUri.TagFollowupIcon, null, EwsExtendedPropertyIds.TagFollowupIcon, EwsFieldType.Integer),
            };
        }

        internal static string GetAllFieldXml(EwsItemType ewsItemType)
        {
            var builder = new StringBuilder();
            foreach (var definition in definitions.Where(d => 
                d.FieldUri != EwsFieldUri.CommonEnd && 
                d.FieldUri != EwsFieldUri.CommonStart &&
                d.FieldUri != EwsFieldUri.TagSubject &&
                d.FieldUri != EwsFieldUri.TagSubjectPrefix &&
                d.FieldUri != EwsFieldUri.TagIconIndex &&
                d.FieldUri != EwsFieldUri.NormalizedSubject && 
                d.FieldUri != EwsFieldUri.FlagRequest &&
                d.FieldUri != EwsFieldUri.ReplyRequested &&
                d.FieldUri != EwsFieldUri.ResponseRequested &&
                d.FieldUri != EwsFieldUri.TagFlagStatus &&
                d.FieldUri != EwsFieldUri.TagFlagCompleteTime &&
                d.FieldUri != EwsFieldUri.TagToDoItemFlags &&
                d.FieldUri != EwsFieldUri.ReminderSet &&
                d.FieldUri != EwsFieldUri.TagFollowupIcon
                ))
            {
                // if item type is "item" (like flagged email, recurrence is not supported)
                bool skipDefinition = ewsItemType == EwsItemType.Item && definition.FieldUri == EwsFieldUri.Recurrence;
                if (!skipDefinition)
                    builder.AppendLine(definition.GetXml());
            }

            return builder.ToString();
        }

        public static string GetXmlForFolderIdentifier(EwsFolderIdentifier identifier)
        {
            const string template = "<t:{0} Id='{1}' />";
            if (identifier.IsDistinguishedFolderId)
                return string.Format(template, "DistinguishedFolderId", identifier.Id);
            else
                return string.Format(template, "FolderId", identifier.Id);
        }

        public static EwsPropertyDefinition GetPropertyDefinition(EwsFieldUri field)
        {
            var definition = definitions.FirstOrDefault(d => d.FieldUri == field);
            if (definition == null)
                throw new NotSupportedException("Definition does not exists");

            return definition;
        }

        public static EwsPropertyDefinition GetPropertyDefinition(int propertyId)
        {
            var definition = definitions.FirstOrDefault(d => d.PropertyId == propertyId);
            if (definition == null)
                throw new NotSupportedException("Definition does not exists");

            return definition;
        }

        public static string BuildSetFieldValueXml(EwsFieldUri fieldUri, bool value)
        {
            return BuildSetFieldValueXml(fieldUri, GetString(value));
        }

        public static string BuildSetFieldValueXml(EwsFieldUri fieldUri, double value)
        {
            return BuildSetFieldValueXml(fieldUri, GetString(value));
        }

        public static string BuildSetFieldValueXml(EwsFieldUri fieldUri, int value)
        {
            return BuildSetFieldValueXml(fieldUri, GetString(value));
        }

        public static string BuildSetFieldValueXml(EwsFieldUri fieldUri, DateTime value, DateTimeKind kind)
        {
            return BuildSetFieldValueXml(fieldUri, GetString(value, kind));
        }

        public static string BuildSetFieldValueXml(EwsFieldUri fieldUri, EwsTaskStatus value)
        {
            return BuildSetFieldValueXml(fieldUri, GetString(value));
        }

        public static string BuildSetFieldValueXml(EwsFieldUri fieldUri, EwsImportance value)
        {
            return BuildSetFieldValueXml(fieldUri, GetString(value));
        }

        public static string BuildSetFieldRawValueXml(EwsFieldUri fieldUri, string value, EwsAttributes attributes = null)
        {
            var definition = GetPropertyDefinition(fieldUri);

            string xml = definition.BuildSetFieldValueXml(new [] { value }, attributes);

            return xml;
        }

        public static string BuildSetFieldValueXml(EwsFieldUri fieldUri, string value, EwsAttributes attributes = null)
        {
            return BuildSetFieldValueXml(fieldUri, new[] { value }, attributes);
        }

        public static string BuildSetFieldValueXml(EwsFieldUri fieldUri, string[] values, EwsAttributes attributes = null)
        {
            var definition = GetPropertyDefinition(fieldUri);

            string xml = definition.BuildSetFieldValueXml(values.Select(v => v.ToEscapedXml()).ToArray(), attributes);

            return xml;
        }

        public static string BuildUpdateFieldValueXml(EwsFieldUri fieldUri, bool value, bool useTrueFalse = true)
        {
            return BuildUpdateFieldValueXml(fieldUri, new[] { GetString(value, useTrueFalse) });
        }

        public static string BuildUpdateFieldValueXml(EwsFieldUri fieldUri, int value)
        {
            return BuildUpdateFieldValueXml(fieldUri, new[] { GetString(value) });
        }

        public static string BuildUpdateFieldValueXml(EwsFieldUri fieldUri, double value)
        {
            return BuildUpdateFieldValueXml(fieldUri, new[] { GetString(value) });
        }

        public static string BuildUpdateFieldValueXml(EwsFieldUri fieldUri, DateTime value, DateTimeKind kind)
        {
            return BuildUpdateFieldValueXml(fieldUri, new[] { GetString(value, kind) });
        }

        public static string BuildUpdateFieldValueXml(EwsFieldUri fieldUri, EwsTaskStatus status)
        {
            return BuildUpdateFieldValueXml(fieldUri, new[] { GetString(status) });
        }

        public static string BuildUpdateFieldValueXml(EwsFieldUri fieldUri, EwsImportance value)
        {
            return BuildUpdateFieldValueXml(fieldUri, new[] { GetString(value) });
        }

        public static string BuildUpdateFieldValueXml(EwsFieldUri fieldUri, string value, EwsAttributes attributes = null)
        {
            return BuildUpdateFieldValueXml(fieldUri, new[] { value.ToEscapedXml() }, attributes);
        }

        public static string BuildUpdateFieldRawValueXml(EwsFieldUri fieldUri, string value, EwsAttributes attributes = null)
        {
            var definition = GetPropertyDefinition(fieldUri);

            return definition.BuildSetFieldXml(new []{ value }, attributes);
        }

        public static string BuildUpdateFieldValueXml(EwsFieldUri fieldUri, string[] values, EwsAttributes attributes = null)
        {
            var definition = GetPropertyDefinition(fieldUri);

            return definition.BuildSetFieldXml(values.Select(v => v.ToEscapedXml()).ToArray(), attributes);
        }

        public static string BuildClearFieldValueXml(EwsFieldUri fieldUri)
        {
            var definition = GetPropertyDefinition(fieldUri);

            return definition.BuildClearFieldXml();
        }
       
        private static string GetString(bool value, bool useTrueFalse = true)
        {
            if (useTrueFalse)
                return value ? "true" : "false";
            else
                return value ? "1" : "0";
        }

        private static string GetString(double value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static string GetString(int value)
        {
            return value.ToString("0", CultureInfo.InvariantCulture);
        }

        private static string GetString(EwsTaskStatus value)
        {
            return ((int)value).ToString();
        }

        private static string GetString(EwsImportance value)
        {
            return value.ToString();
        }

        private static string GetString(DateTime value, DateTimeKind kind)
        {
            return value.ToEwsDateTimeValue(kind);
        }
    }
}
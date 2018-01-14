using System;

namespace Chartreuse.Today.Exchange.Ews.Schema
{
    internal class EwsExtendedPropertyDefinition : EwsDistinguishedPropertyDefinition
    {
        protected string PropertySetId { get; private set; }

        internal EwsExtendedPropertyDefinition(EwsFieldUri fieldUri, string propertySetId, int propertyId, EwsFieldType fieldType)
            : base(fieldUri, propertyId, fieldType)
        {
            this.PropertySetId = propertySetId;
        }

        public override string GetXml()
        {
            string xml;
            if (!string.IsNullOrEmpty(this.PropertySetId))
            {
                xml = string.Format(
                    "<t:ExtendedFieldURI PropertySetId='{0}' PropertyId='{1}' PropertyType='{2}' />",
                    this.PropertySetId,
                    this.PropertyId.ToString("D"),
                    this.PropertyType);
            }
            else
            {
                xml = string.Format(
                    "<t:ExtendedFieldURI PropertyTag='{0}' PropertyType='{1}' />",
                    this.PropertyId.ToString("D"),
                    this.PropertyType);
            }

#if DEBUG
            xml = "<!--" + this.FieldUri + "-->" + xml;
#endif

            return xml;
        }

        public override string BuildSetFieldXml(string[] values, EwsAttributes attributes)
        {
            if (values.Length == 0 || values.Length > 1)
                throw new ArgumentException("Only single value are valid");

            const string template =
                "<t:SetItemField>" +
                "   {0}" +
                "   <t:Item{1}>" +
                "       <t:ExtendedProperty>" +
                "           {0}" +
                "           <t:Value>{2}</t:Value>" +
                "       </t:ExtendedProperty>" +
                "   </t:Item>" +
                "</t:SetItemField>";

            string xmlDefinition = this.GetXml();
            string xml = string.Format(
                template,
                xmlDefinition,
                attributes,
                values[0]);

            return xml;
        }

        public override string BuildClearFieldXml()
        {
            const string template =
                "<t:DeleteItemField>" +
                "   {0}" +
                "</t:DeleteItemField>";

            string xml = string.Format(
                template,
                this.GetXml());

            return xml;
        }

        public override string BuildSetFieldValueXml(string[] value, EwsAttributes attributes)
        {
            string innerXml = this.GetXml();

            return string.Format("<t:ExtendedProperty>{0}   <t:Value>{1}</t:Value>" + "</t:ExtendedProperty>", innerXml, value[0]);
        }
    }
}
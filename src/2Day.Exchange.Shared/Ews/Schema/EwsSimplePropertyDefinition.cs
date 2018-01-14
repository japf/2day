using System;

namespace Chartreuse.Today.Exchange.Ews.Schema
{
    internal class EwsSimplePropertyDefinition : EwsPropertyDefinition
    {
        private readonly string type;

        public EwsSimplePropertyDefinition(EwsFieldUri fieldUri, string type = "Item") : base(fieldUri)
        {
            this.type = type;
        }

        public override string GetXml()
        {
            return string.Format("<t:FieldURI FieldURI='{0}:{1}' />", this.type.ToLowerInvariant(), this.FieldUri);
        }

        public virtual string GetFieldValueXml(string[] values, EwsAttributes attributes)
        {
            if (values.Length == 0 || values.Length > 1)
                throw new ArgumentException("Only single value are valid");

            const string template = "   <t:{0}>" +
                                    "       <t:{1}{2}>{3}</t:{1}>" +
                                    "   </t:{0}>";

            string xml = string.Format(
                template,
                this.type,
                this.FieldUri,
                attributes,
                values[0]);

            return xml;
        }

        public override string BuildSetFieldXml(string[] values, EwsAttributes attributes)
        {
            const string template =
                "<t:SetItemField>" +
                "   {0}" +
                "   {1}" +
                "</t:SetItemField>";
         
            string xmlDefinition = this.GetXml();
            string xml = string.Format(
                template,
                xmlDefinition,
                this.GetFieldValueXml(values, attributes));

            return xml;
        }

        public override string BuildSetFieldValueXml(string[] value, EwsAttributes attributes)
        {
            if (attributes != null && attributes.Count > 0)
                return string.Format("<t:{0} {1}>{2}</t:{0}>", this.FieldUri, attributes, value[0]);
            else
                return string.Format("<t:{0}>{1}</t:{0}>", this.FieldUri, value[0]);
        }

        public override string BuildClearFieldXml()
        {
            const string template =
                "<t:DeleteItemField>" +
                "   <t:FieldURI FieldURI='{0}:{1}'></t:FieldURI>" +
                "</t:DeleteItemField>";

            string xml = string.Format(
                template,
                this.type.ToLowerInvariant(),
                this.FieldUri);

            return xml;
        }
    }
}
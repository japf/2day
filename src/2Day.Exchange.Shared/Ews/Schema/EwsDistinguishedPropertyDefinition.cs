namespace Chartreuse.Today.Exchange.Ews.Schema
{
    internal class EwsDistinguishedPropertyDefinition : EwsSimplePropertyDefinition
    {
        private readonly int propertyId;

        public override int PropertyId
        {
            get { return this.propertyId; }
        }

        protected EwsFieldType PropertyType { get; private set; }

        internal EwsDistinguishedPropertyDefinition(EwsFieldUri fieldUri, int propertyId, EwsFieldType fieldType)
            : base(fieldUri)
        {
            this.propertyId = propertyId;
            this.PropertyType = fieldType;
        }

        public override string GetXml()
        {
            return string.Format(
                "<t:ExtendedFieldURI DistinguishedPropertySetId='Common' PropertyId='{0}' PropertyType='{1}' />", 
                this.PropertyId.ToString("D"),
                this.PropertyType);
        }

        public override string BuildSetFieldValueXml(string[] value, EwsAttributes attributes)
        {
            string xml = this.GetXml();
            return "<t:ExtendedProperty>" +
                        xml +
                    "   <t:Value>" + value[0] + "</t:Value>" +
                    "</t:ExtendedProperty>";
        }
    }
}
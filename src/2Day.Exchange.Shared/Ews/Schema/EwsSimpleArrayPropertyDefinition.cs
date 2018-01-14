using System.Text;

namespace Chartreuse.Today.Exchange.Ews.Schema
{
    internal class EwsSimpleArrayPropertyDefinition : EwsSimplePropertyDefinition
    {
        public EwsSimpleArrayPropertyDefinition(EwsFieldUri fieldUri) : base(fieldUri)
        {
        }

        public override string GetFieldValueXml(string[] values, EwsAttributes attributes)
        {
            const string template =
                "   <t:Item>" +
                "       <t:{0}{1}>" +
                "           {2}" +
                "       </t:{0}>" +
                "   </t:Item>";

            var builder = new StringBuilder();
            foreach (string value in values)
                builder.AppendLine(string.Format("<t:String>{0}</t:String>", value));

            string xml = string.Format(
                template,
                this.FieldUri,
                attributes,
                builder);

            return xml;
        }

        public override string BuildSetFieldValueXml(string[] values, EwsAttributes attributes)
        {
            const string template = "<t:{0}>{1}</t:{0}>";
            var builder = new StringBuilder();
            foreach (string value in values)
            {
                builder.AppendLine(string.Format("<t:String>{0}</t:String>\n", value));
            }

            return string.Format(template, this.FieldUri, builder);
        }
    }
}
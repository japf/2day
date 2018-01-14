using System.Collections.Generic;
using System.Text;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Shared.Commands;

namespace Chartreuse.Today.Exchange.Ews
{
    public abstract class EwsRequestParameterBuilderBase : IRequestParameterBuilder
    {
        private static readonly Dictionary<string, string> emptyDictionary = new Dictionary<string, string>(); 

        public string BuildXml(string command)
        {
            this.EnsureCanExecute();

            var attributes = new StringBuilder();
            foreach (var kvp in this.GetCommandAttributes())
                attributes.AppendFormat(" {0}=" + "'{1}'", kvp.Key, kvp.Value);

            string beginTag =
                string.Format(
                    "<{0} xmlns='http://schemas.microsoft.com/exchange/services/2006/messages' xmlns:m='http://schemas.microsoft.com/exchange/services/2006/messages' xmlns:t='http://schemas.microsoft.com/exchange/services/2006/types' {1}>",
                    command, attributes.ToString());
            string endTag = string.Format("</{0}>", command);

            string innerXml = this.BuildXmlCore();

            string xml = string.Format("{0}{1}{2}", beginTag, innerXml, endTag);

            string xmlValidationErrorMessage;
            if (!EwsXmlHelper.IsValidXml(xml, out xmlValidationErrorMessage))
                throw new CommandRequestXmlException(string.Format("Invalid xml content: {0}", xmlValidationErrorMessage));

            return xml;
        }

        protected abstract void EnsureCanExecute();

        protected abstract string BuildXmlCore();

        protected virtual Dictionary<string, string> GetCommandAttributes()
        {
            return emptyDictionary;
        }
    }
}

using System.IO;
using System.Net;
using System.Xml.Linq;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Exchange.Ews;
using Chartreuse.Today.Exchange.Ews.Exceptions;
using Chartreuse.Today.Exchange.Shared;

namespace Chartreuse.Today.Exchange.ActiveSync
{
    public abstract class ASResponseParserBase : IResponseParser
    {
        public virtual bool RequireValidXml
        {
            get { return true; }
        }

        public void ParseResponse(string commandName, WebRequestResponse response)
        {
            if (response.Response.StatusCode == HttpStatusCode.Unauthorized)
                throw new CommandAuthorizationException(string.Format("Authorization failed: {0}", response.Response.ReasonPhrase));

            string xml = response.ResponseBody;

            XDocument xdoc = null;

            string xmlValidationErrorMessage;
            if (!EwsXmlHelper.IsValidXml(xml, out xmlValidationErrorMessage))
            {
                if (this.RequireValidXml)
                    throw new CommandResponseXmlException(string.Format("Invalid xml content: {0}", xmlValidationErrorMessage));
            }
            else
            {
                xdoc = XDocument.Load(new StringReader(xml));
            }

            this.ParseResponseCore(commandName, xdoc, response);
        }

        protected abstract void ParseResponseCore(string commandName, XDocument document, WebRequestResponse response);
    }
}

using System;
using Chartreuse.Today.Exchange.Shared.Commands;

namespace Chartreuse.Today.Exchange.Ews.AutoDiscover
{
    public class AutoDiscoverParameter : IRequestParameterBuilder
    {
        private readonly string email;

        public AutoDiscoverParameter(string email)
        {
            if (email == null)
                throw new ArgumentNullException("email");

            this.email = email;
        }

        public string BuildXml(string command)
        {
            const string template = 
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<Autodiscover xmlns=\"http://schemas.microsoft.com/exchange/autodiscover/outlook/requestschema/2006\">" +
                "    <Request>" +
                "        <EMailAddress>{0}</EMailAddress>" +
                "        <AcceptableResponseSchema>http://schemas.microsoft.com/exchange/autodiscover/outlook/responseschema/2006a</AcceptableResponseSchema>" +
                "    </Request>" +
                "</Autodiscover>";

            return string.Format(template, this.email);
        }
    }
}
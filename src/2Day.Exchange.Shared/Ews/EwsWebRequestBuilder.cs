using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Net;

namespace Chartreuse.Today.Exchange.Ews
{
    internal class EwsWebRequestBuilder : WebRequestBuilder
    {
        protected override byte[] PrepareRequestBody(string content)
        {
            const string soapEnveloppe = "<soap:Envelope " +
                                         "    xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' " +
                                         "    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' " +
                                         "    xmlns:xsd='http://www.w3.org/XMLSchema' " +
                                         "    xmlns:t='http://schemas.microsoft.com/exchange/services/2006/types'>" +
                                         "    <soap:Body>" +
                                         "        {0}" +
                                         "    </soap:Body>" +
                                         "</soap:Envelope>";

            string enveloppe = string.Format(soapEnveloppe, content);

            return Encoding.UTF8.GetBytes(enveloppe);
        }

        protected override async Task<string> ReadResponseAsync(HttpResponseMessage response)
        {
            string result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}

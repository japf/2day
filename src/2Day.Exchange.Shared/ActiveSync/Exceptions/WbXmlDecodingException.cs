using System;

namespace Chartreuse.Today.Exchange.ActiveSync.Exceptions
{
    public class WbXmlDecodingException : Exception
    {
        public string XmlString { get; private set; }

        internal WbXmlDecodingException(string xmlString,Exception innerException)
            :base("Error during decoding WbXml bytes",innerException)
        {
            this.XmlString = xmlString;
        }
    }
}

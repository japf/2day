using System;

namespace Chartreuse.Today.Exchange.ActiveSync.Exceptions
{
    public class WbXmlEncodingException : Exception
    {
        public byte[] WbxmlBytes { get; private set; }

        internal WbXmlEncodingException(byte[] wbxml, Exception innerException)
            :base("Error during encoding WbXml bytes",innerException)
        {
            this.WbxmlBytes = wbxml;
        }
    }
}

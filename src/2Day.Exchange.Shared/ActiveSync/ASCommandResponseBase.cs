using System;
using System.Net;
using Chartreuse.Today.Exchange.ActiveSync.Encoder;

namespace Chartreuse.Today.Exchange.ActiveSync
{
    class ASCommandResponseBase
    {
        private byte[] wbxmlBytes = null;
        private string xmlString = null;
        private HttpStatusCode httpStatus = HttpStatusCode.OK;

        public byte[] WbxmlBytes
        {
            get
            {
                return this.wbxmlBytes;
            }
        }

        public string XmlString
        {
            get
            {
                return this.xmlString;
            }
        }

        public HttpStatusCode HttpStatus
        {
            get
            {
                return this.httpStatus;
            }
        }

        public ASCommandResponseBase(HttpWebResponse httpResponse)
        {
            
        }

        private string DecodeWBXML(byte[] wbxml)
        {
            try
            {
                ASWBXML decoder = new ASWBXML();
                decoder.LoadBytes(wbxml);
                return decoder.GetXml();
            }
            catch (Exception)
            {
                //VSError.ReportException(ex);
                return "";
            }
        }
    }
}

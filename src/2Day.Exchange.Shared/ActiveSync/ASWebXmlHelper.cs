using System;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Exchange.ActiveSync.Encoder;

namespace Chartreuse.Today.Exchange.ActiveSync
{
    internal static class ASWebXmlHelper
    {
        internal static string Decode(byte[] wbxml)
        {
            if (wbxml == null || wbxml.Length == 0)
                return string.Empty;
            try
            {
                ASWBXML decoder = new ASWBXML();
                decoder.LoadBytes(wbxml);
                return decoder.GetXml();
            }
            catch (Exception ex)
            {
                LogService.Log("ASXML", "Failed to decode content: " + ex.Message);
                throw;
            }
        }

        internal static byte[] Encode(string stringXML)
        {
            try
            {
                ASWBXML encoder = new ASWBXML();
                encoder.LoadXml(stringXML);
                return encoder.GetBytes();
            }
            catch (Exception ex)
            {
                LogService.Log("ASXML", "Failed to encode content: " + ex.Message);
                throw;
            }
        }
    }
}

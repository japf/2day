using System.Diagnostics;
using System.Xml.Linq;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews
{
    [DebuggerDisplay("Class: {Class} Code: {Code} Message: {Message} Content: {Content}")]
    public class EwsResponseMessage
    {
        public EwsResponseClass Class { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public XElement Content { get; set; }
    }
}

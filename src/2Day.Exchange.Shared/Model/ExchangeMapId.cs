
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Chartreuse.Today.Exchange.Model
{
    [DataContract]
    [DebuggerDisplay("Local: LocalId, Remote: ExchangeId")]
    public class ExchangeMapId
    {
        [DataMember]
        public int LocalId { get; set; }

        [DataMember]
        public string ExchangeId { get; set; }
    }
}

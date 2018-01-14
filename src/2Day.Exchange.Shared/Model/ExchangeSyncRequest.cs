using System.Runtime.Serialization;

namespace Chartreuse.Today.Exchange.Model
{
    [DataContract]
    public class ExchangeSyncRequest
    {
        [DataMember]
        public ExchangeConnectionInfo ConnectionInfo { get; set; }
        
        [DataMember]
        public ExchangeChangeSet ChangeSet { get; set; }
        
        [DataMember]
        public string SyncState { get; set; }
    }
}
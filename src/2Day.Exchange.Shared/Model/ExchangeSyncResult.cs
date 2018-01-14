using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Chartreuse.Today.Exchange.Model
{
    [DataContract]
    public class ExchangeSyncResult : ExchangeResultBase
	{
        [DataMember]
	    public ExchangeChangeSet ChangeSet
	    {
	        get; 
            set;
	    }

        [DataMember]
        public int TaskAddedCount
        {
            get;
            set;
        }

        [DataMember]
        public int TaskEditedCount
        {
            get;
            set;
        }

        [DataMember]
        public int TaskDeletedCount
        {
            get;
            set;
        }

        [DataMember]
        public string SyncState
        {
            get;
            set;
        }

        [DataMember]
        public string FolderName
        {
            get;
            set;
        }

        [DataMember]
        public List<ExchangeMapId> MapId
        {
            get;
            set;
        }

        public void AddMap(int localId, string exchangeId)
        {
            this.MapId.Add(new ExchangeMapId { LocalId = localId, ExchangeId = exchangeId });
        }

        public ExchangeSyncResult()
        {
            this.ChangeSet = new ExchangeChangeSet();
            this.MapId = new List<ExchangeMapId>();
        }
	}
}

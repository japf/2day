using System;
using System.Runtime.Serialization;

namespace Chartreuse.Today.Exchange.Model
{
	[DataContract]
	public class ServerDeletedAsset
	{
		[DataMember]
		public DateTime DeletionDate
		{
			get;
			set;
		}

		[DataMember]
		public string Id
		{
			get;
			set;
		}

        public ServerDeletedAsset(string id = null)
        {
            this.Id = id;
            this.DeletionDate = DateTime.Now;
        }
	}
}

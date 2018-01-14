using System;
using System.Runtime.Serialization;

namespace Chartreuse.Today.Exchange.Model
{
	[DataContract]
	public class ExchangeAuthorizationResult : ServiceOperationResult
	{
	    [DataMember]
		public Uri ServerUri
		{
			get;
			set;
		}

		[DataMember]
		public ExchangeAuthorizationStatus AuthorizationStatus
		{
			get;
			set;
		}

        [DataMember]
	    public string Status
	    {
	        get; 
            set; 
        }
	}
}

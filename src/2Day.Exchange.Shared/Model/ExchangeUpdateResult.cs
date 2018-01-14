using System;
using System.Runtime.Serialization;

namespace Chartreuse.Today.Exchange.Model
{
	[DataContract]
	public class ExchangeUpdateResult : ExchangeResultBase
	{
		[DataMember]
		public Uri ResolvedUrl
		{
			get;
			set;
		}
	}
}

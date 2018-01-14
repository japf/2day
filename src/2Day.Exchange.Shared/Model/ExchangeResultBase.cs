using System.Runtime.Serialization;

namespace Chartreuse.Today.Exchange.Model
{
    [DataContract]
    public class ExchangeResultBase
    {
        [DataMember]
        public ExchangeAuthorizationResult AuthorizationResult
        {
            get;
            set;
        }

        [DataMember]
        public ServiceOperationResult OperationResult
        {
            get;
            set;
        }

        public ExchangeResultBase()
		{
			this.AuthorizationResult = new ExchangeAuthorizationResult();
			this.OperationResult = new ServiceOperationResult();
		}
    }
}

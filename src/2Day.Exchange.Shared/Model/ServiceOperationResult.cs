using System.Runtime.Serialization;

namespace Chartreuse.Today.Exchange.Model
{
	[DataContract]
	public class ServiceOperationResult
	{
		public ServiceOperationResult()
		{
			this.IsOperationSuccess = false;
		}

		[DataMember]
		public string ErrorMessage
		{
			get;
			set;
		}

		[DataMember]
		public bool IsOperationSuccess
		{
			get;
			set;
		}
	}
}

using System;
using System.Runtime.Serialization;

namespace Chartreuse.Today.Exchange.Model
{
	[DataContract]
	[KnownType(typeof(ExchangeConnectionInfo))]
	public class ExchangeConnectionInfo
	{
		[DataMember]
		public Uri ServerUri
		{
			get;
			set;
		}

		[DataMember]
		public string Username
		{
			get;
			set;
		}

        [DataMember]
        public byte[] EncryptedPassword
		{
			get;
			set;
		}

        [DataMember]
        public string Password
        {
            get;
            set;
        }

		[DataMember]
		public string Email
		{
			get;
			set;
		}

		[DataMember]
		public string Domain
		{
			get;
			set;
		}

		[DataMember]
		public bool RequireSSL
		{
			get;
			set;
		}

		[DataMember]
		public string TimeZoneStandardName
		{
			get;
			set;
		}

        [DataMember]
        public double UtcOffset
        {
            get;
            set;
        }

	    [DataMember]
        public ExchangeServerVersion Version
	    {
	        get; 
            set;
	    }

        public string DeviceId
        {
            get;
            set;
        }

        public uint PolicyKey
        {
            get;
            set;
        }

        public bool IsBackgroundSync
        {
            get;
            set;
        }

	    public string Source
	    {
	        get; 
            set; 
        }

        public bool SyncFlaggedItems
        {
            get;
            set;
        }
    }
}

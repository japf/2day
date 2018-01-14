using System;

namespace Chartreuse.Today.Exchange.ActiveSync
{
    public class ASRequestSettings
    {
        public string ProtocolVersion { get; private set; }

        public bool UseSSL { get; private set; }

        public string HostName { get; set; }

        public string Login { get; private set; }

        public string Password { get; private set; }

        public string DeviceId { get; private set; }

        public string DeviceType { get; private set; }

        public uint PolicyKey { get; set; }

        public ASRequestSettings(string hostname, string login, string password, string protocolVersion, bool useSSL, string deviceId, string deviceType, uint policyKey)
        {
            if (string.IsNullOrEmpty(login))
                throw new ArgumentNullException(nameof(login));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(protocolVersion))
                throw new ArgumentNullException(nameof(protocolVersion));
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));
            if (string.IsNullOrEmpty(deviceType))
                throw new ArgumentNullException(nameof(deviceType));

            this.HostName = hostname;
            this.Login = login;
            this.Password = password;
            this.ProtocolVersion = protocolVersion;
            this.UseSSL = useSSL;
            this.DeviceId = deviceId;
            this.DeviceType = deviceType;
            this.PolicyKey = policyKey;
        }
    }
}

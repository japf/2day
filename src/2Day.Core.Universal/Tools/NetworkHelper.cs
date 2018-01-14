using System.Net.NetworkInformation;
using Windows.Networking.Connectivity;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.Core.Universal.Tools
{
    public static class NetworkHelper
    {
        public static bool IsNetworkAvailable
        {
            get { return NetworkInterface.GetIsNetworkAvailable(); }
        }

        public static NetworkType GetNetworkStatus()
        {
            if (!IsNetworkAvailable)
                return NetworkType.None;

            var profile = NetworkInformation.GetInternetConnectionProfile();

            if (profile.IsWlanConnectionProfile)
                return NetworkType.Wifi;
            else if (profile.IsWwanConnectionProfile)
                return NetworkType.Cellular;
            
            return NetworkType.None;
        }
    }
}

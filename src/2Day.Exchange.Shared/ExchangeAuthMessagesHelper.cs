using Chartreuse.Today.Exchange.Resources;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange
{
    public static class ExchangeAuthMessagesHelper
    {
        public static string ToReadableString(this ExchangeAuthorizationStatus authorizationStatus)
        {
            switch (authorizationStatus)
            {
                case ExchangeAuthorizationStatus.Unkown:
                    return ExchangeResources.Exchange_AuthUnkown;
                case ExchangeAuthorizationStatus.OK:
                    return ExchangeResources.Exchange_AuthOk;
                case ExchangeAuthorizationStatus.UserCredentialsInvalid:
                    return ExchangeResources.Exchange_AuthInvalidCredential;
                case ExchangeAuthorizationStatus.AutodiscoveryServiceNotFound:
                    return ExchangeResources.Exchange_AuthAutoDiscoveryNotFound;
                case ExchangeAuthorizationStatus.ServerUrlNotFound:
                    return ExchangeResources.Exchange_AuthServerUrlNotFound;
                case ExchangeAuthorizationStatus.TrustFailure:
                    return ExchangeResources.Exchange_AuthTrustFailure;
                case ExchangeAuthorizationStatus.ServerAccessForbidden:
                    return ExchangeResources.Exchange_AuthServerAccessForbiden;
                case ExchangeAuthorizationStatus.HostNotFound:
                    return ExchangeResources.Exchange_AuthServerUrlNotFound;
                case ExchangeAuthorizationStatus.ServerMethodNotAllowed:
                    return ExchangeResources.Exchange_AuthServerMethodNotAllowed;
                default:
                    return string.Empty;
            }
        }
    }
}

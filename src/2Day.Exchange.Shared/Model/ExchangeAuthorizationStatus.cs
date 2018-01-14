namespace Chartreuse.Today.Exchange.Model
{
    public enum ExchangeAuthorizationStatus
    { 
        Unkown,
        OK,
        UserCredentialsInvalid,
        AutodiscoveryServiceNotFound,
        ServerUrlNotFound,
        TrustFailure,
        ServerAccessForbidden,
        HostNotFound,
        ServerMethodNotAllowed
    }
}
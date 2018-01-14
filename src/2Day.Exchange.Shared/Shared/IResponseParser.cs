using Chartreuse.Today.Core.Shared.Net;

namespace Chartreuse.Today.Exchange.Shared
{
    public interface IResponseParser
    {
        void ParseResponse(string commandName, WebRequestResponse response);
    }
}

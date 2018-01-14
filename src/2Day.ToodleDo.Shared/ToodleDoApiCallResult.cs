using System.Xml.Linq;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.ToodleDo
{
    public struct ToodleDoApiCallResult
    {
        public string Error { get; private set; }
        public int ErrorId { get; private set; }
        public bool HasError { get; private set; }
        public XDocument Document { get; private set; }

        public ToodleDoApiCallResult(XDocument xDocument)
            : this()
        {
            this.Document = xDocument;
        }

        public ToodleDoApiCallResult(string api, string error, int errorId = -1)
            : this()
        {
            TrackingManagerHelper.Trace($"ToodleDoService - Error while calling api: {api} error: {error} id: {errorId}");

            this.Error = error;
            this.ErrorId = errorId;
            this.HasError = true;
        }
    }
}
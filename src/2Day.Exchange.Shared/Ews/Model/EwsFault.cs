namespace Chartreuse.Today.Exchange.Ews.Model
{
    public class EwsFault
    {
        public string FaultCode { get; set; }
        public string FaultString { get; set; }

        public string ResponseCode { get; set; }
        public string Message { get; set; }
        public int LineNumber { get; set; }
        public int LinePosition { get; set; }
        public string Violation { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                    "FaultCode: {0}, FaultString: {1}, ResponseCode: {2}, Message: {3}, LineNumber: {4}, LinePosition: {5}, Violation: {6}",
                    this.FaultCode ?? "?",
                    this.FaultString ?? "?",
                    this.ResponseCode ?? "?",
                    this.Message ?? "?",
                    this.LineNumber,
                    this.LinePosition,
                    this.Violation ?? "?");

        }
    }
}
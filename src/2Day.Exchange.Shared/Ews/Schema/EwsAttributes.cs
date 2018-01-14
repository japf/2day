using System.Collections.Generic;
using System.Text;

namespace Chartreuse.Today.Exchange.Ews.Schema
{
    public class EwsAttributes : Dictionary<string, string>
    {
        private const string Whitespace = " ";

        public override string ToString()
        {
            if (this.Count == 0)
            {
                return string.Empty;
            }
            else
            {
                var builder = new StringBuilder(Whitespace);
                foreach (var kvp in this)
                {
                    builder.AppendFormat("{0}='{1}'", kvp.Key, kvp.Value);
                }
                return builder.ToString();
            }
        }
    }
}

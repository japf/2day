using System.Collections.Generic;

namespace Chartreuse.Today.Exchange.ActiveSync
{
    class ASWBXMLCodePage
    {
        private string strNamespace = "";
        private string strXmlns = "";
        private Dictionary<byte, string> tokenLookup = new Dictionary<byte, string>();
        private Dictionary<string, byte> tagLookup = new Dictionary<string, byte>();

        public string Namespace
        {
            get
            {
                return this.strNamespace;
            }
            set
            {
                this.strNamespace = value;
            }
        }

        public string Xmlns
        {
            get
            {
                return this.strXmlns;
            }
            set
            {
                this.strXmlns = value;
            }
        }

        public void AddToken(byte token, string tag)
        {
            this.tokenLookup.Add(token, tag);
            this.tagLookup.Add(tag, token);
        }

        public byte GetToken(string tag)
        {
            if (this.tagLookup.ContainsKey(tag))
                return this.tagLookup[tag];

            return 0xFF;
        }

        public string GetTag(byte token)
        {
            if (this.tokenLookup.ContainsKey(token))
                return this.tokenLookup[token];

            return null;
        }
    }
}

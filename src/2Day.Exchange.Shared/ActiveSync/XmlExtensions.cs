using System.Xml;
using System.Xml.Linq;

namespace Chartreuse.Today.Exchange.ActiveSync
{
    public static class XmlExtensions
    {

        public static string GetPrefix(this XNode node)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                XElement elt = (XElement) node;
                return elt.Name.NamespaceName;
            }
            return string.Empty;
        }

        public static string GetPrefix(this XAttribute attribute)
        {
            return attribute.Name.NamespaceName;
        }        
    }
}

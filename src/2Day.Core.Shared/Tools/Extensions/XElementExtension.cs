using System.Xml.Linq;

namespace Chartreuse.Today.Core.Shared.Tools.Extensions
{
    public static class XElementExtension
    {
        public static string GetAttribute(this XElement xElement, string key)
        {
            var attribute = xElement.Attribute(key);
            if (attribute != null && !string.IsNullOrEmpty(attribute.Value))
            {
                return attribute.Value;
            }

            return string.Empty;
        }

        public static string GetElement(this XElement xElement, string key)
        {
            var element = xElement.Element(key);
            if (element != null && !string.IsNullOrEmpty(element.Value))
            {
                return element.Value;
            }

            return string.Empty;
        }

        public static string GetElement(this XDocument xDocument, string key)
        {
            var element = xDocument.Element(key);
            if (element != null && !string.IsNullOrEmpty(element.Value))
            {
                return element.Value;
            }

            return string.Empty;
        }
    }
}

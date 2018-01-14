using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Chartreuse.Today.Exchange.Ews
{
    public static class EwsXmlHelper
    {
        private const string whitespace = " ";
        private const string comma = ",";

        private static readonly CultureInfo dateTimeCultureInfo = new CultureInfo("en-us");

        public static T ReadStringAs<T>(string content)
        {
            Type type = typeof (T);

            if (type == typeof(string))
            {
                return (T)(object)WebUtility.HtmlDecode(content);
            }
            if (type == typeof(int))
            {
                return (T)(object)int.Parse(content);
            }
            if (type == typeof(bool))
            {
                return (T) (object) bool.Parse(content);
            }
            if (type == typeof(double))
            {
                return (T) (object) double.Parse(content, CultureInfo.InvariantCulture);
            }
            if (type == typeof(DateTime))
            {
                DateTime result;
                if (DateTime.TryParse(content, dateTimeCultureInfo, DateTimeStyles.RoundtripKind, out result))
                {
                    if (result.Kind != DateTimeKind.Local)
                        result = new DateTime(result.Year, result.Month, result.Day, result.Hour, result.Minute, result.Second, DateTimeKind.Local);

                    return (T)(object)result;
                }

                return default(T);
            }

            if (type.GetTypeInfo().IsEnum)
            {
                // handle flags
                content = content.Replace(whitespace, comma);
                return (T)Enum.Parse(type, content);
            }

            throw new NotSupportedException("This type is not supported");
        }

        public static int XGetChildMonthNamesIndex(this XElement parent, string node)
        {
            var month = parent.XGetChildValue<string>(node, true);
            if (!string.IsNullOrEmpty(month))
                return CultureInfo.InvariantCulture.DateTimeFormat.MonthNames.ToList().IndexOf(month) + 1;

            return 0;
        }

        public static XElement TryGetNode(this XDocument xdoc, string name)
        {
            return xdoc.Root.Element(name.WithNamespace());
        }

        public static XElement TryGetNode(this XElement xElement, string name, string ns)
        {
            if (string.IsNullOrWhiteSpace(ns))
            {
                return xElement.Element(name.WithNamespace());
            }
            else
            {
                if (!namespaces.ContainsKey(ns))
                    throw new NotSupportedException("No associated namespace");

                return xElement.Element(namespaces[ns] + name);
            }
        }

        public static T XGetAttributeValue<T>(this XElement xElement, string attribute)
        {
            string value = xElement.Attribute(attribute).Value;

            return ReadStringAs<T>(value);
        }

        public static T XGetChildNodeAttributeValue<T>(this XElement xElement, string node, string attribute)
        {
            var xAttribute = xElement.Element(GetNamespace(node) + node).Attribute(attribute);
            if (xAttribute != null && xAttribute.Value != null)
                return ReadStringAs<T>(xAttribute.Value);

            return default(T);
        }

        public static T XGetChildNodeAttributeValue<T>(this XElement xElement, string node, string attribute, string ns)
        {
            if (!namespaces.ContainsKey(ns))
                throw new NotSupportedException("No associated namespace");

            string value = xElement.Element(namespaces[ns] + node).Attribute(attribute).Value;

            return ReadStringAs<T>(value);
        }

        public static string XGetChildInnerValue(this XElement xElement, string node, bool allowNull)
        {
            XElement child = xElement.Element(GetNamespace(node) + node);
            if (child == null)
                return null;

            var reader = child.CreateReader();
            reader.MoveToContent();

            return reader.ReadInnerXml();
        }

        public static T XGetChildValue<T>(this XElement xElement, string node, bool allowNull)
        {
            XElement child = xElement.Element(GetNamespace(node) + node);

            if (child == null && allowNull)
                return default(T);

            return ReadStringAs<T>(child.Value);
        }

        public static T XGetChildValue<T>(this XElement xElement, string node, string ns)
        {
            if (!namespaces.ContainsKey(ns))
                throw new NotSupportedException("No associated namespace");

            XElement child = xElement.Element(namespaces[ns] + node);

            if (child == null)
                return default(T);

            return ReadStringAs<T>(child.Value);
        }

        public static T XGetChildValue<T>(this XElement xElement, string node)
        {
            XElement child = xElement.Element(GetNamespace(node) + node);

            if (child == null)
                return default(T);

            return ReadStringAs<T>(child.Value);
        }

        public static List<XElement> XGetAllChildren(this XElement xElement, string token)
        {
            return xElement.Elements(GetNamespace(token) + token).ToList();
        }

        public static XElement XGetChild(this XElement xElement, string path, string ns = null)
        {
            var result = xElement.XGetChildrenOf(path, ns, false);
            if (result != null && result.Count == 1)
                return result[0];

            return null;
        }

        public static List<XElement> XGetChildrenOf(this XElement xElement, string path, string ns = null, bool takeChildren = true)
        {
            XElement current = xElement;
            string[] elements = path.Split('/');
            int count = elements.Length;

            for (int index = 0; index < count; index++)
            {
                var token = elements[index];

                XNamespace xNamespace = null;
                if (token.Contains(":"))
                {
                    var split = token.Split(':');
                    if (!namespaces.ContainsKey(split[0]))
                        throw new NotSupportedException("Unknown namespace");

                    xNamespace = namespaces[split[0]];
                    token = split[1];
                }
                else
                {
                    if (ns == null)
                        xNamespace = GetNamespace(token);
                    else
                        xNamespace = namespaces[ns];
                }

                current = current.Element(xNamespace + token);

                if (current == null)
                    return new List<XElement>();
            }

            if (current == null)
            {
                return new List<XElement>();
            }
            else
            {
                if (takeChildren)
                    return current.Elements().ToList();
                else
                    return new List<XElement> { current };
            }
        }

        public static XName WithNamespace(this string node)
        {
            return GetNamespace(node) + node;
        }

        private static XNamespace GetNamespace(string node)
        {
            if (!tokens.ContainsKey(node))
                throw new NotSupportedException("Unknown node: " + node);

            string key = tokens[node];

            if (!namespaces.ContainsKey(key))
                throw new NotSupportedException("No associated namespace");
            return namespaces[key];
        }

        private static Dictionary<string, XNamespace> namespaces = new Dictionary<string, XNamespace>
        {
            {"soap",    XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/") },
            {"m",       XNamespace.Get("http://schemas.microsoft.com/exchange/services/2006/messages") },
            {"t",       XNamespace.Get("http://schemas.microsoft.com/exchange/services/2006/types") },
            {"e",       XNamespace.Get("http://schemas.microsoft.com/exchange/services/2006/errors" )},
            {"ad",       XNamespace.Get("http://schemas.microsoft.com/exchange/autodiscover/responseschema/2006" )},
            {"oad",       XNamespace.Get("http://schemas.microsoft.com/exchange/autodiscover/outlook/responseschema/2006a" )},
            
            {string.Empty , XNamespace.Get(string.Empty)}
        };

        private static Dictionary<string, string> tokens = new Dictionary<string, string>
        {
            { "Envelope", "soap" },
            { "Body", "soap" },
            { "Fault", "soap" },

            { "GetFolderResponse", "m" },
            { "ResponseMessages", "m" },
            { "ResponseCode", "m" },
            { "MessageText", "m" },
            { "MoveItemResponse", "m" },
            { "DeleteItemResponse", "m" },
            { "FindItemResponse", "m" },
            { "GetItemResponse", "m" },
            { "CreateItemResponse", "m" },
            { "UpdateItemResponse", "m" },
            { "FindFolderResponse", "m" },
            { "CreateFolderResponse", "m" },
            { "DeleteFolderResponse", "m" },
            
            { "Folders", "m" },
            { "RootFolder", "m" },
            
            { "Items", "t" },
            { "TasksFolder", "t" },
            { "ItemId", "t" },
            { "FolderId", "t" },
            { "ParentFolderId", "t" },
            { "Subject", "t" },
            { "Recurrence", "t" },
            { "DisplayName", "t" },
            { "TotalCount", "t" },
            { "ChildFolderCount", "t" },
            { "UnreadCount", "t" },
            { "ReminderDueBy", "t" },
            { "ReminderIsSet", "t" },
            { "Importance", "t" },

            { "ExtendedProperty", "t" },
            { "ExtendedFieldURI", "t" },
            { "Value", "t" },

            { "DailyRecurrence", "t" },
            { "DailyRegeneration", "t" },
            { "WeeklyRecurrence", "t" },
            { "WeeklyRegeneration", "t" },
            { "AbsoluteMonthlyRecurrence", "t" },
            { "RelativeMonthlyRecurrence", "t" },
            { "MonthlyRegeneration", "t" },
            { "AbsoluteYearlyRecurrence", "t" },
            { "RelativeYearlyRecurrence", "t" },
            { "YearlyRegeneration", "t" },
            { "NoEndRecurrence", "t" },
            { "Interval", "t" },
            { "StartDate", "t" },
            { "DaysOfWeek", "t" },
            { "DayOfWeekIndex", "t" },
            { "DayOfMonth", "t" },
            { "Month", "t" },

            { "LineNumber", "t" },
            { "LinePosition", "t" },
            { "Violation", "t" },
            { "faultcode", string.Empty },
            { "faultstring", string.Empty },

            { "Response", "ad" },
            { "Account", "ad" },
            { "Protocol", "ad" },
        };

        public static bool IsValidXml(string value, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                XDocument.Load(new StringReader(value));

            }
            catch (XmlException e)
            {
                errorMessage = e.Message;
                return false;
            }

            return true;
        }
    }
}
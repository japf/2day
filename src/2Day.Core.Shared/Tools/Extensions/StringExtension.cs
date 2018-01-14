using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Shared.Tools.Extensions
{
    public static class StringExtension
    {
        private static readonly Regex htmlRegex = new Regex("<\\/?[a-z][a-z0-9]*[^<>]*>");

        // source code from: http://invokeit.wordpress.com/2011/10/06/how-to-remove-diatrics-accent-marks-in-windows-phone-7-x/
        static readonly char[] englishReplace = { 'e' };
        static readonly char[] englishAccents = { 'é' };

        static readonly char[] frenchReplace = { 'a', 'a', 'a', 'a', 'c', 'e', 'e', 'e', 'e', 'i', 'i', 'o', 'o', 'u', 'u', 'u' };
        static readonly char[] frenchAccents = { 'à', 'â', 'ä', 'æ', 'ç', 'é', 'è', 'ê', 'ë', 'î', 'ï', 'ô', 'œ', 'ù', 'û', 'ü' };

        static readonly char[] germanReplace = { 'a', 'o', 'u', 's' };
        static readonly char[] germanAccents = { 'ä', 'ö', 'ü', 'ß' };

        static readonly char[] spanishReplace = { 'a', 'e', 'i', 'o', 'u' };
        static readonly char[] spanishAccents = { 'á', 'é', 'í', 'ó', 'ú' };

        static readonly char[] italianReplace = { 'a', 'e', 'e', 'i', 'o', 'o', 'u' };
        static readonly char[] italianAccents = { 'à', 'è', 'é', 'ì', 'ò', 'ó', 'ù' };

        static readonly char[] portugueseReplace = { 'a', 'a', 'a', 'a', 'e', 'e', 'i', 'o', 'o', 'o', 'u', 'u' };
        static readonly char[] portugueseAccents = { 'ã', 'á', 'â', 'à', 'é', 'ê', 'í', 'õ', 'ó', 'ô', 'ú', 'ü' };


        public static string TryTrim(this string str)
        {
            return string.IsNullOrEmpty(str) ? str : str.Trim();
        }

        /// <summary>
        /// UriEscapeDataString can only handle 60 000 characters. This method can handle unlimited length
        /// http://stackoverflow.com/questions/6695208/uri-escapedatastring-invalid-uri-the-uri-string-is-too-long
        /// </summary>
        public static string EscapeLongDataString(this string longString)
        {
            const int limit = 32766;

            var sb = new StringBuilder();
            int loops = longString.Length / limit;
            for (var i = 0; i <= loops; i++)
            {
                sb.Append(i < loops
                    ? Uri.EscapeUriString(longString.Substring(limit * i, limit))
                    : Uri.EscapeUriString(longString.Substring(limit * i)));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Try to format a string by filling a {0} with a message. If the message is null, empty or whitespace
        /// then the source format (the '{0}') is removed.
        /// </summary>
        /// <param name="strFormat">Input string format</param>
        /// <param name="message">Message to fill</param>
        /// <returns>Transformed string</returns>
        public static string TryFormat(this string strFormat, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return strFormat.Replace("({0})", string.Empty).Replace("{0}", string.Empty);
            else
                return string.Format(strFormat, message);
        }

        public static bool IsEqualsOrEmptyTo(this string str, string other)
        {
            // ignore '\r' because RichEditBox control add them but we don't want to flag the content has changed
            if (string.IsNullOrWhiteSpace(str))
                return string.IsNullOrWhiteSpace(other);
            else if(string.IsNullOrWhiteSpace(other))
                return string.IsNullOrWhiteSpace(str);
            else
                return string.Equals(str.Replace("\r", string.Empty), other.Replace("\r", string.Empty), StringComparison.OrdinalIgnoreCase);
        }

        public static string TakeLast(this string str, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");

            if (str.Length < count)
                return str;
            else
                return "..." + str.Substring(str.Length - count);
        }

        public static string FirstLetterToLower(this string str)
        {
            if (str != null)
            {
                if (str.Length > 1)
                    return char.ToLower(str[0]) + str.Substring(1);
                else
                    return str.ToLower();
            }

            return null;
        }

        public static string FirstLetterToUpper(this string str)
        {
            if (str != null)
            {
                if (str.Length > 1)
                    return char.ToUpper(str[0]) + str.Substring(1);
                else
                    return str.ToUpper();
            }

            return null;
        }

        static readonly StringBuilder sbStripAccents = new StringBuilder();

        public static string RemoveDiacritics(this string accentedStr)
        {
            char[] replacement;
            char[] accents;

            var culture = CultureInfo.CurrentUICulture.ToString().ToLower();
            if (culture.StartsWith("en"))
            {
                replacement = englishReplace;
                accents = englishAccents;
            }
            else if (culture.StartsWith("fr"))
            {
                replacement = frenchReplace;
                accents = frenchAccents;
            }
            else if (culture.StartsWith("de"))
            {
                replacement = germanReplace;
                accents = germanAccents;
            }
            else if (culture.StartsWith("pt"))
            {
                replacement = portugueseReplace;
                accents = portugueseAccents;
            }
            else if (culture.StartsWith("it"))
            {
                replacement = italianReplace;
                accents = italianAccents;
            }
            else if (culture.StartsWith("es"))
            {
                replacement = spanishReplace;
                accents = spanishAccents;
            }
            else
            {
                return accentedStr;
            }

            if (accents != null && replacement != null && accentedStr.IndexOfAny(accents) > -1)
            {
                sbStripAccents.Length = 0;
                sbStripAccents.Append(accentedStr);
                for (int i = 0; i < accents.Length; i++)
                {
                    sbStripAccents.Replace(accents[i], replacement[i]);
                }

                return sbStripAccents.ToString();
            }
            else
                return accentedStr;
        }

        public static string AggregateString(this IEnumerable<string> input)
        {
            if (input == null || !input.Any())
                return string.Empty;
            else if (input.Count() < 2)
                return input.ElementAt(0);
            else
                return input.Aggregate((a, b) => a + ", " + b);
        }
        
        public static bool HasHtml(this string htmlText)
        {
            if (string.IsNullOrWhiteSpace(htmlText))
                return false;

            return htmlRegex.IsMatch(htmlText);
        }

        /// <summary>
        /// Remove all HTML tags from a string
        /// </summary>
        /// <param name="htmlText">Input string containing zero or more HTML tags</param>
        /// <returns>String containing no HTML tags</returns>
        /// <note>If the string is null or empty, it is returned as is</note>
        public static string StripHtml(this string htmlText)
        {
            if (!string.IsNullOrEmpty(htmlText))
            {
                string decoded = System.Net.WebUtility.HtmlDecode(htmlRegex.Replace(htmlText, string.Empty));

                // remove any \r or \n at the beginning or at the end of the string
                string output = decoded.Trim('\r', '\n');
                output = output.Replace("<", "");
                output = output.Replace(">", "");

                return output;
            }
            else
            {
                return htmlText;
            }
        }

        /// <summary>
        /// Replace all the '\' characters with a '/' character so that an URI struct can be created from the string
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>Output string</returns>
        public static string GetUriCompatibleString(this string input)
        {
            if (input != null && input.Contains(@"\"))
                return input.Replace(@"\", "/");

            return input;
        }

        public static Uri TryCreateUri(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            else
            {
                try
                {
                    return SafeUri.Get(input.GetUriCompatibleString());
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        // from http://stackoverflow.com/a/641632/146222
        private static string CleanInvalidXmlChars(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) 
                return input;

            StringBuilder output = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                if ((ch >= 0x0020 && ch <= 0xD7FF) ||
                    (ch >= 0xE000 && ch <= 0xFFFD) ||
                    ch == 0x0009 ||
                    ch == 0x000A ||
                    ch == 0x000D)
                {
                    output.Append(ch);
                }
            }
            return output.ToString();
        }

        public static string ToEscapedXml(this string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return string.Empty;

            // escape invalid XML character before to prevent nasty exception :-)
            xml = CleanInvalidXmlChars(xml);

            if (string.IsNullOrWhiteSpace(xml))
                return string.Empty;

            try
            {
                var xElement = new XElement("Name", xml);

                // use ToString() to get an escaped version of the input
                if (xElement.FirstNode != null)
                    return xElement.FirstNode.ToString();
            }
            catch (Exception ex)
            {
                if (Ioc.HasType<ITrackingManager>())
                    Ioc.Resolve<ITrackingManager>().Event(TrackingSource.Misc, "ToEscapedXml", string.Format("Failed to escape {0}: {1}", xml, ex.ToString()));
                return xml;
            }

            return string.Empty;
        }

        public static string TryGetHyperlink(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            Regex linkParser = new Regex(@"((https?)\://|www.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\(\)\*\-\._~%]*)*", RegexOptions.IgnoreCase);
            var match = linkParser.Match(input);
            if (match != null && match.Success)
            {
                string parsed = match.ToString();
                if (parsed != null)
                {
                    try
                    {
                        SafeUri.Get(parsed, UriKind.RelativeOrAbsolute);

                        parsed = parsed.ToLowerInvariant();
                        if (!parsed.StartsWith("http") && !parsed.StartsWith("https") && !parsed.StartsWith("www"))
                            parsed = "www." + input;

                        return parsed;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }

            return null;
        }
    }
}

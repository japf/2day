using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Chartreuse.Today.Core.Shared.Services;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Shared.Tools
{
    public class RegexHelper
    {
        private static readonly Regex[] phoneRegexes;
        private static readonly Regex[] uriRegexes;
        
        static RegexHelper()
        {
            phoneRegexes = new[]
            {
                new Regex(@"\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})"),
                new Regex(@"[-. ]?(\+?[0-9]{2,3})?[-. ]?([0-9]{1,3})[-. ]?([0-9]{1,3})[-. ]?([0-9]{2,3})[-. ]?([0-9]{2,3})[-. ]?([0-9]{2,3})[-. ]?([0-9]{0,3})")
            };

            uriRegexes = new[]
            {
                new Regex("((([A-Za-z]{3,9}:(?:\\/\\/)?)(?:[\\-;:&=\\+\\$,\\w]+@)?[A-Za-z0-9\\.\\-]+|(?:www\\.|[\\-;:&=\\+\\$,\\w]+@)[A-Za-z0-9\\.\\-]+)((?:\\/[\\+~%\\/\\.\\w\\-_]*)?\\??(?:[\\-\\+=&;:%@\\.\\w_]*)#?(?:[\\.\\!\\/\\\\\\w]*))?)"),
                //new Regex("(([a-z0-9]+)\\.([a-z0-9]{2,6}))") 
            };
        }

        public static List<string> FindUris(string content)
        {
            var result = new List<string>();
            foreach (var uriRegex in uriRegexes)
            {
                foreach (Match match in uriRegex.Matches(content))
                {
                    if (!result.Contains(match.Value) && result.All(s => !s.Contains(match.Value)))
                        result.Add(match.Value);
                }
            }

            return result;
        }

        public static string FindPhoneNumber(IPlatformService platformService, params string[] contents)
        {
            if (contents == null || contents.Length == 0)
                return null;

            string result = null;
            HashSet<string> matches = new HashSet<string>();

            try
            {
                if (platformService == null || platformService.DeviceFamily != DeviceFamily.WindowsMobile)
                    return null;

                foreach (string content in contents)
                {
                    if (content == null)
                        continue;

                    foreach (var regex in phoneRegexes)
                    {
                        var match = regex.Match(content);
                        if (match.Success && !string.IsNullOrWhiteSpace(match.Value))
                        {
                            string candidate = match.Value.Trim();
                            if (!matches.Contains(candidate))
                                matches.Add(candidate);
                        }
                    }
                }

                int length = -1;
                foreach (string match in matches)
                {
                    if (match.Length > length)
                    {
                        result = match;
                        length = match.Length;
                    }
                }
            }
            catch (Exception ex)
            {
                TrackingManagerHelper.Exception(ex, "Error while getting phone numbers");
            }

            return result;
        }
    }
}

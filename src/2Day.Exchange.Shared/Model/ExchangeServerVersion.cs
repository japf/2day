using System.Collections.Generic;

namespace Chartreuse.Today.Exchange.Model
{
    public enum ExchangeServerVersion   
    {
        Exchange2007_SP1,
        Exchange2010,
        Exchange2010_SP1,
        Exchange2010_SP2,
        Exchange2013,
        ExchangeOffice365,
    }

    public static class ExchangeServerVersionHelper
    {
        private static readonly List<string> exchangeVersions;

        static ExchangeServerVersionHelper()
        {
            exchangeVersions = new List<string>(new[]
                                                         {
                                                             "2007",
                                                             "2010",
                                                             "2013",
                                                             "Office 365"
                                                         });
        }

        public static List<string> ExchangeVersions
        {
            get { return exchangeVersions; }
        }

        public static string GetString(this ExchangeServerVersion version)
        {
            switch (version)
            {
                case ExchangeServerVersion.Exchange2007_SP1:
                    return "2007";
                case ExchangeServerVersion.Exchange2010:
                case ExchangeServerVersion.Exchange2010_SP1:
                case ExchangeServerVersion.Exchange2010_SP2:
                    return "2010";
                case ExchangeServerVersion.Exchange2013:
                    return "2013";
                case ExchangeServerVersion.ExchangeOffice365:
                    return "Office 365";
                default:
                    return "2010";
            }
        }

        public static ExchangeServerVersion GetEnum(string value)
        {
            switch (value)
            {
                case "2007":
                    return ExchangeServerVersion.Exchange2007_SP1;
                case "2010":
                    return ExchangeServerVersion.Exchange2010;
                case "2013":
                    return ExchangeServerVersion.Exchange2013;
                case "Office 365":
                    return ExchangeServerVersion.ExchangeOffice365;
                default:
                    return ExchangeServerVersion.Exchange2010;
            }
        }
    }
}
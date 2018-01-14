using System;
using System.Globalization;

namespace Chartreuse.Today.Exchange.Ews
{
    public static class EwsDateTimeHelper
    {
        public static string ToEwsDateTimeValue(this DateTime datetime, DateTimeKind kind)
        {
            DateTime utcDateTime = datetime;
            DateTime localDateTime;

            switch (datetime.Kind)
            {
                case DateTimeKind.Unspecified:
                    utcDateTime = datetime.ToUniversalTime();
                    break;
                case DateTimeKind.Utc:
                    utcDateTime = datetime;
                    break;
                case DateTimeKind.Local:
                    utcDateTime = datetime.ToUniversalTime();
                    break;
            }
            localDateTime = utcDateTime.ToLocalTime();

            switch (kind)
            {
                case DateTimeKind.Utc:
                    return utcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
                case DateTimeKind.Local:
                    return localDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentException("kind");
            }
        }

        public static string ToEwsXmlDateValue(this DateTime datetime)
        {
            return datetime.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
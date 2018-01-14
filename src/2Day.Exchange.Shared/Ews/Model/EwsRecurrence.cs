using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange.Ews.Model
{
    [DebuggerDisplay("Type: {RecurrenceType} Start: {StartDate} Interval: {Interval} DaysOfWeek: {DaysOfWeek} DayOfWeekIndex: {DayOfWeekIndex} DayOfMonth: {DayOfMonth} Month: {Month}")]
    public class EwsRecurrence
    {
        public ExchangeRecurrencePattern RecurrenceType { get; set; }

        public DateTime StartDate { get; set; }

        public int Interval { get; set; }

        public ExchangeDayOfWeek DaysOfWeek { get; set; }

        public ExchangeDayOfWeekIndex DayOfWeekIndex { get; set; }

        public int DayOfMonth { get; set; }

        public int Month { get; set; }

        private string StartDateXml
        {
            get { return this.StartDate.ToEwsXmlDateValue(); }
        }

        private string MonthName
        {
            get { return CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(this.Month); }
        }

        private string DaysOfWeekString
        {
            get { return this.DaysOfWeek.ToString().Replace(",", ""); }
        }

        internal static EwsRecurrence CreateFromXml(string xml)
        {
            if (xml == null) 
                throw new ArgumentNullException(nameof(xml));

            xml = "<Root>" + xml + "</Root>";

            var xdoc = XDocument.Load(new StringReader(xml));

            var info = new EwsRecurrence {Interval = 1};

            var node = xdoc.TryGetNode("DailyRecurrence");
            if (node != null)
            {
                info.RecurrenceType = ExchangeRecurrencePattern.Daily;
                info.Interval = node.XGetChildValue<int>("Interval", true);
            }

            node = xdoc.TryGetNode("DailyRegeneration");
            if (node != null)
            {
                info.RecurrenceType = ExchangeRecurrencePattern.DailyRegeneration;
                info.Interval = node.XGetChildValue<int>("Interval", true);
            }

            node = xdoc.TryGetNode("WeeklyRecurrence");
            if (node != null)
            {
                info.RecurrenceType = ExchangeRecurrencePattern.Weekly;
                info.Interval = node.XGetChildValue<int>("Interval", true);
                info.DaysOfWeek = node.XGetChildValue<ExchangeDayOfWeek>("DaysOfWeek", true);
            }

            node = xdoc.TryGetNode("WeeklyRegeneration");
            if (node != null)
            {
                info.RecurrenceType = ExchangeRecurrencePattern.WeeklyRegeneration;
                info.Interval = node.XGetChildValue<int>("Interval", true);
            }

            node = xdoc.TryGetNode("AbsoluteMonthlyRecurrence");
            if (node != null)
            {
                info.RecurrenceType = ExchangeRecurrencePattern.Monthly;
                info.Interval = node.XGetChildValue<int>("Interval", true);
                info.DayOfMonth = node.XGetChildValue<int>("DayOfMonth", true);
            }

            node = xdoc.TryGetNode("RelativeMonthlyRecurrence");
            if (node != null)
            {
                info.RecurrenceType = ExchangeRecurrencePattern.MonthlyRelative;
                info.Interval = node.XGetChildValue<int>("Interval", true);
                info.DaysOfWeek = node.XGetChildValue<ExchangeDayOfWeek>("DaysOfWeek", true);
                info.DayOfWeekIndex = node.XGetChildValue<ExchangeDayOfWeekIndex>("DayOfWeekIndex", true);
            }

            node = xdoc.TryGetNode("MonthlyRegeneration");
            if (node != null)
            {
                info.RecurrenceType = ExchangeRecurrencePattern.MonthlyRegeneration;
                info.Interval = node.XGetChildValue<int>("Interval", true);
            }

            node = xdoc.TryGetNode("AbsoluteYearlyRecurrence");
            if (node != null)
            {
                info.RecurrenceType = ExchangeRecurrencePattern.Yearly;
                info.DayOfMonth = node.XGetChildValue<int>("DayOfMonth", true);
                info.Month = node.XGetChildMonthNamesIndex("Month");
            }

            node = xdoc.TryGetNode("RelativeYearlyRecurrence");
            if (node != null)
            {
                info.RecurrenceType = ExchangeRecurrencePattern.YearlyRelative;
                info.DaysOfWeek = node.XGetChildValue<ExchangeDayOfWeek>("DaysOfWeek", true);
                info.DayOfWeekIndex = node.XGetChildValue<ExchangeDayOfWeekIndex>("DayOfWeekIndex", true);
                info.Month = node.XGetChildMonthNamesIndex("Month");
            }

            node = xdoc.TryGetNode("YearlyRegeneration");
            if (node != null)
            {
                info.RecurrenceType = ExchangeRecurrencePattern.YearlyRegeneration;
                info.Interval = node.XGetChildValue<int>("Interval", true);
            }

            node = xdoc.TryGetNode("NoEndRecurrence");
            if (node != null)
                info.StartDate = node.XGetChildValue<DateTime>("StartDate", true);

            return info;
        }

        internal string BuildXml()
        {
            if (this.Interval < 1 || this.Interval > 99)
                throw new NotSupportedException("invalid Interval property value");

            string xml = null;
            switch (this.RecurrenceType)
            {
                case ExchangeRecurrencePattern.None:
                    xml = string.Empty;
                    break;

                case ExchangeRecurrencePattern.Daily:              
                    xml = this.GetTemplate("DailyRecurrence", new Dictionary<string, object>
                    {
                        { "Interval", this.Interval },
                    });
                    break;

                case ExchangeRecurrencePattern.DailyRegeneration:
                    xml = this.GetTemplate("DailyRegeneration", new Dictionary<string, object>
                    {
                        { "Interval", this.Interval },
                    });
                    break;

                case ExchangeRecurrencePattern.Weekly:
                    if (this.DaysOfWeek == ExchangeDayOfWeek.None || this.DaysOfWeek == ExchangeDayOfWeek.Weekday || this.DaysOfWeek == ExchangeDayOfWeek.WeekendDay)
                        throw new NotSupportedException("invalid DaysOfWeek property value");
                    xml = this.GetTemplate("WeeklyRecurrence", new Dictionary<string, object>
                    {
                        { "Interval", this.Interval },
                        { "DaysOfWeek", this.DaysOfWeekString },
                    });
                    break;

                case ExchangeRecurrencePattern.WeeklyRegeneration:
                    xml = this.GetTemplate("WeeklyRegeneration", new Dictionary<string, object>
                    {
                        { "Interval", this.Interval },
                    });
                    break;
                    
                case ExchangeRecurrencePattern.Monthly:
                    if (this.DayOfMonth < 1 || this.DayOfMonth > 31)
                        throw new NotSupportedException("invalid DayOfMonth property value");
                    xml = this.GetTemplate("AbsoluteMonthlyRecurrence", new Dictionary<string, object>
                    {
                        { "Interval", this.Interval },
                        { "DayOfMonth", this.DayOfMonth },
                    });
                    break;

                case ExchangeRecurrencePattern.MonthlyRelative:
                    if (this.DaysOfWeekString.Contains(" "))
                        throw new NotSupportedException("invalid DaysOfWeek property value");
                    xml = this.GetTemplate("RelativeMonthlyRecurrence", new Dictionary<string, object>
                    {
                        { "Interval", this.Interval },
                        { "DaysOfWeek", this.DaysOfWeekString },
                        { "DayOfWeekIndex", this.DayOfWeekIndex },
                    });
                    break;

                case ExchangeRecurrencePattern.MonthlyRegeneration:
                    xml = this.GetTemplate("MonthlyRegeneration", new Dictionary<string, object>
                    {
                        { "Interval", this.Interval },
                    });
                    break;

                case ExchangeRecurrencePattern.Yearly:
                    if (this.Month < 1 || this.Month > 12)
                        throw new NotSupportedException("invalid DayOfMonth property value");
                    if (this.DayOfMonth < 1 || this.DayOfMonth > 31)
                        throw new NotSupportedException("invalid DayOfMonth property value");
                    xml = this.GetTemplate("AbsoluteYearlyRecurrence", new Dictionary<string, object>
                    {
                        { "DayOfMonth", this.DayOfMonth },
                        { "Month", this.MonthName }
                    });
                    break;

                case ExchangeRecurrencePattern.YearlyRelative:
                    if (this.DaysOfWeekString.Contains(" "))
                        throw new NotSupportedException("invalid DaysOfWeek property value");
                    if (this.Month < 1 || this.Month > 12)
                        throw new NotSupportedException("invalid DayOfMonth property value");
                    xml = this.GetTemplate("RelativeYearlyRecurrence", new Dictionary<string, object>
                    {
                        { "DaysOfWeek", this.DaysOfWeekString },
                        { "DayOfWeekIndex", this.DayOfWeekIndex },
                        { "Month", this.MonthName }
                    });
                    break;

                case ExchangeRecurrencePattern.YearlyRegeneration:
                    xml = this.GetTemplate("YearlyRegeneration", new Dictionary<string, object>
                    {
                        { "Interval", this.Interval },
                    });
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return xml;
        }

        private string GetTemplate(string recurrence, Dictionary<string, object> properties)
        {
            const string template =
                "<t:{0}>" +
                "  {1}" +
                "</t:{0}>" +
               "<t:NoEndRecurrence>" +
               "   <t:StartDate>{2}</t:StartDate>" +
               "</t:NoEndRecurrence>";

            var builder = new StringBuilder();
            foreach (var property in properties)
                builder.AppendLine(string.Format("<t:{0}>{1}</t:{0}>", property.Key, property.Value));

            string xml = string.Format(template, recurrence, builder, this.StartDateXml);

            return xml;
        }

        protected bool Equals(EwsRecurrence other)
        {
            return this.RecurrenceType == other.RecurrenceType && 
                this.StartDate.Date.Equals(other.StartDate.Date) && 
                this.Interval == other.Interval && 
                this.DaysOfWeek == other.DaysOfWeek && 
                this.DayOfWeekIndex == other.DayOfWeekIndex && 
                this.DayOfMonth == other.DayOfMonth && 
                this.Month == other.Month;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((EwsRecurrence)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)this.RecurrenceType;
                hashCode = (hashCode * 397) ^ this.StartDate.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Interval;
                hashCode = (hashCode * 397) ^ (int)this.DaysOfWeek;
                hashCode = (hashCode * 397) ^ (int)this.DayOfWeekIndex;
                hashCode = (hashCode * 397) ^ this.DayOfMonth;
                hashCode = (hashCode * 397) ^ this.Month;
                return hashCode;
            }
        }

        public static bool operator ==(EwsRecurrence left, EwsRecurrence right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EwsRecurrence left, EwsRecurrence right)
        {
            return !Equals(left, right);
        }
    }
}

using System;
using System.Globalization;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public struct SmartViewDateParameter
    {
        public DateTime? Date { get; private set; }
        public int Days { get; private set; }

        public SmartViewDateParameter(DateTime? date) : this()
        {
            this.Date = date;
        }

        public SmartViewDateParameter(int days)
            : this()
        {
            this.Days = days;
        }

        public override string ToString()
        {
            if (this.Date != null)
                return this.Date.Value.ToString("O", CultureInfo.InvariantCulture);
            else
                return this.Days.ToString(CultureInfo.InvariantCulture);
        }
    }
}
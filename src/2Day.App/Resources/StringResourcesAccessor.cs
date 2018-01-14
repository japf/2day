using Chartreuse.Today.Core.Shared.Tools.Extensions;

namespace Chartreuse.Today.App.Resources
{
    public partial class StringResourcesAccessor
    {
        public string DateMonday
        {
            get { return DateTimeExtensions.MondayString; }
        }

        public string DateTuesday
        {
            get { return DateTimeExtensions.TuesdayString; }
        }

        public string DateWednesday
        {
            get { return DateTimeExtensions.WednesdayString; }
        }

        public string DateThursday
        {
            get { return DateTimeExtensions.ThursdayString; }
        }

        public string DateFriday
        {
            get { return DateTimeExtensions.FridayString; }
        }

        public string DateSaturday
        {
            get { return DateTimeExtensions.SaturdayString; }
        }

        public string DateSunday
        {
            get { return DateTimeExtensions.SundayString; }
        }        
    }
}

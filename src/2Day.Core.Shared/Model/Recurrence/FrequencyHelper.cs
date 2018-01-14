namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public static class FrequencyHelper
    {
        public static T Custom<T>(this Frequency frequency) where T : ICustomFrequency
        {
            return (T) frequency.CustomFrequency;
        }
    }
}
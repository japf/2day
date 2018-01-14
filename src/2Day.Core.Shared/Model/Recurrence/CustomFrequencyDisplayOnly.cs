using System;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public class CustomFrequencyDisplayOnly : ICustomFrequency
    {
        private readonly Frequency frequency;

        public CustomFrequencyDisplayOnly(Frequency frequency)
        {
            this.frequency = frequency;

            this.DefaultTitle = StringResources.CustomFrequency_Custom;
        }

        public bool Equals(ICustomFrequency other)
        {
            return false;
        }

        public FrequencyType FrequencyType { get; }
        public string Value { get; set; }
        public string DefaultTitle { get; }
        public string DisplayValue { get; }
        public bool IsCustomizable { get; }
        public bool IsValidValue { get; }

        public Frequency Frequency
        {
            get { return this.frequency; }
        }

        public void SetReferenceDate(DateTime? date)
        {
        }

        public DateTime ComputeNextDate(DateTime fromDate)
        {
            return new DateTime();
        }
    }
}

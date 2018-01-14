using System;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public class OnceOnlyFrequency : ICustomFrequency
    {
        public FrequencyType FrequencyType
        {
            get { return FrequencyType.Once; }
        }

        public string Value
        {
            get
            {
                return String.Empty;
            }
            set
            {
                // Do nothing
            }
        }

        public string DefaultTitle
        {
            get { return StringResources.CustomFrequency_Once; }
        }

        public string DisplayValue
        {
            get { return StringResources.CustomFrequency_Once; }
        }

        public bool IsCustomizable
        {
            get { return false; }
        }

        public bool IsValidValue
        {
            get { return true; }
        }

        public DateTime ComputeNextDate(DateTime fromDate)
        {
            throw new NotSupportedException();
        }

        public void SetReferenceDate(DateTime? date)
        {
        }

        public bool Equals(ICustomFrequency other)
        {
            return other is OnceOnlyFrequency;
        }
    }
}

using System;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public class Frequency
    {
        public bool UseFixedDate { get; set; }
        public ICustomFrequency CustomFrequency { get; set; }

        public bool IsCustom
        {
            get { return this.CustomFrequency != null && this.CustomFrequency.IsCustomizable; }
        }

        public Frequency()
        {
            this.UseFixedDate = true;
        }

        public Frequency(ICustomFrequency customFrequency)
            : this()
        {
            if (customFrequency == null)
                throw new ArgumentNullException("customFrequency");
            this.CustomFrequency = customFrequency;
        }

    }
}

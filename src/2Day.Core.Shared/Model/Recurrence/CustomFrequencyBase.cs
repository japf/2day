using System;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public abstract class CustomFrequencyBase : ICustomFrequency, IEquatable<ICustomFrequency>
    {
        protected const char valueSeparator = '|';

        #region Private fields
        protected string value;
        protected string displayValue;
        protected bool invalidateDisplayValue;
        #endregion

        #region Public properties
        public abstract FrequencyType FrequencyType { get; }

        public string Value
        {
            get { return this.value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    this.ParseValue(this.value);
                }
            }
        }

        public string DisplayValue
        {
            get
            {
                // Update display value only when requested
                if (this.invalidateDisplayValue)
                    this.UpdateDisplayValue();
                return this.displayValue;
            }
        }

        public abstract string DefaultTitle { get; }

        public abstract bool IsCustomizable { get; }

        public abstract bool IsValidValue { get; }

        #endregion

        protected CustomFrequencyBase()
        {
            this.invalidateDisplayValue = true;
        }

        public abstract DateTime ComputeNextDate(DateTime fromDate);

        protected abstract void ParseValue(string value);

        public virtual void SetReferenceDate(DateTime? date) {}

        protected abstract void UpdateDisplayValue();

        public virtual bool Equals(ICustomFrequency other)
        {
            return other != null && other.GetType() == this.GetType() && other.Value == this.Value;
        }
    }
}

namespace Chartreuse.Today.Core.Shared.Tools
{
    public class EnumLocalizedValue
    {
        public object Value { get; private set; }
        public string DisplayValue { get; private set; }

        public EnumLocalizedValue(object value, string displayValue)
        {
            this.DisplayValue = displayValue;
            this.Value = value;
        }

        public override string ToString()
        {
            return this.DisplayValue;
        }
    }
}
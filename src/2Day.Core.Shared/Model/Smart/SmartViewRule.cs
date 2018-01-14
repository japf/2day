using System;
using System.Collections.Generic;
using System.Linq;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public abstract class SmartViewRule<T> : SmartViewRule
    {
        protected SmartViewRule()
        {
        }

        protected SmartViewRule(SmartViewFilter filter, SmartViewField field, T value) :
            base(filter, field, value)
        {
        }

        public new T Value
        {
            get { return (T) this.value; }
        }

        public override string AsString()
        {
            return string.Format("{0} {1} {2}", this.Field, this.Filter, this.Value);
        }
    }

    public abstract class SmartViewRule
    {
        private readonly SmartViewFilter filter;
        private readonly SmartViewField field;
        
        protected readonly object value;

        public SmartViewFilter Filter
        {
            get { return this.filter; }
        }

        public SmartViewField Field
        {
            get { return this.field; }
        }

        public abstract List<SmartViewField> SupportedFields { get; }

        public abstract List<SmartViewFilter> SupportedFilters { get; }

        public object Value
        {
            get { return this.value; }
        }

        protected SmartViewRule()
        {
        }

        protected SmartViewRule(SmartViewFilter filter, SmartViewField field, object value)
        {
            if (!this.SupportedFilters.Contains(filter))
                throw new ArgumentException("filter");
            if (!this.SupportedFields.Contains(field))
                throw new ArgumentException("field");

            this.filter = filter;
            this.field = field;
            this.value = value;
        }

        public abstract bool IsMatch(ITask task);

        public abstract string AsString();

        public bool KnowsField(string source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return this.SupportedFields.Any(f => f.ToString().Equals(source));
        }

        public bool KnowsField(SmartViewField field)
        {
            return this.SupportedFields.Any(f => f.Equals(field));
        }

        public abstract SmartViewRule Read(SmartViewFilter filter, SmartViewField field, string value);
    }
}
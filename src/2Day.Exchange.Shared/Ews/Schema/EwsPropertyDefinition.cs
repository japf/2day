using System;
using System.Diagnostics;

namespace Chartreuse.Today.Exchange.Ews.Schema
{
    [DebuggerDisplay("Field {fieldUri}")]
    public abstract class EwsPropertyDefinition
    {
        private readonly EwsFieldUri fieldUri;

        public EwsFieldUri FieldUri
        {
            get { return this.fieldUri; }
        }

        public virtual int PropertyId
        {
            get { return 0; }
        }

        public abstract string GetXml();

        protected EwsPropertyDefinition(EwsFieldUri fieldUri)
        {
            this.fieldUri = fieldUri;
        }

        public virtual string BuildSetFieldXml(string[] value, EwsAttributes attributes)
        {
            throw new NotSupportedException();
        }

        public virtual string BuildClearFieldXml()
        {
            throw new NotSupportedException();
        }

        public virtual string BuildSetFieldValueXml(string[] value, EwsAttributes attributes)
        {
            throw new NotSupportedException();
        }
    }
}
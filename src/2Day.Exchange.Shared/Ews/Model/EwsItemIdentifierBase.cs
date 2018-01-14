using System;
using System.Diagnostics;

namespace Chartreuse.Today.Exchange.Ews.Model
{
    [DebuggerDisplay("IsValid: {IsValid} Id: {Id}")]
    public class EwsItemIdentifierBase
    {
        public string Id { get; protected set; }

        public string ChangeKey { get; protected set; }

        public string ErrorMessage { get; protected set; }

        public bool IsValid
        {
            get { return string.IsNullOrWhiteSpace(this.ErrorMessage); }
        }

        public EwsItemIdentifierBase(string id, string changeKey)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrWhiteSpace(changeKey))
                throw new ArgumentNullException("changeKey");

            this.Id = id;
            this.ChangeKey = changeKey;
        }

        public EwsItemIdentifierBase(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentNullException("errorMessage");

            this.ErrorMessage = errorMessage;
        }
    }
}
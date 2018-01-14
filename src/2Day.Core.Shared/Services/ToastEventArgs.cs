using System;

namespace Chartreuse.Today.Core.Shared.Services
{
    public class ToastEventArgs : EventArgs
    {
        private readonly string message;
        private readonly ToastType type;

        public ToastEventArgs(string message, ToastType type)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            this.message = message;
            this.type = type;
        }

        public string Message
        {
            get { return this.message; }
        }

        public ToastType Type
        {
            get { return this.type; }
        }
    }
}

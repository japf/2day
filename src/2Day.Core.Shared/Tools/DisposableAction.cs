using System;

namespace Chartreuse.Today.Core.Shared.Tools
{
    /// <summary>
    /// An implementation of <see cref="IDisposable"/> that executes an <see cref="Action"/> when the object is disposed
    /// </summary>
    public class DisposableAction : IDisposable
    {
        public static IDisposable Empty = new DisposableAction();

        private readonly bool isEmpty;
        private Action action;

        private DisposableAction()
        {
            this.isEmpty = true;
        }

        public DisposableAction(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            this.action = action;
            this.isEmpty = false;
        }


        public void Dispose()
        {
            if (!this.isEmpty)
            {
                if (this.action == null)
                    throw new NotSupportedException("This object has already been disposed");

                this.action();
                this.action = null;
            }
        }
    }
}

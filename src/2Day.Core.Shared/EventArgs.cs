using System;
using System.Diagnostics;

namespace Chartreuse.Today.Core.Shared
{
    public class EventArgs<T> : EventArgs
    {
        public T Item { get; private set; }

        [DebuggerStepThrough]
        public EventArgs(T item)
        {
            this.Item = item;
        }
    }

    public class EventArgs<TItem, TParameter> : EventArgs
    {
        public TItem Item { get; private set; }
        public TParameter Parameter { get; private set; }

        [DebuggerStepThrough]
        public EventArgs(TItem item, TParameter parameter)
        {
            this.Item = item;
            this.Parameter = parameter;
        }
    }
}

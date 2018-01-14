using System;

namespace Chartreuse.Today.Core.Shared.Tools.Dialog
{
    public interface IMessageBoxContent
    {
        string Content { get; }
        event EventHandler RequestClose;
    }
}
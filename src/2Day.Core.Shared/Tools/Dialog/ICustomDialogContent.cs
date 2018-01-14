using System;

namespace Chartreuse.Today.Core.Shared.Tools.Dialog
{
    public interface ICustomDialogContent
    {
        void OnDismissed();
        event EventHandler RequestClose;
    }
}
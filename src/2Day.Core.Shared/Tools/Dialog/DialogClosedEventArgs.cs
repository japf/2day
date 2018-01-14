using System;

namespace Chartreuse.Today.Core.Shared.Tools.Dialog
{
    public class DialogClosedEventArgs : EventArgs
    {
        public DialogClosedEventArgs(int buttonIndex)
        {
            this.ButtonIndex = buttonIndex;
            if (buttonIndex == -1)
            {
                this.Cancel = true;
            }
        }

        public DialogClosedEventArgs(bool cancel, string text)
        {
            this.Cancel = cancel;
            this.Text = text;
        }

        public int ButtonIndex { get; private set; }
        public string Text { get; private set; }
        public bool Cancel { get; private set; }
    }
}
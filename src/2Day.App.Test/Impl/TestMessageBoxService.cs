using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Tools.Dialog;

namespace Chartreuse.Today.App.Test.Impl
{
    public class TestMessageBoxService : IMessageBoxService
    {
        public Task<DialogClosedEventArgs> ShowAsync(string title, string content, IEnumerable<string> buttons)
        {
            return null;
        }
        
        public Task<DialogResult> ShowAsync(string title, string content, DialogButton button = DialogButton.OK)
        {
            return null;
        }

        public Task<string> ShowCustomTextEditDialogAsync(string title, string placeholder, string content = null)
        {
            return null;
        }
        
        public Task ShowToastAsync(string title, string content)
        {
            return null;
        }
    }
}
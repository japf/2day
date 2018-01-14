using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Tools.Dialog
{
    public interface IMessageBoxService
    {
        Task<DialogClosedEventArgs> ShowAsync(string title, string content, IEnumerable<string> buttons);
                
        Task<DialogResult> ShowAsync(string title, string content, DialogButton button = DialogButton.OK);

        Task<string> ShowCustomTextEditDialogAsync(string title, string placeholder, string content = null);
    }
}
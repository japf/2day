using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.VoiceCommand.Commands.Foreground
{
    internal class OpenViewCortanaForegroundCommand : OpenAbstractFolderCortanaForegoundCommandBase
    {
        public OpenViewCortanaForegroundCommand() : base("view", "openView")
        {
        }

        protected override IEnumerable<IAbstractFolder> GetFolders(IWorkbook workbook)
        {
            return workbook.Views;
        }
    }
}
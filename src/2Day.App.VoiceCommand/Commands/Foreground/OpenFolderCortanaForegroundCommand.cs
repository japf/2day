using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.VoiceCommand.Commands.Foreground
{
    internal class OpenFolderCortanaForegroundCommand : OpenAbstractFolderCortanaForegoundCommandBase
    {
        public OpenFolderCortanaForegroundCommand() : base("folder", "openFolder")
        {
        }

        protected override IEnumerable<IAbstractFolder> GetFolders(IWorkbook workbook)
        {
            return workbook.Folders;
        }
    }
}
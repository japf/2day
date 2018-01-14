using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.VoiceCommand.Commands.Foreground
{
    internal class OpenContextCortanaForegroundCommand : OpenAbstractFolderCortanaForegoundCommandBase
    {
        public OpenContextCortanaForegroundCommand() : base("context", "openContext")
        {
        }

        protected override IEnumerable<IAbstractFolder> GetFolders(IWorkbook workbook)
        {
            return workbook.Contexts;
        }
    }
}
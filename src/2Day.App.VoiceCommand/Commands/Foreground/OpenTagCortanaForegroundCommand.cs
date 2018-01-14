using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.VoiceCommand.Commands.Foreground
{
    internal class OpenTagCortanaForegroundCommand : OpenAbstractFolderCortanaForegoundCommandBase
    {
        public OpenTagCortanaForegroundCommand() : base("tag", "openTag")
        {
        }

        protected override IEnumerable<IAbstractFolder> GetFolders(IWorkbook workbook)
        {
            return workbook.Tags;
        }
    }
}
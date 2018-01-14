using System.Collections.Generic;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.VoiceCommand.Commands.Foreground
{
    internal class OpenSmartViewCortanaForegroundCommand : OpenAbstractFolderCortanaForegoundCommandBase
    {
        public OpenSmartViewCortanaForegroundCommand() : base("smartview", "openSmartView")
        {
        }

        protected override IEnumerable<IAbstractFolder> GetFolders(IWorkbook workbook)
        {
            return workbook.SmartViews;
        }
    }
}
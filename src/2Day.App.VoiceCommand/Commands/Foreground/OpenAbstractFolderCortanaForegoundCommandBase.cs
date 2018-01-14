using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Media.SpeechRecognition;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.VoiceCommand.Commands.Foreground
{
    internal abstract class OpenAbstractFolderCortanaForegoundCommandBase : CortanaForegroundCommandBase
    {
        private readonly string folderKeyName;

        protected OpenAbstractFolderCortanaForegoundCommandBase(string folderKeyName, string commandName) : base(commandName)
        {
            this.folderKeyName = folderKeyName;
        }

        protected abstract IEnumerable<IAbstractFolder> GetFolders(IWorkbook workbook);

        public override bool Execute(SpeechRecognitionResult result, ICortanaRuntimeActions runtimeActions, IWorkbook workbook)
        {
            string folderName = this.SemanticInterpretation(this.folderKeyName, result);
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                var target = this.GetFolders(workbook).FirstOrDefault(f => f.Name.Equals(folderName, StringComparison.CurrentCultureIgnoreCase));
                if (target != null)
                {
                    runtimeActions.SelectFolder(target);
                    return true;
                }
            }

            return false;
        }
    }
}
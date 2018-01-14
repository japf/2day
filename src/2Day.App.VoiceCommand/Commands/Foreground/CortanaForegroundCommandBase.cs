using System.Linq;
using Windows.Media.SpeechRecognition;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.VoiceCommand.Commands.Foreground
{
    internal abstract class CortanaForegroundCommandBase : CortanaCommandBase
    {
        protected CortanaForegroundCommandBase(string commandName) : base(commandName)
        {
        }

        public abstract bool Execute(SpeechRecognitionResult result, ICortanaRuntimeActions runtimeActions, IWorkbook workbook);

        /// <summary>
        /// Returns the semantic interpretation of a speech result. Returns null if there is no interpretation for
        /// that key.
        /// </summary>
        /// <param name="interpretationKey">The interpretation key.</param>
        /// <param name="speechRecognitionResult">The result to get an interpretation from.</param>
        /// <returns></returns>
        protected string SemanticInterpretation(string interpretationKey, SpeechRecognitionResult speechRecognitionResult)
        {
            if (speechRecognitionResult.SemanticInterpretation.Properties.ContainsKey(interpretationKey))
                return speechRecognitionResult.SemanticInterpretation.Properties[interpretationKey].FirstOrDefault();

            return string.Empty;
        }
    }
}
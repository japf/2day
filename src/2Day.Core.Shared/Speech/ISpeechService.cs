using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chartreuse.Today.Core.Shared.Speech
{
    public interface ISpeechService
    {
        /// <summary>
        /// Recognize speech with a grammar that recognizes the strings given as argument (ex: use with "Yes" and "No")
        /// </summary>
        /// <param name="choices">The possible sentence to recognize</param>
        /// <returns>A task that completed when recognition is completed</returns>
        Task<SpeechResult> RecognizeAsync(IEnumerable<string> choices = null);

        /// <summary>
        /// Recognize speech
        /// </summary>
        /// <param name="listenText">Listen text shown in the UI</param>
        /// <param name="exampleText">Example text</param>
        /// <returns>A task that completed when recognition is completed</returns>
        Task<SpeechResult> RecognizeAsync(string listenText, string exampleText);
    }
}

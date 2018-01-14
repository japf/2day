using System.Collections.Generic;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Speech;

namespace Chartreuse.Today.App.Test.Impl
{
    public class TestSpeechService : ISpeechService
    {
        public Task SpeechAsync(string text)
        {
            return null;
        }

        public Task<SpeechResult> RecognizeAsync(IEnumerable<string> choices = null)
        {
            return null;
        }

        public Task<SpeechResult> RecognizeAsync(string listenText, string exampleText)
        {
            return null;
        }

        public void PauseMusicPlayback()
        {
        }

        public void ResumeMusicPlayback()
        {
        }
    }
}
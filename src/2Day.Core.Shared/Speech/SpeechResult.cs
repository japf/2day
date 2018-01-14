using System;

namespace Chartreuse.Today.Core.Shared.Speech
{
    public struct SpeechResult
    {
        public string Text { get; private set; }
        public bool IsSuccess { get; private set; }

        public SpeechResult(string text, bool success = true) : this()
        {
            this.Text = text;
            this.IsSuccess = success;

            if (string.IsNullOrWhiteSpace(text) && success)
                throw new ArgumentException("When text is not defined, success must be false");
        }
    }
}
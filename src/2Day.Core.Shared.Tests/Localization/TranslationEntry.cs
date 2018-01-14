using System.Diagnostics;

namespace Chartreuse.Today.Core.Shared.Tests.Localization
{
    [DebuggerDisplay("Key: {Key} Value: {Value}")]
    public struct TranslationEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
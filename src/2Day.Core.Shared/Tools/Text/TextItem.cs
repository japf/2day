namespace Chartreuse.Today.Core.Shared.Tools.Text
{
    /// <summary>
    /// Text item. Simple plain old C# object (POCO) to
    /// hold a string.
    /// </summary>
    public class TextItem
    {
        public string Text { get; private set; }

        public TextItem(string text)
        {
            this.Text = text;
        }
    }
}

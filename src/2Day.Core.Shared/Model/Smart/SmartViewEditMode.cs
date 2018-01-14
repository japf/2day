namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public struct SmartViewEditMode
    {
        public SmartViewEditType Type { get; private set; }
        public string Suffix { get; private set; }   

        public SmartViewEditMode(SmartViewEditType type, string suffix = null) : this()
        {
            this.Suffix = suffix;
            this.Type = type;
        }
    }
}
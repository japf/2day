namespace Chartreuse.Today.Exchange.Ews.Model
{
    public static class EwsKnownFolderIdentifiers
    {
        public static readonly EwsFolderIdentifier DeletedItems = new EwsFolderIdentifier("deleteditems", true);
        public static readonly EwsFolderIdentifier Tasks = new EwsFolderIdentifier("tasks", true);
        public static readonly EwsFolderIdentifier SearchFolders = new EwsFolderIdentifier("searchfolders", true);
        public static readonly EwsFolderIdentifier Root = new EwsFolderIdentifier("root", true);
        public static readonly EwsFolderIdentifier Inbox = new EwsFolderIdentifier("inbox", true);
        public static readonly EwsFolderIdentifier Draft = new EwsFolderIdentifier("draft", true);
    }
}
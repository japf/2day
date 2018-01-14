namespace Chartreuse.Today.ToodleDo
{
    public class ToodleDoAccount
    {
        public string UserId { get; private set; }
        public int FolderEditTimestamp { get; private set; }
        public int ContextEditTimestamp { get; private set; }
        public int TaskEditTimestamp { get; private set; }
        public int TaskDeleteTimestamp { get; private set; }

        public ToodleDoAccount(string userId, int folderEditTimestamp, int contextEditTimestamp, int taskEditTimestamp, int taskDeleteTimestamp)
        {
            this.UserId = userId;

            this.ContextEditTimestamp = contextEditTimestamp;
            this.FolderEditTimestamp = folderEditTimestamp;
            this.TaskEditTimestamp = taskEditTimestamp;
            this.TaskDeleteTimestamp = taskDeleteTimestamp;
        }
    }
}
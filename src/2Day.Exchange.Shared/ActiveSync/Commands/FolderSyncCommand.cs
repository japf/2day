namespace Chartreuse.Today.Exchange.ActiveSync.Commands
{
    // folder sync status code: http://msdn.microsoft.com/en-us/library/gg675582(v=exchg.80).aspx
    internal class FolderSyncCommand : ASCommandBase<FolderSyncCommandParameter, FolderSyncCommandResult>
    {
        public override string CommandName
        {
            get { return "FolderSync"; }
        }

        public FolderSyncCommand(FolderSyncCommandParameter parameter, ASRequestSettings settings) 
            : base(parameter, settings)
        {
        }
    }
}

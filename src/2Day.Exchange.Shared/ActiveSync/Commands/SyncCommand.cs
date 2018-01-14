namespace Chartreuse.Today.Exchange.ActiveSync.Commands
{
    internal class SyncCommand : ASCommandBase<SyncCommandParameter,SyncCommandResult>
    {
        public override string CommandName
        {
            get { return "Sync"; }
        }

        public SyncCommand(SyncCommandParameter parameter, ASRequestSettings settings)
            : base(parameter, settings)
        {
        }
    }
}

namespace Chartreuse.Today.Exchange.ActiveSync.Commands
{
    internal class ProvisionCommand : ASCommandBase<ProvisionCommandParameter, ProvisionCommandResult>
    {
        public override string CommandName
        {
            get { return "Provision"; }
        }

        public ProvisionCommand(ProvisionCommandParameter parameter, ASRequestSettings settings) 
            : base(parameter, settings)
        {
        }
    }
}

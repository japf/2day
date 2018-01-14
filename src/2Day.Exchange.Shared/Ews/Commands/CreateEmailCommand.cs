namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class CreateEmailCommand : EwsCommandBase<CreateEmailParameter, CreateEmailResult>
    {
        public override string CommandName
        {
            get { return "CreateItem"; }
        }

        public CreateEmailCommand(CreateEmailParameter parameters, EwsRequestSettings settings)
            : base(new EwsRequestParameter<CreateEmailParameter>(parameters, settings))
        {
        }
    }
}
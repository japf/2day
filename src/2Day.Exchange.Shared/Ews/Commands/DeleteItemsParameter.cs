using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Commands
{
    public class DeleteItemsParameter : MoveItemsParameter
    {
        public DeleteItemsParameter()
        {
            this.Target = EwsKnownFolderIdentifiers.DeletedItems;
        }

        protected override void EnsureCanExecute()
        {
        }
    }
}
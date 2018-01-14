namespace Chartreuse.Today.Core.Shared.Sync
{
    public class SynchronizationChanges
    {
        public int LocalAdd { get; set; }
        public int LocalEdit { get; set; }
        public int LocalDelete { get; set; }

        public int WebAdd { get; set; }
        public int WebEdit { get; set; }
        public int WebDelete { get; set; }

        public void Reset()
        {
            this.LocalAdd = 0;
            this.LocalEdit = 0;
            this.LocalDelete = 0;

            this.WebAdd = 0;
            this.WebEdit = 0;
            this.WebDelete = 0;
        }
    }
}

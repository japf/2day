namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class DebugItemViewModel : ViewModelBase
    {
        private int value;
        public string Name { get; set; }

        public int Value
        {
            get { return this.value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    this.RaisePropertyChanged("Value");
                }
            }
        }
    }
}
using System;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    public class SeparatorItemViewModel : MenuItemViewModel
    {
        private readonly string name;

        public override string Name
        {
            get { return this.name; }
        }

        public SeparatorItemViewModel(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            this.name = name;
        }
    }
}
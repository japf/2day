using System.Collections.Generic;
using System.Diagnostics;
using Chartreuse.Today.Core.Shared.Model.Smart;

namespace Chartreuse.Today.App.Shared.ViewModel
{
    [DebuggerDisplay("Value: {value} Type: {editType}")]
    public class SmartViewRuleValueViewModel : ViewModelBase
    {
        private readonly SmartViewEditType editType;
        private readonly IEnumerable<string> values;
        private object value;

        public SmartViewRuleValueViewModel(SmartViewEditType type, IEnumerable<string> values = null)
        {
            this.editType = type;
            this.values = values;
        }

        public SmartViewEditType EditType
        {
            get { return this.editType; }
        }
        
        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    this.RaisePropertyChanged("Value");
                }
            }
        }

        public IEnumerable<string> Values
        {
            get { return this.values; }
        }
    }
}
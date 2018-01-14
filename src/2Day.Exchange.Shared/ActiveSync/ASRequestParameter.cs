using Chartreuse.Today.Exchange.Shared.Commands;

namespace Chartreuse.Today.Exchange.ActiveSync
{
    public class ASRequestParameter<TParameter> : CommandParameterBase<TParameter, ASRequestSettings> 
        where TParameter : IRequestParameterBuilder
    {
        public ASRequestParameter(TParameter parameter, ASRequestSettings settings) 
            : base(parameter, settings)
        {
        }
    }
}

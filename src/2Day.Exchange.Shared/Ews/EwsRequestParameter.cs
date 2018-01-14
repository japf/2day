using Chartreuse.Today.Exchange.Shared.Commands;

namespace Chartreuse.Today.Exchange.Ews
{
    public class EwsRequestParameter<TParameter> : CommandParameterBase<TParameter, EwsRequestSettings>
        where TParameter : IRequestParameterBuilder
    {
        public EwsRequestParameter(TParameter parameter, EwsRequestSettings settings)
            : base(parameter, settings)
        {
        }
    }
}

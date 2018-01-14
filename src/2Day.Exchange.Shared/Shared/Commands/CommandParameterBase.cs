using System;

namespace Chartreuse.Today.Exchange.Shared.Commands
{
    public abstract class CommandParameterBase<TParameter, TSettings> where TParameter : IRequestParameterBuilder
    {
        public TParameter Parameter { get; private set; }

        public TSettings Settings { get; private set; }

        protected CommandParameterBase(TParameter parameter, TSettings settings)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");
            if (settings == null)
                throw new ArgumentNullException("settings");

            this.Settings = settings;
            this.Parameter = parameter;
        }
    }
}

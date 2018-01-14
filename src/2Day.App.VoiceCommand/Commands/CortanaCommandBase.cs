using System;

namespace Chartreuse.Today.App.VoiceCommand.Commands
{
    internal abstract class CortanaCommandBase
    {
        private readonly string commandName;

        internal CortanaCommandBase(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentNullException(nameof(commandName));

            this.commandName = commandName;
        }

        public string CommandName
        {
            get { return this.commandName; }
        }
    }
}
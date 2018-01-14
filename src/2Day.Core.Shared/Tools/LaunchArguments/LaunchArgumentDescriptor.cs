using System;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Shared.Tools.LaunchArguments
{
    public class LaunchArgumentDescriptor
    {
        internal LaunchArgumentDescriptor(LaunchArgumentType type)
        {
            this.Type = type;
        }

        internal LaunchArgumentDescriptor(ITask task, LaunchArgumentType type)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            this.Task = task;
            this.Type = type;
        }

        internal LaunchArgumentDescriptor(IAbstractFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            this.Folder = folder;
            this.Type = LaunchArgumentType.Select;
        }

        internal LaunchArgumentDescriptor()
        {
            this.Type = LaunchArgumentType.Unknown;
        }

        public ITask Task { get; private set; }

        public IAbstractFolder Folder { get; private set; }

        public LaunchArgumentType Type { get; private set; }
    }
}
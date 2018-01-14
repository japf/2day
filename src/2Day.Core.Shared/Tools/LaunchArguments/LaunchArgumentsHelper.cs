using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.Core.Shared.Tools.LaunchArguments
{
    public static class LaunchArgumentsHelper
    {
        private const string TagIdFormat = "tag-{0}";
        private const string ViewIdFormat = "view-{0}";
        private const string SmartViewIdFormat = "smartview-{0}";
        private const string FolderIdFormat = "folder-{0}";
        private const string ContextIdFormat = "context-{0}";
        private const string TaskIdFormat = "task-{0}";

        public const string QuickAddTask = "quickAddTile";
        public const string Sync = "sync";

        private static readonly LaunchArgumentDescriptor emptyDescriptor = new LaunchArgumentDescriptor();

        private static readonly Dictionary<string, LaunchArgumentType> launchArgumentTypes = new Dictionary<string, LaunchArgumentType>
        {
            { "complete", LaunchArgumentType.CompleteTask },
            { "edit", LaunchArgumentType.EditTask }
        }; 

        public static string GetArgCompleteTask(ITask task)
        {
            return string.Format("complete/{0}", string.Format(TaskIdFormat, task.Id));
        }

        public static string GetArgEditTask(ITask task)
        {
            return string.Format("{0}", string.Format(TaskIdFormat, task.Id));
        }

        public static string GetArgSelectFolder(IAbstractFolder abstractFolder)
        {
            if (abstractFolder is ITag)
                return string.Format(TagIdFormat, abstractFolder.Id);
            else if (abstractFolder is ISmartView)
                return string.Format(SmartViewIdFormat, abstractFolder.Id);
            else if (abstractFolder is IView)
                return string.Format(ViewIdFormat, abstractFolder.Id);
            else if (abstractFolder is IFolder)
                return string.Format(FolderIdFormat, abstractFolder.Id);
            else if (abstractFolder is IContext)
                return string.Format(ContextIdFormat, abstractFolder.Id);
            else
                return null;
        }

        public static LaunchArgumentDescriptor GetDescriptorFromArgument(IWorkbook workbook, string argument)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            // input is 'action/task-0' or 'task-0' or 'folder-0'
            if (string.IsNullOrWhiteSpace(argument))
                return emptyDescriptor;

            int itemId = -1;

            LaunchArgumentType type = LaunchArgumentType.EditTask;

            string[] parameters = argument.Split('/');
            if (parameters.Length == 2 && launchArgumentTypes.ContainsKey(parameters[0]))
            {
                type = launchArgumentTypes[parameters[0]];
                argument = parameters[1];
            }
            else if (argument == Sync)
            {
                return new LaunchArgumentDescriptor(LaunchArgumentType.Sync);
            }

            string[] itemDescriptor = argument.Split('-');
            if (itemDescriptor.Length != 2)
                return emptyDescriptor;

            if (!int.TryParse(itemDescriptor[1], out itemId))
                return emptyDescriptor;

            string itemType = itemDescriptor[0];
            if (itemType == "task")
            {
                ITask task = workbook.Tasks.FirstOrDefault(t => t.Id == itemId);
                if (task != null)
                    return new LaunchArgumentDescriptor(task, type);
            }
            else if (itemType == "folder")
            {
                IAbstractFolder folder = workbook.Folders.FirstOrDefault(t => t.Id == itemId);
                if (folder != null)
                    return new LaunchArgumentDescriptor(folder);
            }
            else if (itemType == "smartview")
            {
                IAbstractFolder folder = workbook.SmartViews.FirstOrDefault(t => t.Id == itemId);
                if (folder != null)
                    return new LaunchArgumentDescriptor(folder);
            }
            else if (itemType == "view")
            {
                IAbstractFolder folder = workbook.Views.FirstOrDefault(t => t.Id == itemId);
                if (folder != null)
                    return new LaunchArgumentDescriptor(folder);
            }
            else if (itemType == "context")
            {
                IAbstractFolder folder = workbook.Contexts.FirstOrDefault(t => t.Id == itemId);
                if (folder != null)
                    return new LaunchArgumentDescriptor(folder);
            }
            else if (itemType == "tag")
            {
                IAbstractFolder folder = workbook.Tags.FirstOrDefault(t => t.Id == itemId);
                if (folder != null)
                    return new LaunchArgumentDescriptor(folder);
            }

            return emptyDescriptor;
        }
    }
}

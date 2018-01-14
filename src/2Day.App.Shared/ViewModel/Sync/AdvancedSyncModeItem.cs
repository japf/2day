using System;
using System.Windows.Input;

namespace Chartreuse.Today.App.Shared.ViewModel.Sync
{
    public class AdvancedSyncModeItem
    {
        private readonly string title;
        private readonly string description;
        private readonly Uri icon;
        private readonly Action action;
        private readonly ICommand executeCommand;

        public AdvancedSyncModeItem(string title, string description, Uri icon, Action action)
        {
            if (title == null)
                throw new ArgumentNullException("title");
            if (description == null)
                throw new ArgumentNullException("description");
            if (icon == null)
                throw new ArgumentNullException("icon");
            if (action == null)
                throw new ArgumentNullException("action");

            this.title = title;
            this.description = description;
            this.icon = icon;
            this.action = action;

            this.executeCommand = new RelayCommand(this.action);
        }

        public string Title
        {
            get { return this.title; }
        }

        public string Description
        {
            get { return this.description; }
        }

        public Uri Icon
        {
            get { return this.icon; }
        }

        public Action Action
        {
            get { return this.action; }
        }

        public ICommand ExecuteCommand
        {
            get { return this.executeCommand; }
        }
    }
}
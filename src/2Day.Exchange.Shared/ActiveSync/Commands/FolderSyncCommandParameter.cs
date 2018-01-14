using System;
using System.Text;
using Chartreuse.Today.Exchange.Shared.Commands;

namespace Chartreuse.Today.Exchange.ActiveSync.Commands
{
    internal class FolderSyncCommandParameter : IRequestParameterBuilder
    {
        public string SyncKey { get; private set; }

        public FolderSyncCommandParameter(string syncKey)
        {
            if (string.IsNullOrEmpty(syncKey))
                throw new ArgumentNullException("syncKey");

            this.SyncKey = syncKey;
        }

        public string BuildXml(string command)
        {
            var builder = new StringBuilder();

            builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            builder.Append("<FolderSync xmlns=\"FolderHierarchy\">");
            builder.Append("    <SyncKey>{0}</SyncKey>");
            builder.Append("</FolderSync>");

            return string.Format(builder.ToString(), this.SyncKey);
        }
    }
}

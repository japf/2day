using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Exchange.Model;

namespace Chartreuse.Today.Exchange.ActiveSync.Commands
{
    public class FolderSyncCommandResult : ASResponseParserBase
    {
        public string SyncKey { get; private set; }
        public string Status { get; private set; }

        public List<ExchangeFolder> AddedFolders { get; private set; }
        public List<ExchangeFolder> ModifiedFolders { get; private set; }
        public List<ExchangeFolder> DeletedFolders { get; private set; }

        public FolderSyncCommandResult()
        {
            this.AddedFolders = new List<ExchangeFolder>();
            this.ModifiedFolders = new List<ExchangeFolder>();
            this.DeletedFolders = new List<ExchangeFolder>();
        }

        protected override void ParseResponseCore(string commandName, XDocument document, WebRequestResponse response)
        {
            XElement folderElement = document.Element("FolderSync");
            if (folderElement != null)
            {
                XElement keyElement = folderElement.Element("SyncKey");
                if (keyElement != null)
                    this.SyncKey = keyElement.Value;

                XElement statusElement = folderElement.Element("Status");
                if (statusElement != null)
                    this.Status = statusElement.Value;

                FillFolderList(document, "Add", this.AddedFolders);
                FillFolderList(document, "Update", this.ModifiedFolders);
                FillFolderList(document, "Delete", this.DeletedFolders);
            }
        }

        private static void FillFolderList(XDocument xdoc, string tag, List<ExchangeFolder> folders)
        {
            var elements =
                (from folder in xdoc.Element("FolderSync").Elements("Changes").Elements(tag)
                 select new ExchangeFolder()
                 {
                     DisplayName = folder.Element("DisplayName").Value,
                     ServerId = folder.Element("ServerId").Value,
                     ParentId = folder.Element("ParentId").Value,
                     FolderType = int.Parse(folder.Element("Type").Value)
                 }).ToList();

            folders.AddRange(elements);
        }
    }
}

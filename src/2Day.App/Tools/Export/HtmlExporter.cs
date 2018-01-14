using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Chartreuse.Today.App.Shared.ViewModel;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Model.Groups;

namespace Chartreuse.Today.App.Tools.Export
{
    public static class HtmlExporter
    {
        public static async Task PrepareExport(DataRequest request, FolderItemViewModel folderItem)
        {
            DataRequestDeferral deferral = request.GetDeferral();

            var folder = await Package.Current.InstalledLocation.GetFolderAsync("Tools");
            folder = await folder.GetFolderAsync("Export");
            var file = await folder.GetFileAsync("HtmlTemplate.html");
            var template = await FileIO.ReadTextAsync(file);

            template = Template(template, "folderName", () => folderItem.Name);
            template = RepeatTemplate(template, "tasks", folderItem.SmartCollection.Items);

            request.Data.Properties.Title = string.Format("2Day - {0}", folderItem.Name);
            request.Data.SetHtmlFormat(HtmlFormatHelper.CreateHtmlFormat(template));

            deferral.Complete(); 
        }

        private static string Template(string template, string key, Func<string> replaceHandler)
        {
            return template.Replace("{{" + key + "}}", replaceHandler());
        }


        private static string ImageTemplate(string template, string key, string imagePath, DataRequest request)
        {
            string fullPath = "ms-appx://" + imagePath;
            string imageName = Path.GetFileName(imagePath);

            request.Data.ResourceMap[imageName] = RandomAccessStreamReference.CreateFromUri(SafeUri.Get(fullPath));

            return template.Replace("{{" + key + "}}", imageName);
        }

        private static string RepeatTemplate(string template, string key, IEnumerable<Group<ITask>> groups)
        {
            string startTag = "{{begin-" + key + "}}";
            int startIndex = template.IndexOf(startTag);
            string endTag = "{{end-" + key + "}}";
            int endIndex = template.IndexOf(endTag) + endTag.Length;

            string repeatTemplate = template.Substring(startIndex, endIndex - startIndex);
            template = template.Replace(repeatTemplate, string.Empty);

            repeatTemplate = repeatTemplate.Replace(startTag, string.Empty).Replace(endTag, string.Empty);

            StringBuilder repeats = new StringBuilder();
            foreach (var group in groups)
            {
                string groupTemplate = repeatTemplate;
                groupTemplate = Template(groupTemplate, "title", () => "<b>" + group.Title + "</b>");
                groupTemplate = Template(groupTemplate, "folder", () => string.Empty);
                groupTemplate = Template(groupTemplate, "due", () => string.Empty);
                groupTemplate = Template(groupTemplate, "priority", () => string.Empty);
                groupTemplate = Template(groupTemplate, "note", () => string.Empty);
                groupTemplate = Template(groupTemplate, "completed", () => string.Empty);

                repeats.Append(groupTemplate);

                foreach (var task in group)
                {
                    string taskTemplate = repeatTemplate;
                    string dueDate;
                    if (task.Due.HasValue)
                        dueDate = string.Format("{0} {1}", task.Due.Value.ToString("ddd"), task.Due.Value.ToString("d"));
                    else
                        dueDate = string.Empty;

                    taskTemplate = Template(taskTemplate, "title", () => task.Title);
                    taskTemplate = Template(taskTemplate, "folder", () => task.Folder.Name);
                    taskTemplate = Template(taskTemplate, "due", () => dueDate);
                    taskTemplate = Template(taskTemplate, "priority", () => task.Priority.ToString());
                    taskTemplate = Template(taskTemplate, "note", () => task.Note);
                    taskTemplate = Template(taskTemplate, "completed", () => task.IsCompleted ? "checked" : string.Empty);

                    repeats.Append(taskTemplate);
                }
            }

            string value = repeats.ToString();
            template = template.Insert(startIndex, value);

            return template;
        }
    }
}

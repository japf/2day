using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Core.Shared.Tools.Extensions;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Model;
using Newtonsoft.Json;

namespace Chartreuse.Today.ToodleDo
{
    public class ToodleDoService
    {
        public const int ErrorIdFolderAlreadyExists = 5;

        private const int MaxDeletedTasksCount = 50;
        private const int MaxLengthUri = 8200;
        private const int GetTaskPaginationCount = 1000;

        private readonly Func<string> getKey;
        private readonly string appId;
        private readonly string serverUrl;
        private bool isCanceled;

        private string Key
        {
            get { return this.getKey(); }
        }

        public event EventHandler<EventArgs<Exception>>  OnWebException;
        public event EventHandler<EventArgs<ToodleDoApiCallResult>> OnApiError;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToodleDoService"/> class.
        /// </summary>
        /// <param name="getKey">A function to get the key that must be used to call the ToodleDo API. Key is obtained
        /// during the login procedure.</param>
        /// <param name="appId">The application ID</param>
        public ToodleDoService(Func<string> getKey, string appId)
        {
            if (getKey == null)
                throw new ArgumentNullException("getKey");
            if (appId == null)
                throw new ArgumentNullException("appId");

            this.serverUrl = ToodleDoConstants.ServerUrl;
            this.getKey = getKey;
            this.appId = appId;
        }

        public void Cancel()
        {
            this.isCanceled = true;
        }

        #region Task management

        public async Task<string> AddTask(ToodleDoTask toodleDoTask)
        {
            this.LogDebugFormat("Adding tasks {0}", toodleDoTask.Title);

            string apiCall = null;

            TaskChangeRequest request = this.GetTaskChangeRequest(toodleDoTask, TaskProperties.All, false);
            apiCall = string.Format("{0}/tasks/add.php?key={1};tasks=[{2}];fields={3};f=xml;t={4}", this.serverUrl, this.Key, request.JsonData, request.Options, DateTime.UtcNow.Ticks);

            if (apiCall.Length > MaxLengthUri && toodleDoTask.Note != null)
            {
                // ToodleDo sends an error if the whole uri is more than 8 200 characters
                while (apiCall.Length > MaxLengthUri)
                {
                    toodleDoTask.Note = toodleDoTask.Note.Substring(0, toodleDoTask.Note.Length - 100);
                    request = this.GetTaskChangeRequest(toodleDoTask, TaskProperties.All, false);
                    apiCall = string.Format("{0}/tasks/add.php?key={1};tasks=[{2}];fields={3};f=xml;t={4}", this.serverUrl, this.Key, request.JsonData, request.Options, DateTime.UtcNow.Ticks);
                }
            }

            var result = await this.DownloadDataAsync(apiCall);
            if (result.HasError || !result.Document.Descendants("tasks").Any())
                return null;

            string id = null;

            List<string> ids =  result.Document.Descendants("task").Select(x => x.Element("id").Value).ToList();
            if (ids.Count > 0 && ids[0] != "-1")
                id = ids[0];
            else
                this.LogDebugFormat("Error while adding task {0}", toodleDoTask.Title);

            return id;
        }

        public async Task DeleteTask(string taskId)
        {
            await this.DeleteTasks(new[] { taskId });
        }

        public async Task DeleteTasks(IEnumerable<string> taskIds)
        {
            if (!taskIds.Any())
                return;

            this.LogDebugFormat("Deleting tasks {0}", taskIds.Select(i => i.ToString()).Aggregate((a, b) => a + "," + b));

            var tasks = taskIds.Distinct().ToList();
            if (tasks.Count > MaxDeletedTasksCount)
            {
                int group = tasks.Count/MaxDeletedTasksCount + 1;
                for (int i = 0; i < group; i++)
                {
                    await this.DeleteTasksCore(tasks.Skip(MaxDeletedTasksCount*i).Take(MaxDeletedTasksCount));
                }
            }
            else
            {
                await this.DeleteTasksCore(tasks);
            }
        }

        private async Task DeleteTasksCore(IEnumerable<string> taskIds)
        {
            if (!taskIds.Any())
                return;

            this.LogDebugFormat("Deleting tasks {0}", taskIds.Select(i => i.ToString()).Aggregate((a, b) => a + "," + b));

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.None;

                jsonWriter.WriteStartArray();
                foreach (var taskId in taskIds)
                {
                    jsonWriter.WriteValue(taskId);
                }
                jsonWriter.WriteEnd();
            }

            var data = sb.ToString();

            string apiCall = string.Format("{0}/tasks/delete.php?key={1};tasks={2};f=xml;t={3}", this.serverUrl, this.Key, data, DateTime.UtcNow.Ticks);

            await this.DownloadDataAsync(apiCall);
        }

        public async Task<bool> UpdateTask(string id, ToodleDoTask toodleDoTask, TaskProperties properties)
        {
            // if only the "Modified" property has changed, do nothing
            if (properties == TaskProperties.Modified)
                return true;

            var request = this.GetTaskChangeRequest(toodleDoTask, properties, true);

            string apiCall = null;
            if (string.IsNullOrEmpty(request.Options))
            {
                apiCall = string.Format("{0}/tasks/edit.php?key={1};tasks=[{2}];f=xml;t={3}",
                    this.serverUrl, this.Key, request.JsonData, DateTime.UtcNow.Ticks);
            }
            else
            {
                apiCall = string.Format("{0}/tasks/edit.php?key={1};tasks=[{2}];fields={3};f=xml;t={4}",
                    this.serverUrl, this.Key, request.JsonData, request.Options, DateTime.UtcNow.Ticks);
            }

            var result = await this.DownloadDataAsync(apiCall);

            if (result.HasError)
                return false;

            return true;
        }

        public async Task<List<ToodleDoTask>> GetTasks(bool nonCompletedOnly, int? afterTimestamp = null, int? startIndex = null)
        {
            this.LogDebug("Getting tasks");

            // comp : Boolean (0 or 1). Set to 0 to find only uncompleted tasks. Set to 1 to find only completed tasks. 
            // Omit variable, or set to -1 to retrieve both completed and uncompleted tasks.
            int comp = nonCompletedOnly ? 0 : -1;

            string apiCall = string.Format("{0}/tasks/get.php?key={1};comp={2};fields=folder,context,priority,added,duedate,duetime,startdate,starttime,note,tag,repeat,repeatfrom,parent;f=xml;t={3}", this.serverUrl, this.getKey(), comp, DateTime.UtcNow.Ticks);
            if (afterTimestamp != null && afterTimestamp > 0)
            {
                // remove 60 seconds to make sure we grab all tasks
                // if a task is edited at timestamp X, and we set modAfter to X, ToodleDo'll not sent it to us
                // this trick make sure we take it
                apiCall += string.Format(";modafter={0}", afterTimestamp - 60);
            }
            if (startIndex.HasValue)
            {
                apiCall += string.Format(";start={0}", startIndex);
            }

            var apiCallResult = await this.DownloadDataAsync(apiCall);

            var result = new List<ToodleDoTask>();

            if (!apiCallResult.HasError)
            {
                var tasks = apiCallResult.Document.Descendants("task").Select(x => new ToodleDoTask(x)).ToList();
                if (tasks.Count == GetTaskPaginationCount && !startIndex.HasValue)
                {
                    foreach (var task in tasks)
                    {
                        result.Add(task);
                    }

                    // do more requests to fetch other tasks
                    int iteration = 1;
                    bool done = false;
                    do
                    {
                        var moreTasks = await this.GetTasks(nonCompletedOnly, afterTimestamp, GetTaskPaginationCount * iteration);
                        if (moreTasks.Count < GetTaskPaginationCount)
                        {
                            done = true;
                        }
                        else
                        {
                            iteration++;
                        }

                        foreach (var task in moreTasks)
                        {
                            result.Add(task);
                        }

                    } while (!done && iteration < 10);
                }
                else
                {
                    foreach (var task in tasks)
                    {
                        result.Add(task);
                    }
                }
            }

            return result;
        }

        public async Task<List<string>> GetDeletedTasks(int? afterTimestamp = null)
        {
            this.LogDebug("Getting deleted tasks");

            string apiCall = string.Format("{0}/tasks/deleted.php?key={1};f=xml;t={2}", this.serverUrl, this.getKey(), DateTime.UtcNow.Ticks);
            if (afterTimestamp != null && afterTimestamp > 0)
            {
                // remove 60 seconds to make sure we grab all tasks
                // if a task is edited at timestamp X, and we set modAfter to X, ToodleDo'll not sent it to us
                // this trick make sure we take it
                apiCall += string.Format(";modafter={0}", afterTimestamp - 60);
            }

            var result = await this.DownloadDataAsync(apiCall);

            if (result.HasError)
                return new List<string>();

            return result.Document.Descendants("task").Select(x => x.Element("id").Value).ToList();
        }

        #endregion

        #region Folder management

        public async Task<string> AddFolder(string name)
        {
            this.LogDebugFormat("Adding folder {0}", name);

            string apiCall = string.Format("{0}/folders/add.php?name={1};key={2};f=xml;t={3}", this.serverUrl, name, this.Key, DateTime.UtcNow.Ticks);
            string id = null;

            var result = await this.DownloadDataAsync(apiCall);

            if (result.ErrorId == ErrorIdFolderAlreadyExists)
            {
                // try to find existing folder
                var folders = await this.GetFolders();
                if (!folders.HasError && folders.Data != null)
                {
                    var target = folders.Data.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (target != null)
                        id = target.Id;
                }
            }
            else
            {
                if (result.HasError || !result.Document.Descendants("folders").Any())
                    return null;

                List<string> ids = result.Document.Descendants("folder").Select(x => x.Element("id").Value).ToList();
                if (ids.Count > 0)
                    id = ids[0];
            }

            return id;
        }

        public async Task<bool> DeleteFolder(string folderId)
        {
            this.LogDebugFormat("Deleting folder {0}", folderId);

            string apiCall = string.Format("{0}/folders/delete.php?id={1};key={2};f=xml;t={3}", this.serverUrl, folderId, this.Key, DateTime.UtcNow.Ticks);

            var result = await this.DownloadDataAsync(apiCall);
            if (result.HasError)
                return false;

            return true;
        }

        public async Task<bool> UpdateFolder(string folderId, string newName)
        {
            this.LogDebugFormat("Updating folder {0} new name {1}", folderId, newName);

            string apiCall = string.Format("{0}/folders/edit.php?id={1};name={2};key={3};f=xml;t={4}", this.serverUrl, folderId, Escape(newName), this.Key, DateTime.UtcNow.Ticks);

            var result = await this.DownloadDataAsync(apiCall);
            if (result.HasError)
                return false;

            return true;
        }

        public async Task<ToodleDoResponse<IList<ToodleDoFolder>>> GetFolders()
        {
            this.LogDebug("Getting folders");

            string apiCall = string.Format("{0}/folders/get.php?key={1};f=xml;t={2}", this.serverUrl, this.getKey(), DateTime.UtcNow.Ticks);

            var result = await this.DownloadDataAsync(apiCall);

            if (result.HasError)
                return new ToodleDoResponse<IList<ToodleDoFolder>>(true, result.Error);

            var folders = from c in result.Document.Descendants("folder")
                   select new ToodleDoFolder
                   {
                       Id = c.Element("id").Value,
                       Private = (bool)c.Element("private"),
                       Archived = (bool)c.Element("archived"),
                       Name = c.Element("name").Value
                   };

            return new ToodleDoResponse<IList<ToodleDoFolder>>(folders.Where(f => !f.Archived).ToList());
        }

        #endregion

        #region Context management

        public async Task<string> AddContext(string name)
        {
            this.LogDebugFormat("Adding context {0}", name);

            string apiCall = string.Format("{0}/contexts/add.php?name={1};key={2};f=xml;t={3}", this.serverUrl, Escape(name), this.Key, DateTime.UtcNow.Ticks);
            string id = null;

            var result = await this.DownloadDataAsync(apiCall);

            if (result.ErrorId == ErrorIdFolderAlreadyExists)
            {
                // try to find existing context
                var contexts = await this.GetContexts();
                if (!contexts.HasError && contexts.Data != null)
                {
                    var target = contexts.Data.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (target != null)
                        id = target.Id;
                }
            }
            else
            {
                if (result.HasError || !result.Document.Descendants("contexts").Any())
                    return null;

                List<string> ids = result.Document.Descendants("context").Select(x => x.Element("id").Value).ToList();
                if (ids.Count > 0)
                    id = ids[0];
            }
            return id;
        }

        public async Task<bool> DeleteContext(string contextId)
        {
            this.LogDebugFormat("Deleting context {0}", contextId);

            string apiCall = string.Format("{0}/contexts/delete.php?id={1};key={2};f=xml;t={3}", this.serverUrl, contextId, this.Key, DateTime.UtcNow.Ticks);

            var result = await this.DownloadDataAsync(apiCall);
            if (result.HasError)
                return false;

            return true;
        }

        public async Task<bool> UpdateContext(string contextId, string newName)
        {
            this.LogDebugFormat("Updating context {0} new name {1}", contextId, newName);

            string apiCall = string.Format("{0}/contexts/edit.php?id={1};name={2};key={3};f=xml;t={4}", this.serverUrl, contextId, Escape(newName), this.Key, DateTime.UtcNow.Ticks);

            var result = await this.DownloadDataAsync(apiCall);
            if (result.HasError)
                return false;

            return true;
        }

        public async Task<ToodleDoResponse<IList<ToodleDoContext>>> GetContexts()
        {
            this.LogDebug("Getting contexts");

            string apiCall = string.Format("{0}/contexts/get.php?key={1};f=xml;t={2}", this.serverUrl, this.getKey(), DateTime.UtcNow.Ticks);

            var result = await this.DownloadDataAsync(apiCall);

            if (result.HasError)
                return new ToodleDoResponse<IList<ToodleDoContext>>(true, result.Error);

            var folders = from c in result.Document.Descendants("context")
                          select new ToodleDoContext
                          {
                              Id = c.Element("id").Value,
                              Name = c.Element("name").Value
                          };

            return new ToodleDoResponse<IList<ToodleDoContext>>(folders.ToList());
        }

        #endregion

        #region Account management

        public async Task<ToodleDoResponse<string>> GetUserId(string sig, string email, string password)
        {
            this.LogDebugFormat("Getting user id email {0} password ******({1})", email, password.Length);

            string apiCall = string.Format("{0}/account/lookup.php?appid={1};sig={2};email={3};pass={4};f=xml;t={5}",
                this.serverUrl, this.appId, sig, Escape(email), Escape(password), DateTime.UtcNow.Ticks);

            var result = await this.DownloadDataAsync(apiCall, password);

            if (result.HasError)
                return new ToodleDoResponse<string>(true, result.Error);

            return new ToodleDoResponse<string>(result.Document.GetElement("userid"));
        }

        public async Task<ToodleDoResponse<string>> GetToken(string userId, string sig)
        {
            this.LogDebug("Getting token");

            string apiCall = string.Format("{0}/account/token.php?userid={1};appid={2};sig={3};f=xml;t={4}",
                this.serverUrl, userId, this.appId, sig, DateTime.UtcNow.Ticks);

            var result = await this.DownloadDataAsync(apiCall);

            if (result.HasError)
                return new ToodleDoResponse<string>(true, result.Error);

            return new ToodleDoResponse<string>(result.Document.GetElement("token"));
        }

        public async Task<ToodleDoResponse<ToodleDoAccount>> GetAccountInfo()
        {
            this.LogDebug("Getting account info");

            string apiCall = string.Format("{0}/account/get.php?f=xml;key={1};t={2}", this.serverUrl, this.getKey(), DateTime.UtcNow.Ticks);

            var result = await this.DownloadDataAsync(apiCall);

            if (result.HasError)
                return new ToodleDoResponse<ToodleDoAccount>(true, result.Error);

            XElement account = result.Document.Descendants("account").First();

            return new ToodleDoResponse<ToodleDoAccount>(new ToodleDoAccount
                (
                    account.GetElement("userid"),
                    int.Parse(account.GetElement("lastedit_folder")),
                    int.Parse(account.GetElement("lastedit_context")),
                    int.Parse(account.GetElement("lastedit_task")),
                    int.Parse(account.GetElement("lastdelete_task")))
                );
        }

        #endregion

        private TaskChangeRequest GetTaskChangeRequest(ToodleDoTask toodleDoTask, TaskProperties properties, bool includeId)
        {
            var request = new TaskChangeRequest();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.None;
                jsonWriter.WriteStartObject();

                if (includeId)
                {
                    jsonWriter.WritePropertyName("id");
                    jsonWriter.WriteValue(toodleDoTask.Id);
                }

                if ((properties & TaskProperties.Title) == TaskProperties.Title)
                {
                    jsonWriter.WritePropertyName("title");
                    jsonWriter.WriteValue(toodleDoTask.Title);
                }

                if ((properties & TaskProperties.Note) == TaskProperties.Note)
                {
                    string note = string.Empty;
                    if (!string.IsNullOrEmpty(toodleDoTask.Note))
                        note = toodleDoTask.Note;

                    jsonWriter.WritePropertyName("note");
                    jsonWriter.WriteValue(note);
                    request.Options += "note,";
                }

                if ((properties & TaskProperties.Tags) == TaskProperties.Tags)
                {
                    if (!string.IsNullOrEmpty(toodleDoTask.Tags))
                    {
                        jsonWriter.WritePropertyName("tag");
                        jsonWriter.WriteValue(toodleDoTask.Tags);
                        request.Options += "tag,";
                    }
                }

                if ((properties & TaskProperties.Folder) == TaskProperties.Folder)
                {
                    jsonWriter.WritePropertyName("folder");
                    jsonWriter.WriteValue(toodleDoTask.FolderId);
                    request.Options += "folder,";
                }

                if ((properties & TaskProperties.Context) == TaskProperties.Context)
                {
                    string contextId = toodleDoTask.ContextId;
                    if (string.IsNullOrEmpty(contextId))
                        contextId = "0";

                    jsonWriter.WritePropertyName("context");
                    jsonWriter.WriteValue(contextId);
                    request.Options += "context,";
                }

                if ((properties & TaskProperties.Due) == TaskProperties.Due)
                {
                    int duedate = 0;

                    if (toodleDoTask.Due.HasValue)
                    {
                        duedate = toodleDoTask.Due.Value.DateTimeToTimestamp();
                    }

                    jsonWriter.WritePropertyName("duedate");
                    jsonWriter.WriteValue(duedate);

                    request.Options += "duedate,";
                }

                if ((properties & TaskProperties.Start) == TaskProperties.Start)
                {
                    int startdate = 0;
                    int starttime = 0;

                    if (toodleDoTask.Start.HasValue)
                    {
                        startdate = toodleDoTask.Start.Value.Date.DateTimeToTimestamp();
                        starttime = (int)toodleDoTask.Start.Value.TimeOfDay.TotalSeconds;
                    }

                    jsonWriter.WritePropertyName("startdate");
                    jsonWriter.WriteValue(startdate);

                    request.Options += "starttime,";

                    jsonWriter.WritePropertyName("starttime");
                    jsonWriter.WriteValue(starttime);

                    request.Options += "starttime,";
                }

                if ((properties & TaskProperties.Frequency) == TaskProperties.Frequency)
                {

                    jsonWriter.WritePropertyName("repeat");
                    jsonWriter.WriteValue(toodleDoTask.Repeat);

                    request.Options += "repeat,";

                }

                if ((properties & TaskProperties.RepeatFrom) == TaskProperties.RepeatFrom)
                {
                    jsonWriter.WritePropertyName("repeatfrom");
                    jsonWriter.WriteValue(toodleDoTask.RepeatFrom);

                    request.Options += "repeatfrom,";
                }

                if ((properties & TaskProperties.Completed) == TaskProperties.Completed)
                {
                    if (toodleDoTask.Completed.HasValue)
                    {
                        jsonWriter.WritePropertyName("completed");
                        jsonWriter.WriteValue(toodleDoTask.Completed.Value.DateTimeToTimestamp());
                    }
                    else
                    {
                        jsonWriter.WritePropertyName("completed");
                        jsonWriter.WriteValue(0);
                    }
                }

                if ((properties & TaskProperties.Added) == TaskProperties.Added)
                {
                    jsonWriter.WritePropertyName("added");
                    jsonWriter.WriteValue(toodleDoTask.Added.DateTimeToTimestamp());

                    request.Options += "added,";
                }

                if ((properties & TaskProperties.Priority) == TaskProperties.Priority)
                {
                    jsonWriter.WritePropertyName("priority");
                    jsonWriter.WriteValue(toodleDoTask.Priority);

                    request.Options += "priority,";
                }

                if ((properties & TaskProperties.Parent) == TaskProperties.Parent)
                {
                    jsonWriter.WritePropertyName("parent");
                    jsonWriter.WriteValue(toodleDoTask.ParentId);

                    request.Options += "parent,";
                }

                jsonWriter.WriteEndObject();
            }

            request.JsonData = Escape(sb.ToString());

            return request;
        }

        private void LogDebugFormat(string message, params object[] args)
        {
            LogService.LogFormat("ToodleDoService", message, args);
        }

        private void LogDebug(string message)
        {
            LogService.Log("ToodleDoService", message);
        }

        private static string Escape(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            input = input.EscapeLongDataString();

            input = input.Replace(";", "%3B");
            input = input.Replace("&", "%26");
            input = input.Replace("#", "%23");
            input = input.Replace("+", "%2B");
            return input;
        }

        private ToodleDoApiCallResult ProcessResponse(string apiCall, string apiCallResult)
        {
            XDocument xDocument;
            try
            {
                xDocument = XDocument.Load(new StringReader(apiCallResult));
                XElement error = xDocument.Descendants("error").FirstOrDefault();
                if (error != null)
                {
                    int id = -1;
                    XAttribute idAttribute = error.Attribute("id");
                    if (idAttribute != null && !string.IsNullOrEmpty(idAttribute.Value))
                        int.TryParse(idAttribute.Value, out id);

                    var result = new ToodleDoApiCallResult(apiCall, error.Value, id);
                    this.OnApiError.Raise(this, new EventArgs<ToodleDoApiCallResult>(result));

                    return result;
                }
            }
            catch (Exception e)
            {
                this.OnApiError.Raise(this, new EventArgs<ToodleDoApiCallResult>(new ToodleDoApiCallResult(apiCall, string.Format("{0} exception: {1}", apiCallResult, e.Message))));

                return new ToodleDoApiCallResult(apiCall, e.Message);
            }

            return new ToodleDoApiCallResult(xDocument);
        }

        private async Task<ToodleDoApiCallResult> DownloadDataAsync(string apiCall, string password = null)
        {
            if (this.isCanceled)
            {
                this.isCanceled = false;
                throw new OperationCanceledException();
            }

            try
            {
                var requestBuilder = new WebRequestDefaultBuilder();
                
                string response = await requestBuilder.GetAsync(apiCall);

                string message = apiCall;
                if (!string.IsNullOrEmpty(password))
                    message = apiCall.Replace(password, "***password***");
                if (!string.IsNullOrEmpty(this.Key))
                    message = apiCall.Replace(this.Key, "***key***");

                this.LogDebugFormat("ToodleDoService: launching request to: {0}", message);

                return this.ProcessResponse(apiCall, response);
            }
            catch (Exception e)
            {
                if (this.OnWebException != null)
                    this.OnWebException(this, new EventArgs<Exception>(e));

                return new ToodleDoApiCallResult(apiCall, e.Message);
            }
        }

        private struct TaskChangeRequest
        {
            public string JsonData { get; set; }
            public string Options { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Core.Shared.Tools.Tracking;
using Chartreuse.Today.Vercors.Shared;
using Chartreuse.Today.Vercors.Shared.Model;
using Chartreuse.Today.Vercors.Shared.Resources;
using Microsoft.WindowsAzure.MobileServices;

namespace Chartreuse.Today.Shared.Sync.Vercors
{
    internal class VercorsService : IVercorsService
    {
        private const string LogSource = "VercorsService";
        private const int TaskRequestPageSize = 500;
        private readonly MobileServiceClient mobileService;

        private string loginInfo;
        private bool isCanceled;
        private Task<bool> loginTask;

        public string LoginInfo
        {
            get { return this.loginInfo; }
        }

        public VercorsService()
        {
            this.loginInfo = Constants.DefaultLoginInfo;
            this.mobileService = new MobileServiceClient(Constants.VercorsServiceUri, Constants.VercorsApplicationKey);
        }

        private void CheckCancellation()
        {
            if (this.isCanceled)
            {
                this.isCanceled = false;
                throw new OperationCanceledException();
            }
        }

        public async Task<string> DeleteAccount()
        {
            try
            {
                // Asynchronously call the custom API using the POST method. 
                var result = await this.mobileService.InvokeApiAsync("deleteAccount", System.Net.Http.HttpMethod.Get, null);
                if (result != null && result["message"] != null && result["message"].ToString().Equals("ok", StringComparison.OrdinalIgnoreCase))
                    return null;
                else if (result != null && result["message"] != null)
                    return result["message"].ToString();
                else
                    return "unkown error";
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return e.Message;
            }
        }

        public async Task<VercorsTask> AddTask(VercorsTask vercorsTask)
        {
            this.CheckCancellation();

            try
            {
                vercorsTask.Identifier = Guid.NewGuid().ToString().Replace("-", "");
                vercorsTask.ItemId = 0;

                await this.mobileService.GetTable<VercorsTask>().InsertAsync(vercorsTask);

                var result = await this.mobileService.GetTable<VercorsTask>().Where(t => t.Identifier == vercorsTask.Identifier).ToListAsync();
                if (result.Count == 1)
                    return result[0];
                else
                    return null;
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<IEnumerable<VercorsTask>> AddTasks(IEnumerable<VercorsTask> vercorsTasks)
        {
            var result = new List<VercorsTask>();
            foreach (var vercorsTask in vercorsTasks)
            {
                var addedTask = await this.AddTask(vercorsTask);
                result.Add(addedTask);
            }

            return result;
        }

        public async Task<IEnumerable<VercorsTask>> DeleteTasks(IEnumerable<VercorsTask> vercorsTasks)
        {
            if (!vercorsTasks.Any())
                return Enumerable.Empty<VercorsTask>();

            this.CheckCancellation();

            try
            {
                var ids = vercorsTasks.Select(t => t.ItemId).Distinct().ToList();
                List<int> deletedIds = await this.mobileService.InvokeApiAsync<List<int>, List<int>>("tasks/delete", ids);
                return new List<VercorsTask>(vercorsTasks.Where(t => deletedIds.Contains(t.ItemId)));
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<VercorsTask> UpdateTask(VercorsTask vercorsTask)
        {
            this.CheckCancellation();

            try
            {
                vercorsTask.UserId = this.mobileService.CurrentUser.UserId;

                await this.mobileService.GetTable<VercorsTask>().UpdateAsync(vercorsTask);

                var result = await this.mobileService.GetTable<VercorsTask>().Where(t => t.ItemId == vercorsTask.ItemId).ToListAsync();
                if (result.Count == 1)
                    return result[0];
                else
                    return null;
            }
            catch (Exception e)
            {
                //remove from collection is Task is not found
                //add integration test !


                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<IEnumerable<VercorsTask>> UpdateTasks(IEnumerable<VercorsTask> vercorsTasks)
        {
            var result = new List<VercorsTask>();
            foreach (var vercorsTask in vercorsTasks)
            {
                var editedTask = await this.UpdateTask(vercorsTask);
                result.Add(editedTask);
            }

            return result;
        }

        public async Task<List<VercorsTask>> GetTasks(long? afterTimestamp = null)
        {
            this.CheckCancellation();
            List<VercorsTask> result = null;

            try
            {
                if (afterTimestamp == null)
                    result = new List<VercorsTask>(await this.mobileService.GetTable<VercorsTask>().ToCollectionAsync(TaskRequestPageSize));
                else
                    result = new List<VercorsTask>(await this.mobileService.GetTable<VercorsTask>().Where(t => t.EditTimestamp >= afterTimestamp).ToCollectionAsync(TaskRequestPageSize));
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }

            if (result != null)
            {
                foreach (var vercorsTask in result)
                {
                    if (string.IsNullOrWhiteSpace(vercorsTask.Title))
                        vercorsTask.Title = "no title";
                }
            }

            return result;
        }

        public async Task<List<VercorsDeletedTask>> GetDeletedTasks(long afterTimestamp)
        {
            this.CheckCancellation();

            try
            {
                return await this.mobileService.GetTable<VercorsDeletedTask>().Where(dt => dt.DeletedTimestamp >= afterTimestamp).ToListAsync();
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<VercorsFolder> AddFolder(VercorsFolder vercorsFolder)
        {
            this.CheckCancellation();

            try
            {
                vercorsFolder.ItemId = 0;

                await this.mobileService.GetTable<VercorsFolder>().InsertAsync(vercorsFolder);

                var result = await this.mobileService.GetTable<VercorsFolder>().Where(t => t.Name == vercorsFolder.Name).ToListAsync();
                if (result.Count == 1)
                    return result[0];
                else
                    return null;
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<bool> DeleteFolder(VercorsFolder vercorsFolder)
        {
            this.CheckCancellation();

            try
            {
                await this.mobileService.GetTable<VercorsFolder>().DeleteAsync(vercorsFolder);

                var result = await this.mobileService.GetTable<VercorsFolder>().Where(t => t.Name == vercorsFolder.Name).ToListAsync();

                return result.Count == 0;
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return false;
            }
        }

        public async Task<VercorsFolder> UpdateFolder(VercorsFolder vercorsFolder)
        {
            this.CheckCancellation();

            try
            {
                vercorsFolder.UserId = this.mobileService.CurrentUser.UserId;

                await this.mobileService.GetTable<VercorsFolder>().UpdateAsync(vercorsFolder);

                var result = await this.mobileService.GetTable<VercorsFolder>().Where(t => t.Name == vercorsFolder.Name).ToListAsync();
                if (result.Count == 1)
                    return result[0];
                else
                    return null;
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());
                return null;
            }
        }

        public async Task<List<VercorsFolder>> GetFolders()
        {
            this.CheckCancellation();

            try
            {
                return await this.mobileService.GetTable<VercorsFolder>().ToListAsync();
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<VercorsContext> AddContext(VercorsContext vercorsContext)
        {
            this.CheckCancellation();

            try
            {
                vercorsContext.ItemId = 0;

                await this.mobileService.GetTable<VercorsContext>().InsertAsync(vercorsContext);

                var result = await this.mobileService.GetTable<VercorsContext>().Where(t => t.Name == vercorsContext.Name).ToListAsync();
                if (result.Count == 1)
                    return result[0];
                else
                    return null;
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<bool> DeleteContext(VercorsContext vercorsContext)
        {
            this.CheckCancellation();

            try
            {
                await this.mobileService.GetTable<VercorsContext>().DeleteAsync(vercorsContext);

                var result = await this.mobileService.GetTable<VercorsContext>().Where(t => t.Name == vercorsContext.Name).ToListAsync();

                return result.Count == 0;
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return false;
            }
        }

        public async Task<VercorsContext> UpdateContext(VercorsContext vercorsContext)
        {
            this.CheckCancellation();

            try
            {
                vercorsContext.UserId = this.mobileService.CurrentUser.UserId;

                await this.mobileService.GetTable<VercorsContext>().UpdateAsync(vercorsContext);

                var result = await this.mobileService.GetTable<VercorsContext>().Where(t => t.Name == vercorsContext.Name).ToListAsync();
                if (result.Count == 1)
                    return result[0];
                else
                    return null;
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<List<VercorsContext>> GetContexts()
        {
            this.CheckCancellation();

            try
            {
                return await this.mobileService.GetTable<VercorsContext>().ToListAsync();
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<VercorsSmartView> AddSmartView(VercorsSmartView vercorsSmartView)
        {
            this.CheckCancellation();

            try
            {
                vercorsSmartView.ItemId = string.Empty;

                await this.mobileService.GetTable<VercorsSmartView>().InsertAsync(vercorsSmartView);

                var result = await this.mobileService.GetTable<VercorsSmartView>().Where(t => t.Name == vercorsSmartView.Name).ToListAsync();
                if (result.Count == 1)
                    return result[0];
                else
                    return null;
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<bool> DeleteSmartView(VercorsSmartView vercorsSmartView)
        {
            this.CheckCancellation();

            try
            {
                await this.mobileService.GetTable<VercorsSmartView>().DeleteAsync(vercorsSmartView);

                var result = await this.mobileService.GetTable<VercorsSmartView>().Where(t => t.Name == vercorsSmartView.Name).ToListAsync();

                return result.Count == 0;
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return false;
            }
        }

        public async Task<VercorsSmartView> UpdateSmartView(VercorsSmartView vercorsSmartView)
        {
            this.CheckCancellation();

            try
            {
                vercorsSmartView.UserId = this.mobileService.CurrentUser.UserId;

                await this.mobileService.GetTable<VercorsSmartView>().UpdateAsync(vercorsSmartView);

                var result = await this.mobileService.GetTable<VercorsSmartView>().Where(t => t.Name == vercorsSmartView.Name).ToListAsync();
                if (result.Count == 1)
                    return result[0];
                else
                    return null;
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<List<VercorsSmartView>> GetSmartViews()
        {
            this.CheckCancellation();
            try
            {
                var result = await this.mobileService.GetTable<VercorsSmartView>().ToListAsync();
                return result;
            }
            catch (Exception e)
            {
                LogService.Log(LogSource, e.ToString());

                return null;
            }
        }

        public async Task<VercorsAccount> GetUserAccount()
        {
            this.CheckCancellation();

            var result = await this.mobileService.GetTable<VercorsAccount>().ToListAsync();

            return result.FirstOrDefault();
        }

        public async Task<VercorsAccount> AddUserAccount(VercorsAccount vercorsAccount)
        {
            this.CheckCancellation();

            await this.mobileService.GetTable<VercorsAccount>().InsertAsync(vercorsAccount);

            var result = await this.mobileService.GetTable<VercorsAccount>().ToListAsync();
            if (result.Count == 1)
                return result[0];
            else
                return null;
        }
        
        public async Task<bool> LoginAsync(bool silent)
        {
            if (this.loginTask == null)
                this.loginTask = this.LoginAsyncCore(silent);

            return await this.loginTask;
        }

        private async Task<bool> LoginAsyncCore(bool silent)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;

            bool success;

            ISettings settings = this.GetSettings();
            if (!string.IsNullOrEmpty(settings.GetValue<string>(CoreSettings.SyncAuthToken)))
            {
                success = await this.TryLogin(settings);

                // retry after getting a new token
                if (!success)
                {
                    await this.GetNewAuthToken(silent, settings);
                    success = await this.TryLogin(settings);
                }
            }
            else
            {
                await this.GetNewAuthToken(silent, settings);
                success = await this.TryLogin(settings);
            }

            this.loginTask = null;

            LogService.Log("Vercors", string.Format("LoginAsyncCore success: {0}", success));
            return success;
        }

        // -------------------------------------------------------------------------
        // PLATFORM SPECIFIC #IF STUFF BELOW
        // -------------------------------------------------------------------------

        private ISettings GetSettings()
        {
#if WINDOWS || WINDOWS_UWP
            return Core.Universal.Model.WinSettings.Instance;
#else
            return Core.Shared.Tests.Impl.TestSettings.Instance;
#endif
        }

        private async Task<bool> TryLogin(ISettings settings)
        {
            try
            {
                string token = settings.GetValue<string>(CoreSettings.SyncAuthToken);
                LogService.Log("Vercors", string.Format("TryLogin token: {0}", (token != null)));

#if !SYNC_VERCORS
                MobileServiceUser loginResult = await this.mobileService.LoginWithMicrosoftAccountAsync(token);
                if (string.IsNullOrEmpty(loginResult.MobileServiceAuthenticationToken))
                    return false;

                if (!string.IsNullOrEmpty(loginResult.UserId))
                    settings.SetValue(CoreSettings.SyncUserId, loginResult.UserId);

#else
                // hack to force this method to be async...
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                MobileServiceUser user = new MobileServiceUser(Chartreuse.Today.Sync.Test.Runners.VercorsTestRunner.UserId) { MobileServiceAuthenticationToken = token };
                this.mobileService.CurrentUser = user;
#endif

                this.loginInfo = string.Format(VercorsResources.Vercors_LoggedAsFormat, settings.GetValue<string>(CoreSettings.SyncFirstName), settings.GetValue<string>(CoreSettings.SyncLastName));

                return true;
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                if (string.IsNullOrEmpty(ex.Message) || !ex.Message.Contains("authentication token has expired"))
                    TrackingManagerHelper.Exception(ex, "Vercors MobileServiceException");

                return false;
            }

        }

#if SYNC_VERCORS || BACKGROUND_AGENT
        // ReSharper disable once UnusedParameter.Local
        private Task<string> GetNewAuthToken(bool silent, ISettings settings)
        {
            return Task.FromResult(settings.GetValue<string>(CoreSettings.SyncAuthToken));
        }
#else
        private async Task<string> GetNewAuthToken(bool silent, ISettings settings)
        {
            var liveIdClient = new Microsoft.Live.LiveAuthClient(Constants.VercorsServiceUri);

            //  add "wl.emails" in the scope to be able to access user email address
            // (readable in the meResult object)
            Microsoft.Live.LiveLoginResult result;
            if (silent)
                result = await liveIdClient.InitializeAsync(new[] { "wl.signin", "wl.offline_access" });
            else
                result = await liveIdClient.LoginAsync(new[] { "wl.signin", "wl.offline_access" });

            if (result.Status == Microsoft.Live.LiveConnectSessionStatus.Connected && result.Session != null && !string.IsNullOrEmpty(result.Session.AuthenticationToken))
            {
                settings.SetValue(CoreSettings.SyncAuthToken, result.Session.AuthenticationToken);

                var client = new Microsoft.Live.LiveConnectClient(result.Session);

#if !BACKGROUND
                client.GetAsync("me").ContinueWith(r =>
                {
                    var me = r.Result;
                    string firstName = me.Result["first_name"] as string;
                    string lastName = me.Result["last_name"] as string;
                    settings.SetValue(CoreSettings.SyncFirstName, firstName ?? string.Empty);
                    settings.SetValue(CoreSettings.SyncLastName, lastName ?? string.Empty);
                }, 
                TaskScheduler.Default);
#endif
                return result.Session.AuthenticationToken;
            }

            return null;
        }
#endif

        public bool Logout()
        {
#if !BACKGROUND_AGENT
            var liveIdClient = new Microsoft.Live.LiveAuthClient(Constants.VercorsServiceUri);

            if (liveIdClient.CanLogout)
            {
                liveIdClient.Logout();
                return true;
            }
            return false;
#else
            return true;
#endif
        }

        public void Cancel()
        {
            this.isCanceled = true;
        }
    }
}

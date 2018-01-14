using System;
using System.Threading.Tasks;
using Chartreuse.Today.Core.Shared.Net;
using Chartreuse.Today.Core.Shared.Tools.Logging;
using Chartreuse.Today.Exchange.Ews;
using Chartreuse.Today.Exchange.Ews.Model;
using Chartreuse.Today.Sync.Test.Runners;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Chartreuse.Today.Sync.Test.Ews
{
    [TestClass]
    public class EwsErrorManagement : EwsTestBase
    {
        private DateTime due;

        [TestMethod]
        public async Task Create_items_invalid_due_date()
        {
            ExchangeEwsTestRunner.FailOnAnyInvalidResponseMessage = false;

            // will have an invalid datetime
            var ewsTask1 = CreateSampleEwsTask();
            this.due = ewsTask1.DueDate.Value;

            var ewsTask2 = CreateSampleEwsTask();
            ewsTask2.DueDate = ewsTask2.DueDate.Value.AddDays(1);

            WebRequestBuilder.InterceptRequest += this.OnRequestIntercepted;

            var result = await this.server.CreateItemAsync(new[] { ewsTask1, ewsTask2});
            
            LogService.Log("result", JsonConvert.SerializeObject(result));

            WebRequestBuilder.InterceptRequest -= this.OnRequestIntercepted;

            Assert.IsNotNull(result, "result != null");
            Assert.IsNotNull(result.Identifiers, "result.Identifiers != null");
            Assert.AreEqual(2, result.Identifiers.Count, 0, "result.Identifiers.Count != 2");
            Assert.IsFalse(result.Identifiers[0].IsValid, "result.Identifiers[0].IsValid != false");
            Assert.IsNotNull(result.Identifiers[0].ErrorMessage, "result.Identifiers[0].ErrorMessage == null");

            Assert.IsTrue(result.Identifiers[1].IsValid, "result.Identifiers[1].IsValid != true");
            Assert.IsNull(result.Identifiers[1].ErrorMessage, "result.Identifiers[1].ErrorMessage == true");

            ExchangeEwsTestRunner.FailOnAnyInvalidResponseMessage = true;
        }

        [TestMethod]
        public async Task Update_items_invalid_due_date()
        {
            ExchangeEwsTestRunner.FailOnAnyInvalidResponseMessage = false;

            // will have an invalid datetime
            var ewsTask1 = CreateSampleEwsTask();
            this.due = ewsTask1.DueDate.Value;

            var ewsTask2 = CreateSampleEwsTask();
            ewsTask2.DueDate = ewsTask2.DueDate.Value.AddDays(1);

            var createResult = await this.server.CreateItemAsync(new[] { ewsTask1, ewsTask2 });
            Assert.IsNotNull(createResult, "createResult != null");
            LogService.Log("createResult", JsonConvert.SerializeObject(createResult));

            Assert.AreEqual(2, createResult.Identifiers.Count);
            Assert.IsNotNull(createResult.Identifiers[0], "createResult != null");
            ewsTask1.Id = createResult.Identifiers[0].Id;
            ewsTask1.ChangeKey = createResult.Identifiers[0].ChangeKey;
            ewsTask2.Id = createResult.Identifiers[0].Id;
            ewsTask2.ChangeKey = createResult.Identifiers[0].ChangeKey;

            WebRequestBuilder.InterceptRequest += this.OnRequestIntercepted;

            ewsTask1.Subject = "new subject 1";
            ewsTask1.Changes = EwsFields.Subject | EwsFields.DueDate;
            ewsTask2.Subject = "new subject 2";
            ewsTask2.Changes = EwsFields.Subject;

            var result = await this.server.UpdateItemsAsync(new[] {ewsTask1, ewsTask2});

            WebRequestBuilder.InterceptRequest -= this.OnRequestIntercepted;

            Assert.IsNotNull(result, "result != null");
            LogService.Log("result", JsonConvert.SerializeObject(result));

            Assert.IsNotNull(result.Identifiers, "result.Identifiers != null");
            Assert.AreEqual(2, result.Identifiers.Count);
            Assert.IsFalse(result.Identifiers[0].IsValid);
            Assert.IsNotNull(result.Identifiers[0].ErrorMessage);

            Assert.IsTrue(result.Identifiers[1].IsValid);
            Assert.IsNull(result.Identifiers[1].ErrorMessage);

            ExchangeEwsTestRunner.FailOnAnyInvalidResponseMessage = true;
        }

        private void OnRequestIntercepted(object s, WebRequestInterceptorEventArgs e)
        {
            string ewsTask1Due = this.due.ToEwsDateTimeValue(DateTimeKind.Local);
            e.RequestBody = e.RequestBody.Replace(ewsTask1Due, "");
        }
    }
}

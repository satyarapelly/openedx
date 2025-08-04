// <copyright file="PimsPaymentInstrumentsController.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Web.Http;
    using Common.Transaction;
    using Common.Web;
    using Test.Common;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class PimsPaymentInstrumentsController : EmulatorBaseController
    {
        public PimsPaymentInstrumentsController() : base(Constants.TestScenarioManagers.PIMS)
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpGet]
        public HttpResponseMessage GetPI([FromUri]string accountId, [FromUri]string piid)
        {
            return this.GetResponse(Constants.PIMSApiName.GetPI);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpGet]
        public HttpResponseMessage GetPIExtendedView([FromUri]string piid, [FromUri] string accountId = null)
        {
            return this.GetResponse(Constants.PIMSApiName.GetPIExtendedView);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpGet]
        public HttpResponseMessage GetSeCardPersos([FromUri]string accountId, [FromUri]string piid, [FromUri]string deviceId)
        {
            return this.GetResponse(Constants.PIMSApiName.GetSeCardPersos);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpGet]
        public HttpResponseMessage GetSessionDetails([FromUri]string accountId, [FromUri]string sessionId)
        {
            return this.GetResponse(Constants.PIMSApiName.GetSessionDetails);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpGet]
        public HttpResponseMessage ListPI([FromUri]string accountId)
        {
            return this.GetResponse(Constants.PIMSApiName.ListPI);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpGet]
        public HttpResponseMessage ListEmpOrgPI()
        {
            return this.GetResponse(Constants.PIMSApiName.ListEmpOrgPI);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage AddPI([FromUri]string accountId)
        {
            return this.GetResponse(Constants.PIMSApiName.AddPI);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage AddPI()
        {
            return this.GetResponse(Constants.PIMSApiName.AddPI);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage ResumeAddPI([FromUri]string accountId, [FromUri]string piid)
        {
            return this.GetResponse(Constants.PIMSApiName.ResumeAddPI);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage UpdatePI([FromUri]string accountId, [FromUri]string piid)
        {
            return this.GetResponse(Constants.PIMSApiName.UpdatePI);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage ReplacePI([FromUri]string accountId, [FromUri]string piid)
        {
            return this.GetResponse(Constants.PIMSApiName.ReplacePI);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage ValidateCvv([FromUri]string accountId, [FromUri]string piid, [FromBody]object requestData)
        {
            return this.GetResponse(Constants.PIMSApiName.ValidateCvv);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage Validate([FromUri]string accountId, [FromUri]string piid, [FromBody]object requestData)
        {
            return this.GetResponse(Constants.PIMSApiName.Validate);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage LinkTransaction([FromUri]string accountId, [FromUri]string piid, [FromBody]object requestData)
        {
            return this.GetResponse(Constants.PIMSApiName.LinkTransaction);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage SearchByAccountNumber([FromBody] object requestData)
        {
            return this.GetResponse(Constants.PIMSApiName.SearchByAccountNumber);
        }

        protected override HttpResponseMessage GetResponse(string apiName)
        {
            TestContext testContext = null;
            this.Request.TryGetTestContext(out testContext);
            for (int i = 0; i < testContext.ScenarioList.Count; i++)
            {
                string testScenarioName = testContext.ScenarioList[i];

                if (testScenarioName.StartsWith(Constants.TestScenarios.PX) && !testScenarioName.StartsWith(Constants.TestScenarios.PXAccount)
                    && !testScenarioName.StartsWith(Constants.TestScenarios.PXPims) && !testScenarioName.StartsWith(Constants.TestScenarios.PXIssuerService))
                {
                    string targetTestScenarioName = testScenarioName.Replace(Constants.TestScenarios.PX, Constants.TestScenarios.PXPims);
                    testContext.ReplaceTestHeader(testScenarioName, targetTestScenarioName);
                }
            }

            return TestScenarioManager.GetResponse(apiName, testContext);
        }
    }
}
// <copyright file="FraudDetectionController.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Web.Http;
    using Common.Web;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Test.Common;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class FraudDetectionController : ApiController
    {
        private TestScenarioManager TestScenarioManager
        {
            get
            {
                return this.Configuration.GetTestScenarioManager(Constants.TestScenarioManagers.FraudDetection);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage BotCheck(object paylod)
        {
            return this.GetResponse(Constants.FraudDetectionApiName.BotCheck);
        }

        protected HttpResponseMessage GetResponse(string apiName)
        {
            TestContext testContext = null;
            this.Request.TryGetTestContext(out testContext);

            return TestScenarioManager.GetResponse(apiName, testContext);
        }
    }
}
// <copyright file="FraudDetectionController.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Common.Web;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Test.Common;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class FraudDetectionController : ControllerBase
    {
        private TestScenarioManager TestScenarioManager
        {
            get
            {
                return this.HttpContext.RequestServices.GetTestScenarioManager(Constants.TestScenarioManagers.FraudDetection);
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
// <copyright file="PimsPaymentMethodsController.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Common.Transaction;
    using Common.Web;
    using Test.Common;
    using Microsoft.Commerce.Payments.Common.Testing;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Extensions;

    public class PimsPaymentMethodsController : EmulatorBaseController
    {
        public PimsPaymentMethodsController() : base(Constants.TestScenarioManagers.PIMS)
        {
        }

        [HttpGet]
        public HttpResponseMessage GetPM()
        {
            return this.GetResponse(Constants.PIMSApiName.GetPM);
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

            return this.TestScenarioManager.GetResponse(apiName, testContext);
        }
    }
}
// <copyright file="RiskController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class RiskController : EmulatorBaseController
    {
        public RiskController() : base(Constants.TestScenarioManagers.Risk, Constants.DefaultTestScenarios.RiskEmulator)
        {
        }

        [HttpPost]
        public HttpResponseMessage RiskEvaluation([FromBody] object requestData)
        {
            return this.GetResponse(Constants.RiskApiName.RiskEvaluation);
        }
    }
}
// <copyright file="ChallengeManagementSessionController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class ChallengeManagementSessionController : EmulatorBaseController
    {
        public ChallengeManagementSessionController() : base(Constants.TestScenarioManagers.ChallengeManagement, Constants.DefaultTestScenarios.ChallengeManagementServiceEmulator)
        {
        }

        [ActionName("CreateChallengeSession")]
        [HttpPost]
        public HttpResponseMessage CreateChallengeSession(
            [FromBody] SessionDataModel sessionData)
        {
            return this.GetResponse(Constants.ChallengeManagementApiName.CreateChallengeSession);
        }

        [ActionName("GetChallengeSessionData")]
        [HttpGet]
        public HttpResponseMessage GetChallengeSessionData(
            [FromUri] string sessionId)
        {
            return this.GetResponse(Constants.ChallengeManagementApiName.GetChallengeSessionData);
        }

        [ActionName("UpdateChallengeSession")]
        [HttpPut]
        public HttpResponseMessage UpdateChallengeSession(
            [FromBody] SessionBusinessModel sessionBusinessModel)
        {
            return this.GetResponse(Constants.ChallengeManagementApiName.UpdateChallengeSession);
        }
    }
}
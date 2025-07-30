// <copyright file="ChallengeController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class ChallengeController : EmulatorBaseController
    {
        public ChallengeController() : base(Constants.TestScenarioManagers.ChallengeManagement, Constants.DefaultTestScenarios.ChallengeManagementServiceEmulator)
        {
        }

        [ActionName("CreateChallenge")]
        [HttpPost]
        public HttpResponseMessage CreateChallenge(
            [FromQuery] string customerPuid,
            [FromBody] ChallengeCreationModel applyData)
        {
            return this.GetResponse(Constants.ChallengeManagementApiName.CreateChallenge);
        }

        [ActionName("GetChallengeStatus")]
        [HttpGet]
        public HttpResponseMessage GetChallengeStatus(
            [FromQuery] string sessionId)
        {
            return this.GetResponse(Constants.ChallengeManagementApiName.GetChallengeStatus);
        }
    }
}
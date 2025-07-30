// <copyright file="MSRewardsGetUserInfoController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class MSRewardsGetUserInfoController : EmulatorBaseController
    {
        public MSRewardsGetUserInfoController() : base(Constants.TestScenarioManagers.MSRewards, Constants.DefaultTestScenarios.MSRewardsEmulator)
        {
        }

        [ActionName("GetUserInfo")]
        [HttpGet]
        public HttpResponseMessage GetUserInfo([FromQuery] string userId)
        {
            return this.GetResponse(Constants.MSRewardsApiName.GetUserInfo);
        }

        [ActionName("GetUserInfoUserIdEmpty")]
        [HttpGet]
        public HttpResponseMessage GetUserInfoUserIdEmpty()
        {
            return this.GetResponse(Constants.MSRewardsApiName.GetUserInfo);
        }
    }
}
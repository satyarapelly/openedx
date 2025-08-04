// <copyright file="RedeemRewardsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Common.Transaction;
    using Common.Web;
    using PXService.Accessors.MSRewardsService.DataModel;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class RedeemRewardsController : EmulatorBaseController
    {
        public RedeemRewardsController() : base(Constants.TestScenarioManagers.MSRewards, Constants.DefaultTestScenarios.MSRewardsEmulator)
        {
        }

        [ActionName("RedeemRewards")]
        [HttpPost]
        public HttpResponseMessage RedeemRewards(
            [FromBody] Newtonsoft.Json.Linq.JObject redeemData)
        {
            var requestObj = redeemData.ToObject<RedemptionRequest>();
            string apiName = Constants.MSRewardsApiName.RedeemRewards;
            TestContext testContext = null;
            this.Request.TryGetTestContext(out testContext);
            if (testContext.ScenariosContain(Constants.TestScenarios.PXMsRewardsEditPhone))
            {
                apiName = "editPhone";
            }
            else if (testContext.ScenariosContain(Constants.TestScenarios.PXMsRewardsChallenge))
            {
                if (requestObj.RiskContext.ChallengePreference == 0 && requestObj.RiskContext.SolveCode == null)
                {
                    apiName = "challengeFirst";
                }
                else if (requestObj.RiskContext.ChallengePreference != 0 && requestObj.RiskContext.SolveCode == null)
                {
                    apiName = "solveFirst";
                }
            }

            return this.GetResponse(apiName);
        }
    }
}
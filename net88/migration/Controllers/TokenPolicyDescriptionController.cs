// <copyright file="TokenPolicyDescriptionController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using System.Web.Http;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class TokenPolicyDescriptionController : EmulatorBaseController
    {
        public TokenPolicyDescriptionController() : base(Constants.TestScenarioManagers.TokenPolicy, Constants.DefaultTestScenarios.TokenPolicyEmulator)
        {
        }

        [HttpPost]
        public HttpResponseMessage GetTokenPolicyDescription([FromUri] string version, [FromUri]string userId)
        {
            return this.GetResponse(Constants.TokenPolicyApiName.GetTokenPolicyDescription);
        }
    }
}
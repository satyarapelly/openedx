// <copyright file="PartnerSettingsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using System.Web.Http;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class PartnerSettingsController : EmulatorBaseController
    {
        public PartnerSettingsController() : base(Constants.TestScenarioManagers.PartnerSettings, Constants.DefaultTestScenarios.PartnerSettingsEmulator)
        {
        }

        [HttpGet]
        public HttpResponseMessage GetPartnerSettings(
            [FromUri] string partnerName,
            [FromUri] string settingsType,
            [FromUri] string version = null)
        {
            return this.GetResponse(Constants.PartnerSettingsApiName.GetPartnerSettings);
        }
    }
}
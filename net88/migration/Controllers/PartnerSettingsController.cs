// <copyright file="PartnerSettingsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class PartnerSettingsController : EmulatorBaseController
    {
        public PartnerSettingsController() : base(Constants.TestScenarioManagers.PartnerSettings, Constants.DefaultTestScenarios.PartnerSettingsEmulator)
        {
        }

        [HttpGet]
        public HttpResponseMessage GetPartnerSettings(
            [FromQuery] string partnerName,
            [FromQuery] string settingsType,
            [FromQuery] string version = null)
        {
            return this.GetResponse(Constants.PartnerSettingsApiName.GetPartnerSettings);
        }
    }
}
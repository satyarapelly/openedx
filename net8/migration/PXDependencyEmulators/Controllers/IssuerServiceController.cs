// <copyright file="IssuerServiceController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using System.Web.Http;
    using Microsoft.Commerce.Payments.PXService.Model.IssuerService;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class IssuerServiceController : EmulatorBaseController
    {
        public IssuerServiceController() : base(Constants.TestScenarioManagers.IssuerService, Constants.DefaultTestScenarios.IssuerServiceEmulator)
        {
        }

        [ActionName("Apply")]
        [HttpPost]
        public HttpResponseMessage Apply(
            [FromUri] string customerPuid,
            [FromBody] ApplyRequest applyData)
        {
            var response = this.GetResponse(Constants.IssuerServiceApiName.Apply);
            return response;
        }

        [ActionName("Apply")]
        [HttpGet]
        public HttpResponseMessage ApplicationDetails(
            [FromUri] string customerPuid,
            [FromUri] string cardProduct,
            [FromUri] string sessionId)
        {
            var response = this.GetResponse(Constants.IssuerServiceApiName.ApplicationDetails);
            return response;
        }

        [ActionName("Initialize")]
        [HttpPost]
        public HttpResponseMessage Initialize(
            [FromBody] InitializeRequest initializeData)
        {
            return this.GetResponse(Constants.IssuerServiceApiName.Initialize);
        }

        [ActionName("ApplyEligibility")]
        [HttpGet]
        public HttpResponseMessage ApplyEligibility(
            [FromUri] string customerPuid,
            [FromUri] string cardProduct)
        {
            return this.GetResponse(Constants.IssuerServiceApiName.Eligibility);
        }
    }
}
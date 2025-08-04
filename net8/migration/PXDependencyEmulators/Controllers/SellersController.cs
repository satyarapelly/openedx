// <copyright file="SellersController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class SellersController : EmulatorBaseController
    {
        public SellersController() : base(Constants.TestScenarioManagers.SellerMarketPlace, Constants.DefaultTestScenarios.SellerMarketEmulator)
        {
        }

        [HttpGet]
        public HttpResponseMessage GetSeller([FromUri]string paymentProviderId, [FromUri]string sellerId)
        {
            return this.GetResponse(Constants.SellerMarketPlaceApiName.GetSeller);
        }
    }
}
// <copyright file="CatalogController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using System.Web.Http;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class CatalogController : EmulatorBaseController
    {
        public CatalogController() : base(Constants.TestScenarioManagers.Catalog, Constants.DefaultTestScenarios.CatalogEmulator)
        {
        }

        [HttpGet]
        public HttpResponseMessage GetProducts()
        {
            return this.GetResponse(Constants.CatalogApiName.GetProducts);
        }
    }
}
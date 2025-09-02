// <copyright file="StoredValueGiftCatalogController.cs" company="Microsoft">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Mvc;
using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;

    public class StoredValueGiftCatalogController : ControllerBase
    {
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "This needs to be instance methods for web app to work.")]
        [HttpGet]
        public IList<StoredValueFundingCatalog> GetGiftCatalog([FromUri]string currency)
        {
            List<decimal> amounts = new List<decimal>() { 5, 10, 15, 20, 25, 50, 75, 100 };
            List<StoredValueFundingCatalog> catalog = new List<StoredValueFundingCatalog>();
            amounts.ForEach(amount =>
            {
                catalog.Add(new StoredValueFundingCatalog()
                {
                    Amount = amount,
                    CatalogType = "GiftCatalogResource",
                    Currency = currency,
                    Description = string.Format("Test SKU for {0} {1}", amount.ToString("F0"), currency.ToUpper()),
                    Sku = "AMG-01000",
                    Version = "V1"
                });
            });

            return catalog;
        }
    }
}
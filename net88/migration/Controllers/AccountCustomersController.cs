// <copyright file="AccountCustomersController.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class AccountCustomersController : EmulatorBaseController
    {
        public AccountCustomersController() : base(Constants.TestScenarioManagers.Account)
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpGet]
        public HttpResponseMessage GetCustomers([FromQuery] string accountId)
        {
            var resp = this.GetResponse(Constants.AccountApiName.GetCustomers);
            this.PlaceholderReplacements[Constants.Placeholders.AccountId] = accountId;

            return this.ReplacePlaceholders(resp);
        }
    }
}
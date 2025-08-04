// <copyright file="AccountAddressesController.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class AccountAddressesController : EmulatorBaseController
    {
        public AccountAddressesController() : base(Constants.TestScenarioManagers.Account)
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpGet]
        public HttpResponseMessage GetAddresses([FromUri]string accountId)
        {
            return this.GetResponse(Constants.AccountApiName.GetAddresses);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage PostAddress([FromUri] string accountId)
        {
            this.PlaceholderReplacements[Constants.Placeholders.AccountId] = accountId;
            var resp = this.GetResponse(Constants.AccountApiName.PostAddress);

            return this.ReplacePlaceholders(resp);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage LegacyValidateAddress()
        {
            var resp = this.GetResponse(Constants.AccountApiName.LegacyValidateAddress);
            
            return resp;
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpGet]
        public HttpResponseMessage GetAddress([FromUri]string accountId, [FromUri]string addressId)
        {
            this.PlaceholderReplacements[Constants.Placeholders.AccountId] = accountId;
            this.PlaceholderReplacements[Constants.Placeholders.AddressId] = addressId;
            var resp = this.GetResponse(Constants.AccountApiName.GetAddress);

            return this.ReplacePlaceholders(resp);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPost]
        public HttpResponseMessage PostAddressValidate([FromBody]object addressInfo)
        {
            // Both AVS and Accounts service shares the same route for address validation, addresses/validate
            // Due this it not possible to create separate endpoint in webapiconfig.cs of account emulator 
            // Check if the avs accessor is calling for post address or account accessor and return respective response
            if (IsValidateAddressWithAVSFlightExposed(this.Request))
            {
                return this.GetResponse(Constants.AccountApiName.PostAddressValidateAVS);
            }
            else
            {
                return this.GetResponse(Constants.AccountApiName.PostAddressValidate);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPatch]
        public HttpResponseMessage PatchAddress([FromUri] string accountId, [FromUri] string addressId, [FromBody] object addressInfo)
        {
            this.PlaceholderReplacements[Constants.Placeholders.AccountId] = accountId;
            this.PlaceholderReplacements[Constants.Placeholders.AddressId] = addressId;
            
            var resp = this.GetResponse(Constants.AccountApiName.PatchAddress);

            return this.ReplacePlaceholders(resp);
        }

        private static bool IsValidateAddressWithAVSFlightExposed(HttpRequest request)
        {
            if (request.Headers.TryGetValue(Test.Common.Constants.HeaderValues.ExtendedFlightName, out var headerValues))
            {
                var xMSFlightValue = headerValues.FirstOrDefault();
                return xMSFlightValue != null && xMSFlightValue.Contains(Test.Common.Constants.FlightValues.AccountEmulatorValidateAddressWithAVS);
            }

            return false;
        }
    }
}
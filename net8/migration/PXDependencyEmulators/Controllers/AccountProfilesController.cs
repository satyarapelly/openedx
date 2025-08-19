// <copyright file="AccountProfilesController.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class AccountProfilesController : EmulatorBaseController
    {
        public AccountProfilesController() : base(Constants.TestScenarioManagers.Account)
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpGet]
        public HttpResponseMessage GetProfiles([FromUri] string accountId, [FromUri] string type)
        {
            var resp = this.GetResponse(Constants.AccountApiName.GetProfiles);
            this.PlaceholderReplacements[Constants.Placeholders.AccountId] = accountId;

            return this.ReplacePlaceholders(resp);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPut]
        public HttpResponseMessage PutProfile([FromUri] string accountId, [FromUri] string profileId, [FromBody] object profileInfo)
        {
            var resp = this.GetResponse(Constants.AccountApiName.PutProfile);
            this.PlaceholderReplacements[Constants.Placeholders.AccountId] = accountId;
            this.PlaceholderReplacements[Constants.Placeholders.ProfileId] = profileId;

            return this.ReplacePlaceholders(resp);
        }

        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "Extra params needed for routing")]
        [HttpPatch]
        public HttpResponseMessage PatchProfile([FromUri] string accountId, [FromUri] string profileId, [FromBody] object profileInfo)
        {
            var resp = this.GetResponse(Constants.AccountApiName.PatchProfile);
            this.PlaceholderReplacements[Constants.Placeholders.AccountId] = accountId;
            this.PlaceholderReplacements[Constants.Placeholders.ProfileId] = profileId;

            return this.ReplacePlaceholders(resp);
        }
    }
}
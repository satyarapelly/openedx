// <copyright file="StoredValueRedeemController.cs" company="Microsoft">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Http;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;

    public class StoredValueRedeemController : ApiController
    {
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "This needs to be instance methods for web app to work.")]
        [HttpPost]
        public FundStoredValueTransaction PostFunding([FromUri]string legacyAccountId, [FromBody]FundStoredValuePayload payload)
        {
            return new FundStoredValueTransaction()
            {
                TransactionType = "FundResource",
                RedirectionUrl = null,
                Id = "9F8CA8D7-FEE5-4E7A-BDB4-EA30701EC6B6",
                Status = "processing",
                Country = payload.Country,
                Currency = payload.Currency,
                Amount = payload.Amount,
                Description = payload.Description,
                IdentityValue = payload.IdentityValue,
                PaymentCallbackUrl = payload.Success,
                PaymentTransactionId = null,
                PaymentInstrumentId = payload.PaymentInstrumentId,
                Version = "V1"
            };
        }
        
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "This needs to be instance methods for web app to work.")]
        [HttpGet]
        public FundStoredValueTransaction GetFundingStatus([FromUri]string legacyAccountId, [FromUri]string referenceId, [FromBody]FundStoredValuePayload payload)
        {
            if (payload == null)
            {
                payload = new FundStoredValuePayload();
            }

            return new FundStoredValueTransaction()
            {
                TransactionType = "FundResource",
                RedirectionUrl = null,
                Id = referenceId,
                Status = "completed",
                Country = payload.Country,
                Currency = payload.Currency,
                Amount = payload.Amount,
                Description = payload.Description,
                IdentityValue = payload.IdentityValue,
                PaymentCallbackUrl = payload.Success,
                PaymentTransactionId = null,
                PaymentInstrumentId = payload.PaymentInstrumentId,
                Version = "V1"
            };
        }
    }
}
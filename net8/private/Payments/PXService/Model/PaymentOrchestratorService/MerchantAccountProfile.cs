// <copyright file="MerchantAccountProfile.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Collections.Generic;

    public class MerchantAccountProfile
    {
        /// <summary>
        /// Gets or sets the merchant account ID.
        /// For tenants that have multiple merchant accounts, this is the merchant account ID that the transaction should be processed against.
        /// If not provided, the default value for the tenant will be used.
        /// </summary>
        public string MerchantAccountName { get; set; }

        /// <summary>
        /// Gets or sets the AllowedPaymentMethods.
        /// List of payment methods allowed
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Needed for serialization purpose.")]
        public IList<PaymentMethodType> AllowedPaymentMethods { get; set; }

        /// <summary>
        /// Gets or sets the ThirdPartySellerName.
        /// This is the non-Microsoft seller identity used to sell services/goods in specific markets, such as China or India.
        /// Examples are "MSNJV" and "BesTV".
        /// </summary>
        public string ThirdPartySellerName { get; set; }

        /// <summary>
        /// Gets or sets the SellerOfRecord.
        /// This is the four-digit code that represents the Microsoft entity.
        /// </summary>
        public string SellerOfRecord { get; set; }

        /// <summary>
        /// Gets or sets the MerchantDescriptor.
        /// This is the merchant descriptor shown on the customer's bank statement.
        /// </summary>
        public string MerchantDescriptor { get; set; }

        /// <summary>
        /// Gets or sets the MerchantSupportInfo.
        /// This is the merchant support information shown on the customer's bank statement.
        /// </summary>
        public string MerchantSupportInfo { get; set; }
    }
}

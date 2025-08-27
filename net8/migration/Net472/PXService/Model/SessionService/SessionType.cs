// <copyright file="SessionType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.SessionService
{
    public enum SessionType
    {
        /// <summary>
        /// Any:
        /// Indicates the sessionResource is type agnostic. 
        /// When used in a query (GET) api it will always return the  sessionResource 
        /// (if present) even if it was created for a different SessionType
        /// </summary>
        Any,

        /// <summary>
        /// ChargeRedirection: 
        /// Used for async payment scenarios such as Alipay to temporarily store metadata 
        /// associated with receiving provider asyn notifications on a payment
        /// </summary>
        ChargeRedirection,

        /// <summary>
        /// ChallengeRedirection: 
        /// Used for redirection scenarios like 3dsecure to temporarily store metadata 
        /// associated with the 3ds handshake of a transaction
        /// </summary>
        ChallengeRedirection,

        /// <summary>
        /// InvoicePay: 
        /// Used for Commercial Invoicing scenarios to temporarily store metadata 
        /// associated with an Invoice (InvoiceId, ChargeAggregationGroupId)
        /// </summary>
        InvoicePay,

        /// <summary>
        /// Subscription:
        /// Used for sending subscription data (ID, Frequency) between M$ and Payments when a new subscription charge is made
        /// </summary>
        Subscription,

        /// <summary>
        /// ThirdPartySeller:
        /// Used for sending third party seller information including additional buyer and seller information.
        /// </summary>
        ThirdPartySeller,

        /// <summary>
        /// RevenueAllocator:
        /// Used for sending additional usage data (GroupId, ServiceDeliveryTime) between Billing and Payments when capture usage.
        /// </summary>
        RevenueAllocator,

        /// <summary>
        /// Inline:
        /// Used for sending inline data in the format defined in regex<see cref="InlineSessionDataParser"/>.
        /// </summary>
        Inline
    }
}
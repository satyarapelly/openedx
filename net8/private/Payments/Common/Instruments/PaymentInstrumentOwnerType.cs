// <copyright file="PaymentInstrumentOwnerType.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    /// <summary>
    /// Enumerates the types of user account that owns the payment instrument
    /// </summary>
    public enum PaymentInstrumentOwnerType
    {
        /// <summary>
        /// Uninitialized OwnerType
        /// </summary>
        None,

        /// <summary>
        /// The windows live id
        /// </summary>
        Passport,

        /// <summary>
        /// business account
        /// </summary>
        ScsAccount,

        /// <summary>
        /// The ad center account
        /// </summary>
        AdCenter,

        /// <summary>
        /// The ad center customer id
        /// </summary>
        AdCenterCustomerId,

        /// <summary>
        /// The OrgPuid stands for org puid
        /// </summary>
        OrgPuid,

        /// <summary>
        /// AnonymousID stands for anonymous id
        /// </summary>
        AnonymousId
    }
}

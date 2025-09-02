// <copyright file="EncryptedAccountNumber.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System.Diagnostics.CodeAnalysis;

    public class EncryptedAccountNumber
    {
        public long PaymentMethodId { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is the legacy representation and is not serialized")]
        public byte[] EncryptedInfo { get; set; }
    }
}

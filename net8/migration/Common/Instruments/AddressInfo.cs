// <copyright file="AddressInfo.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    public class AddressInfo
    {
        public string UnitNumber { get; set; }

        public string Street { get; set; }

        public string StreetLine2 { get; set; }

        public string StreetLine3 { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        public string State { get; set; }

        public string CountryCode { get; set; }
    }
}

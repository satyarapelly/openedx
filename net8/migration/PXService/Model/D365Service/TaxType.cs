// <copyright file="TaxType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    public enum TaxType
    {
        Generic = 0,
        Gst = 1,
        Jct = 2,
        Vat = 3,
        CityAmusement = 4,
        CityLease = 5,
        Hst = 6,
        Pst = 7,
        Qst = 8,
        Sut = 9,
        LocalCst = 10,
        StateCst = 11,
        StateTct = 12,
        DistrictTct = 13
    }
}
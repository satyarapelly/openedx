// <copyright file="AddressAVSValidationStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService.AddressValidation
{
    public enum AddressAVSValidationStatus
    {
        VerifiedShippable,
        Verified,
        StreetPartial,
        PremisesPartial,
        Multiple,
        InteractionRequired,
        None,
        NotValidated,
        InvalidStreet,
        InvalidCityRegionPostalCode
    }
}
// <copyright file="EnrichmentValidationStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.AddressEnrichmentService.DataModel
{
    public enum EnrichmentValidationStatus
    {
        VerifiedShippable,
        Verified,
        StreetPartial,
        PremisesPartial,
        Multiple,
        InteractionRequired,
        None,
        NotValidated
    }
}
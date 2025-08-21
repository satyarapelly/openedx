// <copyright file="Partner.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.Authentication
{
    public static class Partner
    {
        /// <summary>
        /// All known partner names
        /// </summary>
        public enum Name
        {
            PIFDService,
            PIFDServicePPE,
            PXCOT,
            PXFirstParty,
            PaymentOrchestrator
        }
    }
}
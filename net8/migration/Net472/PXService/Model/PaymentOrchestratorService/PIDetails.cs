// <copyright file="PIDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Collections.Generic;

    public class PIDetails
    {
        public decimal TaxAmount { get; set; }

        public string AccountHolderName { get; set; }

        public int ExpiryYear { get; set; }

        public int ExpiryMonth { get; set; }

        public PIAddress Address { get; set; }

        public string LastFourDigits { get; set; }

        public bool Exportable { get; set; }

        public string CardType { get; set; }

        public string ValidationLevel { get; set; }

        public List<string> NetworkTokens { get; } = new List<string>();

        public bool IsXboxCoBrandedCard { get; set; }
    }
}
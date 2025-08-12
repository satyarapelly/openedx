// <copyright file="CSVTokenValidationResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    public class CSVTokenValidationResult
    {
        public CSVTokenStatus TokenStatus { get; set; }

        public decimal? TokenValue { get; set; }

        public string TokenCurrency { get; set; }
    }
}
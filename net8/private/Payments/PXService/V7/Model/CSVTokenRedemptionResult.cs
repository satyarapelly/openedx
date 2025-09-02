// <copyright file="CSVTokenRedemptionResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    public class CSVTokenRedemptionResult
    {
        public CSVTokenValidationResult TokenValidationResult { get; set; }

        public bool IsSuccess { get; set; }
    }
}
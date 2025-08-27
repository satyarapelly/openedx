// <copyright file="CSVTokenStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    public enum CSVTokenStatus
    {
        Unknown,
        TokenNotFound,
        TokenAlreadyRedeemed,
        TokenExpired,
        NonCSVToken,
        CouldNotValidate,
        ValidCSVToken
    }
}
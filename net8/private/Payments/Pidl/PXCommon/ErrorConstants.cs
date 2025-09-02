// <copyright file="ErrorConstants.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    public static class ErrorConstants
    {
        public static class ErrorCodes
        {
            public const string InvalidRequestData = "InvalidRequestData";
            public const string InvalidOperationException = "InvalidOperationException";
            public const string InternalError = "InternalError";
            public const string UnknownError = "UnknownError";
            public const string ServiceUnavailable = "ServiceUnavailable";
            public const string InvalidPartner = "PIDLPartnerNameIsNotValid";
        }

        public static class ErrorMessages
        {
            public const string ProtocolNotSupport = "Protocal is not supported";
            public const string CertRequired = "Valid client certificate is required.";
            public const string InternalServerError = "Internal server error.";
            public const string CountryNotSupported = "The country is not supported.";
            public const string InvalidPartner = "Invalid Partner Name.";
        }
    }
}

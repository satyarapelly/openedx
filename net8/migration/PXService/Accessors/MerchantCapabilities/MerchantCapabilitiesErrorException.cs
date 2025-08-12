// <copyright file="MerchantCapabilitiesErrorException.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.MerchantCapabilitiesService.V7
{
    using System;
    using System.Net;

    [Serializable]
    public class MerchantCapabilitiesErrorException : ServiceErrorResponseException
    {
        public MerchantCapabilitiesErrorException()
        {
        }

        public MerchantCapabilitiesErrorException(string errorCode, string errorMessage, string errorTarget, HttpStatusCode statusCode)
        {
            this.Error = new MerchantCapabilitiesErrorResponse()
            {
                Error = new MerchantCapabilitiesErrorResponse()
                {
                    Code = errorCode,
                    Message = errorMessage,
                    Target = errorTarget
                }
            };
            this.Error.HttpStatusCode = statusCode;
        }
    }
}
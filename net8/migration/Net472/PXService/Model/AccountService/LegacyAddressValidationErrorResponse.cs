// <copyright file="LegacyAddressValidationErrorResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService
{
    public class LegacyAddressValidationErrorResponse
    {
        public LegacyAddressValidationErrorResponse()
        {
        }

        public LegacyAddressValidationErrorResponse(string code, string reason)
        {
            this.Code = code;
            this.Reason = reason;
        }

        public string Code { get; set; }

        public string Reason { get; set; }

        public string Object_type { get; set; }

        public string Resource_status { get; set; }
    }
}
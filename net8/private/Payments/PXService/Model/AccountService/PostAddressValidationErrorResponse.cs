// <copyright file="PostAddressValidationErrorResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService
{
    public class PostAddressValidationErrorResponse
    {
            public PostAddressValidationErrorResponse()
            {
            }

            public PostAddressValidationErrorResponse(string error_code, string message)
            {
                this.Error_code = error_code;
                this.Message = message;
            }

            public string Error_code { get; set; }

            public string Message { get; set; }

            public Parameters Parameters { get; set; }

            public string Object_type { get; set; }
    }
}
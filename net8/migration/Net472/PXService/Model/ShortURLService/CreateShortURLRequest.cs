// <copyright file="CreateShortURLRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ShortURLService
{
    public class CreateShortURLRequest
    {
        public string URL { get; set; }

        public int? TTLMinutes { get; set; }
    }
}

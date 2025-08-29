// <copyright file="CreateRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ShortURLDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
 
    public class CreateRequest
    {
        public string URL { get; set; }

        public int? TTLMinutes { get; set; }
    }
}
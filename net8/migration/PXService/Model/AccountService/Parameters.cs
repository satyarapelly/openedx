// <copyright file="Parameters.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService
{
    public class Parameters
    {
        public Parameters()
        {
        }

        public Parameters(string property_name, string details)
        {
            this.Property_name = property_name;
            this.Details = details;
        }

        public string Property_name { get; set; }

        public string Details { get; set; }
    }
}
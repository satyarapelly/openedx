// <copyright file="PidlIdentity.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.ApiSurface.Diff
{
    public class PidlIdentity
    {
        public string UserType { get; set; }

        public string ResourceName { get; set; }

        public string Id { get; set; }

        public string Operation { get; set; }

        public string Country { get; set; }

        public string Language { get; set; }

        public string Partner { get; set; }

        public string Scenario { get; set; }

        public string Filters { get; set; }

        public string AllowedPayementMethods { get; set; }
    }
}

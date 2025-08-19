// <copyright file="ProviderToken.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using Newtonsoft.Json;

    public class ProviderToken
    {
        [JsonProperty(PropertyName = "billDeskToken")]
        public BillDeskToken BillDeskToken { get; set; }
    }
}

// <copyright file="InnerHttpError.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService.ChallengeManagementErrorResponse
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class InnerHttpError
    {
        public int StatusCode { get; set; }

        public object Body { get; set; }

        [JsonExtensionData(ReadData = true, WriteData = true)]

        public JObject Properties { get; } = new JObject();
    }
}
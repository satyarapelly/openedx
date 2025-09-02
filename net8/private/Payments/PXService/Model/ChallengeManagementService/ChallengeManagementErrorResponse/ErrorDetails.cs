// <copyright file="ErrorDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService.ChallengeManagementErrorResponse
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ErrorDetails
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public InnerHttpError InnerHttpError { get; set; }

        [JsonExtensionData(ReadData = true, WriteData = true)]

        public JObject Properties { get; } = new JObject();
    }
}
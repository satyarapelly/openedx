// <copyright file="ChallengeManagementServiceErrorResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService.ChallengeManagementErrorResponse
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ChallengeManagementServiceErrorResponse
    {
        public ErrorDetails Error { get; set; }
    }
}
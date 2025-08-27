// <copyright file="ChallengeEnumDefinition.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Web;

    public class ChallengeEnumDefinition
    {
        public enum ChallengeRequestor
        {
            [Description("PXService")]
            PXService
        }

        public enum ChallengeProvider
        {
            [Description("Arkose")]
            Arkose,
            [Description("HIP")]
            HIP,
            [Description("MFA")]
            MFA
        }
    }
}
// <copyright file="SessionEnumDefinition.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Web;

    public class SessionEnumDefinition
    {
        public enum SessionType
        {
            [Description("PXAddPISession")]
            PXAddPISession,
            [Description("ChallengeManagerSession")]
            ChallengeManagerSession
        }

        public enum SessionStatus
        {
            [Description("Active")]
            Active,
            [Description("Completed")]
            Completed,
            [Description("Abandoned")]
            Abandoned,
            [Description("Failed")]
            Failed
        }
    }
}
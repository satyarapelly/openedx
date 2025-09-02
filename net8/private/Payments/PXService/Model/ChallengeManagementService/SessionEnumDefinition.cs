// <copyright file="SessionEnumDefinition.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService
{
    using System.ComponentModel;

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
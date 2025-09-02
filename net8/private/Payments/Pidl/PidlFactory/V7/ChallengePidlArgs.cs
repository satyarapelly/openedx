// <copyright file="ChallengePidlArgs.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    /// <summary>
    /// This class represents the Argument that needs to be supplied to generate a Challenge Pidl
    /// </summary>
    public class ChallengePidlArgs
    {
        /// <summary>
        /// Gets or sets the Account Id for the user
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PaymentInstrument"/>
        /// </summary>
        public PaymentInstrument PaymentInstrument { get; set; }

        /// <summary>
        /// Gets or sets the language for the user
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the Partner Name for the request
        /// </summary>
        public string PartnerName { get; set; }

        /// <summary>
        /// Gets or sets the Pifd Base Url for the request
        /// </summary>
        public string PifdBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the challenge option needs to be reverted
        /// </summary>
        public bool RevertChallengeOption { get; set; }

        /// <summary>
        /// Gets or sets the EventTraceActivity
        /// </summary>
        public EventTraceActivity EventTraceActivity { get; set; }
    }
}

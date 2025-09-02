// <copyright file="IntegrationException.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory
{
    using System;

    /// <summary>
    /// This exception is thrown when there is an integration issue with another service due to contractual discrepancy.
    /// </summary>
    public class IntegrationException : PIDLException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationException"/> class.
        /// </summary>
        /// <param name="targetServiceName">The name of the service with which integration is done.</param>
        /// <param name="message">Exception message.</param>
        /// <param name="errorCode">The error code associated with the integration exception.</param>
        public IntegrationException(string targetServiceName, string message, string errorCode)
            : base(message, errorCode)
        {
            this.TargetServiceName = targetServiceName;
        }

        public string TargetServiceName { get; }

        public override string ToString()
        {
            return $"Error Integrating with Service: {this.TargetServiceName} Message: {this.Message}";
        }
    }
}

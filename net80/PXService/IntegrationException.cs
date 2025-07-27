// <copyright file="IntegrationException.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// This exception is thrown when there is an integration issue with another service due to contractual discrepency
    /// </summary>
    [Serializable]
    public class IntegrationException : PXServiceException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationException"/> class.
        /// </summary>
        /// <param name="targetServiceName">The name of the service with which integration is done</param>
        /// <param name="message">exception message</param>
        /// <param name="errorCode">The Error Code associated with the Integration exception</param>
        public IntegrationException(string targetServiceName, string message, string errorCode)
            : base(message, errorCode)
        {
            this.TargetServiceName = targetServiceName;
        }

        public string TargetServiceName { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override string ToString()
        {
            return string.Format("Error Integrating with Service : {0} Message: {1}", this.TargetServiceName, this.Message);
        }
    }
}

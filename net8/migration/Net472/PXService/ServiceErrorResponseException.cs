// <copyright file="ServiceErrorResponseException.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Net.Http;
    using System.Runtime.Serialization;
    
    public enum ExceptionHandlingPolicy
    {
        Default,

        /// <summary>
        /// ByPass:
        /// the PXServiceExceptionFilter bypass the response as it is
        /// shouldn't change status code or response message
        /// </summary>
        ByPass
    }

    [Serializable]
    public class ServiceErrorResponseException : Exception
    {
        private const string HttpResponseMessageName = "Response";
        private const string ServiceErrorCodeName = "Error";

        public HttpResponseMessage Response { get; set; }

        public ServiceErrorResponse Error { get; set; }

        public ExceptionHandlingPolicy HandlingType { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
            info.AddValue(HttpResponseMessageName, string.Format("Http StatusCode: {0}", this.Response.StatusCode));
            if (this.Error != null)
            {
                info.AddValue(
                    ServiceErrorCodeName, 
                    string.Format("ErrorCode: {0} Message: {1} Source: {2}", this.Error.ErrorCode, this.Error.Message, this.Error.Source ?? string.Empty));
            }
            else
            {
                info.AddValue(ServiceErrorCodeName, string.Empty);
            }
        }
    }
}
// <copyright file="RiskServiceErrorResponseException.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.RiskService.V7
{
    using System;
    using System.Net.Http;
    using System.Runtime.Serialization;

    [Serializable]
    public class RiskServiceErrorResponseException : Exception
    {
        private const string HttpResponseMessageName = "Response";
        private const string ServiceErrorCodeName = "Error";
        private const string ServiceErrorParameterCodeName = "ErrorParameter";

        private HttpResponseMessage response;
        private RiskServiceErrorResponse error;

        public RiskServiceErrorResponseException(HttpResponseMessage response, RiskServiceErrorResponse error)
        {
            this.response = response;
            this.error = error;
        }

        public HttpResponseMessage Response
        {
            get { return this.response; }
        }

        public RiskServiceErrorResponse Error
        {
            get { return this.error; }
        }

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
                    string.Format("ErrorCode: {0} Message: {1}", this.Error.Code, this.Error.Message));
                int i = 0;
                foreach (string p in this.Error.Parameters)
                {
                    info.AddValue(ServiceErrorParameterCodeName + i, p);
                    i++;
                }
            }
            else
            {
                info.AddValue(ServiceErrorCodeName, string.Empty);
            }
        }
    }
}
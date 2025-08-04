// <copyright file="InstrumentManagementServiceV3Client.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace COT.PXService
{
    using System;
    using Newtonsoft.Json;
    using Test.Common;
    
    public class PXServiceClient : ServiceClient
    {
        public const string HeaderCorrelationVector = "MS-CV";

        public PXServiceClient(ServiceClientSettings settings)
            : base(settings)
        {
        }

        protected override string SerializeObject(object requestInput, string contentType)
        {
            if (contentType.ToLower().Contains("application/json"))
            {
                return JsonConvert.SerializeObject(requestInput);
            }
            else
            {
                throw new NotSupportedException(string.Format("{0} is not a supported content type", contentType));
            }
        }

        protected override T Deserialize<T>(string value, string contentType)
        {
            if (contentType.ToLower().Contains("application/json"))
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            else if (contentType.ToLower().Contains(Constants.HeaderValues.HtmlContent))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (string.IsNullOrEmpty(value))
            {
                // The response content is empty when successfully remove PI. Add this to avoid hitting the following "else".
                return default(T);
            }
            else
            {
                throw new NotSupportedException(string.Format("{0} is not a supported content type", contentType));
            }
        }
    }
}

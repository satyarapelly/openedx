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

        // Use 'new' instead of 'override' if base class method is not virtual
        protected new string SerializeObject(object requestInput, string contentType)
        {
            if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return JsonConvert.SerializeObject(requestInput);
            }
            else
            {
                throw new NotSupportedException($"{contentType} is not a supported content type");
            }
        }

        protected new T Deserialize<T>(string value, string contentType)
        {
            if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            else if (contentType.Contains(Constants.HeaderValues.HtmlContent, StringComparison.OrdinalIgnoreCase))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (string.IsNullOrEmpty(value))
            {
                return default;
            }
            else
            {
                throw new NotSupportedException($"{contentType} is not a supported content type");
            }
        }
    }
}


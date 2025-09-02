// <copyright file="ServiceErrorResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;

    public class ServiceErrorResponse
    {
        public ServiceErrorResponse()
        {
        }

        public ServiceErrorResponse(string errorCode, string message)
        {
            this.ErrorCode = errorCode;
            this.Message = message;
        }

        public ServiceErrorResponse(string errorCode, string message, string source)
        {
            this.ErrorCode = errorCode;
            this.Message = message;
            this.Source = source;
        }

        public ServiceErrorResponse(string correlationId, string source, ServiceErrorResponse innerError) 
        {
            this.CorrelationId = correlationId;
            this.ErrorCode = innerError.ErrorCode;
            this.Message = innerError.Message;
            this.Source = source;
            this.InnerError = innerError;
        }

        /// <summary>
        /// Gets or sets the correlation Id of the request
        /// </summary>
        [JsonProperty(Order = 0)]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the error code 
        /// </summary>
        [JsonProperty(Order = 1)]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        [JsonProperty(Order = 2)]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the user display error message
        /// </summary>
        [JsonProperty(Order = 3)]
        public string UserDisplayMessage { get; set; }

        /// <summary>
        /// Gets or sets the source of the error
        /// </summary>
        [JsonProperty(Order = 10)]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the target of the error
        /// </summary>
        [JsonProperty(Order = 11)]
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets the ClientAction indicating what the client is expected to do
        /// when it gets this error
        /// </summary>
        [JsonProperty(Order = 20, PropertyName = "clientAction")]
        public ClientAction ClientAction { get; set; } 

        /// <summary>
        /// Gets or sets details about the error. This lists all possible reasons for the failure.
        /// e.g. If multiple attributes in the input payload is invalid, they would be listed here.
        /// </summary>
        [JsonProperty(Order = 30)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Allowed to be set by derived classes")]
        public IList<ServiceErrorDetail> Details { get; set; }

        /// <summary>
        /// Gets or sets the inner error if the source of the error was an underlying service
        /// when it gets this error
        /// </summary>
        [JsonProperty(Order = 40)]
        public ServiceErrorResponse InnerError { get; set; }

        /// <summary>
        /// Gets or sets component information if the source of the error was a component
        /// when it gets this error
        /// </summary>
        [JsonProperty(Order = 50)]
        public string Component { get; set; }

        [JsonIgnore]
        public HttpStatusCode HttpStatusCode { get; set; }

        /// <summary>
        /// Initializes the Details list if needed and adds the given ServiceErrorDetail to it
        /// </summary>
        /// <param name="newDetail">A new detail object that needs to be added to the list</param>
        public void AddDetail(ServiceErrorDetail newDetail)
        {
            if (this.Details == null)
            {
                this.Details = new List<ServiceErrorDetail>();
            }

            this.Details.Add(newDetail);
        }
    }
}
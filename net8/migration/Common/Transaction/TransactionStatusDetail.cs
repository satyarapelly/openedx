// <copyright file="TransactionStatusDetail.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class TransactionStatusDetail
    {
        public TransactionStatusDetail()
        {
            this.Code = TransactionDeclineCode.None;
        }

        [JsonProperty(PropertyName = "code")]
        public TransactionDeclineCode Code { get; set; }

        [JsonProperty(PropertyName = "processor_response")]
        public object ProcessorResponse { get; set; }

        [JsonProperty(PropertyName = "decline_message")]
        public string DeclineMessage { get; set; }

        public override string ToString()
        {
            string processorResponseMsg = (this.ProcessorResponse != null) ? JsonConvert.SerializeObject(this.ProcessorResponse) : string.Empty;
            return string.Format("code:{0},processor_response:{1},decline_message:{2}", this.Code, processorResponseMsg, this.DeclineMessage);
        }

        public TransactionStatusDetail Clone()
        {
            return new TransactionStatusDetail
            {
                Code = this.Code,
                DeclineMessage = this.DeclineMessage,

                // A reference copy here. 
                ProcessorResponse = this.ProcessorResponse,
            };
        }
    }
}
// <copyright file="PostMessageToChildIFramesActionContext.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PostMessageToChildIFramesActionContext
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        [JsonProperty(PropertyName = "message")]
        public Dictionary<string, object> Message { get; set; }

        [JsonProperty(PropertyName = "targetOrigin")]
        public string TargetOrigin { get; set; }
    }
}

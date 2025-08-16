// <copyright file="PropertyEvent.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using Newtonsoft.Json;

    public class PropertyEvent
    {
        public PropertyEvent()
        {
        }

        public PropertyEvent(PropertyEvent template)
        {
            this.EventType = template.EventType;
            this.Context = template.Context;
        }

        [JsonProperty(PropertyName = "type")]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "context")]
        public object Context { get; set; }

        [JsonProperty(PropertyName = "nextAction")]
        public DisplayHintAction NextAction { get; set; }
    }
}
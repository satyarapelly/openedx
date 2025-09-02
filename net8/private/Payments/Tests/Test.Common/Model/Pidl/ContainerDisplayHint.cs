// <copyright file="ContainerDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Abstract class for describing a Container Display Hint.
    /// A Container Display hint is any Display hint which has child Display hint members
    /// </summary>
    public abstract class ContainerDisplayHint : DisplayHint
    {
        [JsonIgnore]
        public string DisplaySequenceId { get; set; }

        [JsonIgnore]
        public string ContainerDisplayType { get; set; }

        [JsonProperty(PropertyName = "layoutOrientation")]
        public string LayoutOrientation { get; set; }

        [JsonProperty(PropertyName = "layoutAlignment")]
        public string LayoutAlignment { get; set; }

        [JsonProperty(PropertyName = "containerDescription")]
        public string ContainerDescription { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(PropertyName = "members")]
        public List<DisplayHint> Members { get; set; }
    }
}
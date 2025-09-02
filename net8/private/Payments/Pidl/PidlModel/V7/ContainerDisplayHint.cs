// <copyright file="ContainerDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Newtonsoft.Json;

    /// <summary>
    /// Abstract class for describing a Container Display Hint.
    /// A Container Display hint is any Display hint which has child Display hint members
    /// </summary>
    public abstract class ContainerDisplayHint : DisplayHint
    {
        private List<DisplayHint> members;

        private string containerDescription;

        public ContainerDisplayHint()
        {
            this.members = new List<DisplayHint>();
        }

        public ContainerDisplayHint(ContainerDisplayHint template)
            : base(template)
        {
            this.DisplaySequenceId = template.DisplaySequenceId;
            this.ContainerDisplayType = template.ContainerDisplayType;
            this.LayoutOrientation = template.LayoutOrientation;
            this.LayoutAlignment = template.LayoutAlignment;
            this.ContainerDescription = template.ContainerDescription;
            this.IsModalGroup = template.IsModalGroup;
            this.StyleHints = template.StyleHints;
            if (template.members != null)
            {
                this.members = new List<DisplayHint>(template.Members);
            }
        }

        [JsonIgnore]
        public string DisplaySequenceId { get; set; }

        [JsonIgnore]
        public string ContainerDisplayType { get; set; }

        [JsonProperty(PropertyName = "layoutOrientation")]
        public string LayoutOrientation { get; set; }

        [JsonProperty(PropertyName = "layoutAlignment")]
        public string LayoutAlignment { get; set; }

        [JsonProperty(PropertyName = "isModalGroup")]
        public bool? IsModalGroup { get; set; }

        [JsonProperty(PropertyName = "containerDescription")]
        public string ContainerDescription
        {
            get { return string.IsNullOrWhiteSpace(this.containerDescription) ? null : PidlModelHelper.GetLocalizedString(this.containerDescription); }
            set { this.containerDescription = value; }
        }

        [JsonProperty(PropertyName = "members")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<DisplayHint> Members
        {
            get 
            { 
                return this.members; 
            }

            set
            {
                if (value != null)
                {
                    this.members.AddRange(value);
                }
            }
        }

        public void ClearDisplayHints()
        {
            this.members = new List<DisplayHint>();
        }

        public void AddDisplayHint(DisplayHint displayHint)
        {
            this.members.Add(displayHint);
        }

        public void AddDisplayHints(IEnumerable<DisplayHint> displayHints)
        {
            this.members.AddRange(displayHints);
        }

        public void RemoveDisplayHint(DisplayHint displayHint)
        {
            this.members.RemoveAll(member => member.HintId == displayHint.HintId);
        }
    }
}
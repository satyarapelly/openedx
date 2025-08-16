// <copyright file="DisplaySequence.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class for describing the sequencing of group and property display hints
    /// </summary>
    public sealed class DisplaySequence
    {
        private List<DisplayHint> members;

        public DisplaySequence()
        {
            this.members = new List<DisplayHint>();
        }

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

        public void AddDisplayHint(DisplayHint displayHint)
        {
            this.members.Add(displayHint);
        }
    }
}
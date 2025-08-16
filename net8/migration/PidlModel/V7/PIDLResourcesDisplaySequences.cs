// <copyright file="PIDLResourcesDisplaySequences.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Instances of this class describes the display sequences configured to a PIDL Resource at the top level.
    /// Each instance of this class represents a row / multiple rows in PIDLResourcesDisplaySequences.csv
    /// </summary>
    public class PIDLResourcesDisplaySequences
    {
        private List<string> displaySequenceIds;
        private List<string> displayStringSequenceIds;

        public PIDLResourcesDisplaySequences()
        {
            this.displaySequenceIds = new List<string>();
            this.displayStringSequenceIds = new List<string>();
        }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<string> DisplaySequenceIds
        {
            get { return this.displaySequenceIds; }
            set { this.displaySequenceIds = value; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<string> DisplayStringsSequenceIds
        {
            get { return this.displayStringSequenceIds; }
            set { this.displayStringSequenceIds = value; }
        }
    }
}
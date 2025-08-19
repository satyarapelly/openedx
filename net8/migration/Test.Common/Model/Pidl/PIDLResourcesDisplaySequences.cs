// <copyright file="PIDLResourcesDisplaySequences.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;

    /// <summary>
    /// Instances of this class describes the display sequences configured to a PIDL Resource at the top level.
    /// Each instance of this class represents a row / multiple rows in PIDLResourcesDisplaySequences.csv
    /// </summary>
    public class PIDLResourcesDisplaySequences
    {
        private readonly List<string> displaySequenceIds;
        private readonly List<string> displayStringSequenceIds;

        public PIDLResourcesDisplaySequences()
        {
            this.displaySequenceIds = new List<string>();
            this.displayStringSequenceIds = new List<string>();
        }

        public List<string> DisplaySequenceIds
        {
            get { return this.displaySequenceIds; }
        }

        public List<string> DisplayStringsSequenceIds
        {
            get { return this.displayStringSequenceIds; }
        }
    }
}
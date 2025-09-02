// <copyright file="DisplaySequence.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;

    /// <summary>
    /// Class for describing the sequencing of group and property display hints
    /// </summary>
    public sealed class DisplaySequence
    {
        private readonly List<DisplayHint> members;

        public DisplaySequence()
        {
            this.members = new List<DisplayHint>();
        }

        public List<DisplayHint> Members
        {
            get { return this.members; }
        }

        public void AddDisplayHint(DisplayHint displayHint)
        {
            this.members.Add(displayHint);
        }
    }
}
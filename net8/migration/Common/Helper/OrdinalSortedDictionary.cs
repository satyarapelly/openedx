// <copyright file="OrdinalSortedDictionary.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System.Collections.Generic;

    public sealed class OrdinalSortedDictionary : SortedDictionary<string, string>
    {
        public OrdinalSortedDictionary()
            : base(new OrdinalStringComparer())
        {
        }

        public OrdinalSortedDictionary(OrdinalSortedDictionary dict)
            : base(dict, new OrdinalStringComparer())
        {
        }

        private sealed class OrdinalStringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.CompareOrdinal(x, y);
            }
        }
    }
}

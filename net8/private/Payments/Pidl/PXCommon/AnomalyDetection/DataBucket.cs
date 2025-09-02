// <copyright file="DataBucket.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    internal class DataBucket
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "This needs to be interlocked incremented")]
        public int Value;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "This needs to be interlocked incremented")]
        public int Index;

        public DataBucket(int index, int value)
        {
            this.Index = index;
            this.Value = value;
        }
    }
}
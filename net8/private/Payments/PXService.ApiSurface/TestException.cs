// <copyright file="TestException.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.ApiSurface.Diff
{
    using System;

    [Serializable]
    internal class TestException : Exception
    {
        public TestException(string message) : base(message)
        {
        }
    }
}

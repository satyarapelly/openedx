// <copyright file="SensitiveDataAttribute.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;

    /// <summary>
    /// This attribute can be used to mark the Message Properties that shouldn't be traced when serializing 
    /// Xml messages for tracing purposes
    /// </summary>
    public sealed class SensitiveDataAttribute : Attribute
    {
    }
}

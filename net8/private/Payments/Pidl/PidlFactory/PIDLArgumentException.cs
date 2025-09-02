// <copyright file="PIDLArgumentException.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory
{
    using System;

    [Serializable]
    public class PIDLArgumentException : PIDLException
    {
        public PIDLArgumentException(string message, string errorCode)
            : base(message, errorCode)
        {
        }
    }
}
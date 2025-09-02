// <copyright file="PIDLConfigException.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory
{
    using System;

    [Serializable]
    public class PIDLConfigException : PIDLException
    {
        public PIDLConfigException(string fileName, long lineNum, string message, string errorCode) 
            : base(string.Format("File {0}, line {1} has an error.  {2}", fileName, lineNum, message), errorCode)
        {
        }

        public PIDLConfigException(string message, string errorCode)
            : base(message, errorCode)
        {
        }
    }
}
// <copyright file="PIDLConfigException.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace PidlTest.Diff
{
    using System;

    [Serializable]
    public class PidlConfigException : Exception
    {
        public PidlConfigException(string fileName, long lineNum, string message)
            : base(string.Format("File {0}, line {1} has an error.  {2}", fileName, lineNum, message))
        {
        }

        public PidlConfigException(string message)
            : base(message)
        {
        }
    }
}
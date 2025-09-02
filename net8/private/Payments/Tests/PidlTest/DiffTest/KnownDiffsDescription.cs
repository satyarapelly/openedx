// <copyright file="KnownDiffsDescription.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace PidlTest.Diff
{
    public class KnownDiffsDescription
    {
        public string DeltaType { get; set; }
        
        public string BaselineJPath { get; set; }

        public string NewJPath { get; set; }

        public string NewValue { get; set; }
    }
}

// <copyright file="DiffDetails.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>
namespace PidlTest.JsonDiff
{
    public class DiffDetails
    {
        public string Description { get; set; }

        public DiffType DiffType { get; set; }

        public string JPath { get; set; }

        public string Expected { get; set; }

        public string Actual { get; set; }

        public string Data { get; set; }

        public string Triage { get; set; }
    }
}

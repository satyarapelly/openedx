// <copyright file="TestRun.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace PidlTest.Diff
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using JsonDiff;
    using Microsoft.Commerce.Payments.PXService.ApiSurface.Diff;
    using Newtonsoft.Json.Linq;

    internal class TestRun
    {
        public TestRun(Test test)
        {
            this.Test = test;
        }

        public TestRun(Test test, HttpResponseMessage baselineResponse, HttpResponseMessage underTestResponse, JToken baselineJson, JToken underTestJson)
        {
            this.Test = test;
            this.BaseLineTestResponse = baselineResponse;
            this.UnderTestResponse = underTestResponse;
            this.BaselineJson = baselineJson;
            this.UnderTestJson = underTestJson;
            this.UnexpectedDiffs = new List<DiffDetails>();
        }

        public bool IsComparisonSuccess
        {
            get
            {
                return (this.FailedExecution == null || !string.IsNullOrEmpty(this.FailedExecution.Triage))
                  && (this.UnexpectedDiffs == null || this.UnexpectedDiffs.Count(x => !string.IsNullOrEmpty(x.Triage)) == this.UnexpectedDiffs.Count);
            }
        }
        
        public HttpResponseMessage BaseLineTestResponse { get; }

        public HttpResponseMessage UnderTestResponse { get; }
        
        public JToken BaselineJson { get; }

        public JToken UnderTestJson { get; }
        
        public Test Test { get; }

        public DiffDetails FailedExecution { get; set; }

        public List<DiffDetails> UnexpectedDiffs { get; set; }
    }
}

// <copyright file="Test.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.ApiSurface.Diff
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    // NOTE: the order these are placed in is important
    // changing anything about this enum could have unexpected results
    public enum PIState
    {
        Add, Resume, Get, None, IssuerServiceApply, IssuerServiceApplyEligibility, IssuerServiceInitialize
    }

    /// <summary>
    /// All data for a single test scenario
    /// </summary>
    public class Test
    {
        public Test(TestRequestRelativePath path, TestRequestContent content, string testScenarioName = null, Dictionary<string, string> additionalHeaders = null, string testName = null, string piid = null)
        {
            this.Path = path;
            this.Content = content;
            this.State = this.FindFirstPIState();
            this.TestScenarioName = testScenarioName;
            this.AdditionalHeaders = additionalHeaders;
            this.TestName = testName;
            this.PIID = piid;
        }

        public Test(PIState state, string piid, TestRequestRelativePath path, TestRequestContent content)
        {
            this.Path = path;
            this.Content = content;
            this.PIID = piid;
            this.State = state;
        }

        public PIState State { get; set; }

        public string PIID { get; set; }

        public string TestName { get; set; }

        public TestRequestRelativePath Path { get; set; }

        public TestRequestContent Content { get; set; }

        public string TestScenarioName { get; set; }

        public Dictionary<string, string> AdditionalHeaders { get; }

        /// <summary>
        /// this function gets the next test operation for any child test
        /// </summary>
        /// <returns>the next state to be tested.</returns>
        public PIState GetNextState()
        {
            if (!(this.State == PIState.None || this.Content == null))
            {
                for (int i = (int)this.State + 1; i < this.Content.ExpectedStatus.Count; i++)
                {
                    if (this.Content.ExpectedStatus[i] != null)
                    {
                        return (PIState)i;
                    }
                }
            }

            return PIState.None;
        }

        /// <summary>
        /// Parses the expected response status from the test content
        /// </summary>
        /// <returns>HttpStatusCode based on PIState</returns>
        public HttpStatusCode GetStatusCode()
        {
            if (this.Content != null)
            {
                return (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), this.Content.ExpectedStatus[(int)this.State]);
            }
            else if (this.State == PIState.None)
            {
                return HttpStatusCode.OK;
            }
            else
            {
                throw new TestException("Missing content for the PIState");
            }
        }

        /// <summary>
        /// Returns an http request method based on the current PIState
        /// </summary>
        /// <returns>HttpMethod based on current PIState</returns>
        public HttpMethod GetHttpMethod()
        {
            if (this.State == PIState.Add || this.State == PIState.Resume || this.State == PIState.IssuerServiceApply)
            {
                return HttpMethod.Post;
            }
            else
            {
                return HttpMethod.Get;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{{ State: {0}, PIID: {1}, Path: {2}, Content: {3} }}",
                this.State.ToString(),
                this.PIID,
                this.Path.ToString(false, this.State, this.PIID),
                (this.Content != null) ? this.Content.ToString() : "N/A");
        }

        private PIState FindFirstPIState()
        {
            if (this.Content != null)
            {
                for (int i = 0; i < this.Content.ExpectedStatus.Count; i++)
                {
                    if (this.Content.ExpectedStatus[i] != null)
                    {
                        return (PIState)i;
                    }
                }
            }

            return PIState.None;
        }
    }
}
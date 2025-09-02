// <copyright file="OutputDescription.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace PidlTest.Diff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using JsonDiff;
    using Microsoft.Commerce.Payments.PXService.ApiSurface.Diff;

    /// <summary>
    /// Collects data for one failed test. The collected data
    /// is then formatted into to be csv compatable
    /// </summary>
    internal class OutputDescription
    {
        private const int ExcelMaxCellLength = 32000;

        private static readonly Regex correlationRegex = new Regex(@"""CorrelationId"":""[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?""");
        private static readonly Regex correlationHeaderRegex = new Regex(@"""x-ms-correlation-id"": ""[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?""");
        private static readonly Regex msTrackingHeaderRegex = new Regex(@"""x-ms-tracking-id"": ""[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?""");

        public bool IsSuccess { get; set; }

        public bool WriteTriagedDiffs { get; set; }

        public PidlIdentity Identity { get; set; } // contains the properties that make up a PXService request

        public string BaseLineResponse { get; set; }

        public string UnderTestResponse { get; set; }    // i.e. OK, InternalServerError, 404

        public Microsoft.Commerce.Payments.PXService.ApiSurface.Diff.Test Criteria { get; set; }

        public DiffDetails ExecutionError { get; set; } // Stack trace

        public List<DiffDetails> ComparisionErrors { get; set; } // JSON payload and custom text  
        
        public string OutputDiff(string baseEnv, string testEnv)
        {
            StringBuilder output = new StringBuilder();
            
            string scenarioNamePI = string.Empty;
            string statePI = string.Empty;
            string id = string.Empty;
            string url = string.Empty;
            string expectedVal = string.Empty;
            string actualVal = string.Empty;
            string triagedStr = string.Empty;

            string[] splitID = this.Identity.Id.Split('.');
            if (Array.IndexOf(splitID, string.Empty) != -1)
            {
                id = string.Join(string.Empty, splitID);
            }
            else
            {
                id = this.Identity.Id;
            }

            if (this.Criteria != null)
            {
                statePI = this.Criteria.State.ToString();
                url = this.Criteria.Path.ToString(baseEnv.Equals("Local", StringComparison.InvariantCultureIgnoreCase) && testEnv.Equals("Local", StringComparison.InvariantCultureIgnoreCase), this.Criteria.State, this.Criteria.PIID);

                if (this.Criteria.Content != null)
                {
                    scenarioNamePI = this.Criteria.Content.Name;                    
                }
            }

            if (this.ExecutionError != null)
            {
                string descriptionParsed = StripCharacters(this.ExecutionError.Description);
                string pathParsed = StripCharacters(this.ExecutionError.JPath);
                List<string> expectedPaginated = Paginate(StripCharacters(this.ExecutionError.Expected));
                List<string> actualPaginated = Paginate(StripCharacters(this.ExecutionError.Actual));

                for (int index = 0; index < Math.Max(1, Math.Max(expectedPaginated.Count, actualPaginated.Count)); index++)
                {
                    descriptionParsed = index > 0 ? string.Format("{0} - Wrapped({1})", descriptionParsed, index) : descriptionParsed;
                    expectedVal = index < expectedPaginated.Count ? expectedPaginated[index] : string.Empty;
                    actualVal = index < actualPaginated.Count ? actualPaginated[index] : string.Empty;

                    if (TestRunner.TriagedDiffSet?.Count > 0)
                    {
                        triagedStr = QueryTriagedDiffString(
                                     TestRunner.TriagedDiffSet,
                                     url,
                                     this.Identity.ResourceName,
                                     id,
                                     this.Identity.Country,
                                     this.Identity.Language,
                                     this.Identity.Partner,
                                     this.Identity.Operation,
                                     this.Identity.Scenario,
                                     this.Identity.Filters,
                                     this.Identity.AllowedPayementMethods,
                                     descriptionParsed,
                                     pathParsed,
                                     expectedVal,
                                     actualVal,
                                     this.BaseLineResponse,
                                     this.UnderTestResponse,
                                     statePI,
                                     scenarioNamePI,
                                     StripCharacters(this.ExecutionError.Data),
                                     baseEnv,
                                     testEnv);
                    }

                    if (!string.IsNullOrEmpty(triagedStr))
                    {
                        this.ExecutionError.Triage = triagedStr;
                    }

                    if (this.WriteTriagedDiffs || string.IsNullOrEmpty(triagedStr))
                    {
                        output.Append(
                        string.Format(
                            "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}~{16}~{17}~{18}~{19}\n",
                            string.IsNullOrEmpty(this.Criteria.TestName) ? url : $"({this.Criteria.TestName}){url}",
                            this.Identity.ResourceName,
                            id,
                            this.Identity.Country,
                            this.Identity.Language,
                            this.Identity.Partner,
                            this.Identity.Operation,
                            this.Identity.Scenario,
                            this.Identity.Filters,
                            this.Identity.AllowedPayementMethods,
                            descriptionParsed,
                            pathParsed,
                            expectedVal,
                            actualVal,
                            this.BaseLineResponse,
                            this.UnderTestResponse,
                            statePI,
                            scenarioNamePI,
                            StripCharacters(this.ExecutionError.Data),
                            triagedStr));
                    }
                }          
            }
            else if (this.ComparisionErrors.Count > 0)
            {
                foreach (DiffDetails err in this.ComparisionErrors)
                {
                    string descriptionParsed = StripCharacters(err.Description);
                    string pathParsed = StripCharacters(err.JPath);
                    List<string> expectedPaginated = Paginate(StripCharacters(err.Expected));
                    List<string> actualPaginated = Paginate(StripCharacters(err.Actual));

                    for (int index = 0; index < Math.Max(1, Math.Max(expectedPaginated.Count, actualPaginated.Count)); index++)
                    {
                        descriptionParsed = index > 0 ? string.Format("{0} - Wrapped({1})", descriptionParsed, index) : descriptionParsed;
                        expectedVal = index < expectedPaginated.Count ? expectedPaginated[index] : string.Empty;
                        actualVal = index < actualPaginated.Count ? actualPaginated[index] : string.Empty;

                        if (TestRunner.TriagedDiffSet?.Count > 0)
                        {
                            triagedStr = QueryTriagedDiffString(
                                         TestRunner.TriagedDiffSet,
                                         url,
                                         this.Identity.ResourceName,
                                         id,
                                         this.Identity.Country,
                                         this.Identity.Language,
                                         this.Identity.Partner,
                                         this.Identity.Operation,
                                         this.Identity.Scenario,
                                         this.Identity.Filters,
                                         this.Identity.AllowedPayementMethods,
                                         descriptionParsed,
                                         pathParsed,
                                         expectedVal,
                                         actualVal,
                                         this.BaseLineResponse,
                                         this.UnderTestResponse,
                                         statePI,
                                         scenarioNamePI,
                                         StripCharacters(err.Data),
                                         baseEnv,
                                         testEnv);
                        }

                        if (!string.IsNullOrEmpty(triagedStr))
                        {
                            err.Triage = triagedStr;
                        }

                        if (this.WriteTriagedDiffs || string.IsNullOrEmpty(triagedStr))
                        {
                            output.Append(
                            string.Format(
                                "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}~{16}~{17}~{18}~{19}\n",
                                string.IsNullOrEmpty(this.Criteria.TestName) ? url : $"({this.Criteria.TestName}){url}",
                                this.Identity.ResourceName,
                                id,
                                this.Identity.Country,
                                this.Identity.Language,
                                this.Identity.Partner,
                                this.Identity.Operation,
                                this.Identity.Scenario,
                                this.Identity.Filters,
                                this.Identity.AllowedPayementMethods,
                                descriptionParsed,
                                pathParsed,
                                expectedVal,
                                actualVal,
                                this.BaseLineResponse,
                                this.UnderTestResponse,
                                statePI,
                                scenarioNamePI,
                                StripCharacters(err.Data),
                                triagedStr));
                        }
                    }
                }
            }

            return output.Length == 0 ? null : output.ToString().Substring(0, output.Length - 1); // takes off the last new line character
        }

        private static string StripCharacters(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                return string.Join(" ", data.Split('~', '\n', '\r'));
            }

            return string.Empty;
        }

        private static List<string> Paginate(string value)
        {
            return Enumerable.Range(0, (int)Math.Ceiling((double)value.Length / (double)ExcelMaxCellLength))
                .Select(i => value.Substring(i * ExcelMaxCellLength, Math.Min(ExcelMaxCellLength, value.Length - (i * ExcelMaxCellLength)))).ToList();
        }

        private static string QueryTriagedDiffString(
            HashSet<string[]> source,
            string url,
            string resourceName,
            string identity,
            string country,
            string language,
            string partner,
            string operation,
            string scenario,
            string filter,
            string allowedPaymentMethods,
            string description,
            string jpath,
            string expected,
            string actual,
            string baseLineResponse,
            string underTestResponse,
            string statePI,
            string scenarioNamePI,
            string data,
            string baseEnv,
            string testEnv)
        {
            var comments = source.Where(line =>
            {
                return line[0] == url &&
                    line[1] == resourceName &&
                    line[2] == identity &&
                    line[3] == country &&
                    line[4] == language &&
                    line[5] == partner &&
                    line[6] == operation &&
                    line[7] == scenario &&
                    line[8] == filter &&
                    line[9] == allowedPaymentMethods &&
                    line[10] == description &&
                    line[11] == jpath &&
                    CompareResponseContent(line[12], expected) &&
                    CompareResponseContent(line[13], actual) &&
                    line[14] == baseLineResponse &&
                    line[15] == underTestResponse &&
                    line[16] == statePI &&
                    line[17] == scenarioNamePI &&
                    CompareData(line[18], data, baseEnv, testEnv);
            }).Select(line =>
            {
                string triageValue = string.Empty;
                try
                {
                    triageValue = line[line.Length - 1];
                }
                catch
                {
                }

                return triageValue;
            });

            return comments?.FirstOrDefault();
        }

        private static bool CompareResponseContent(string triaged, string actual)
        {
            triaged = triaged.Trim();
            actual = actual.Trim();

            if (triaged.Equals(actual, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            triaged = RemoveGuids(triaged);
            actual = RemoveGuids(actual);

            return triaged.Equals(actual, StringComparison.OrdinalIgnoreCase);
        }

        private static string RemoveGuids(string value)
        {
            value = correlationHeaderRegex.Replace(
                value,
                @"""x-ms-correlation-id"":""GUID""");

            value = msTrackingHeaderRegex.Replace(
                value,
                @"""x-ms-tracking-id"":""GUID""");

            return value;
        }

        private static bool CompareData(string baseData, string testData, string baseEnv, string testEnv)
        {
            if (baseData == testData)
            {
                return true;
            }

            if (string.Equals(baseEnv, "SelfHost", StringComparison.InvariantCultureIgnoreCase)
              && string.Equals(testEnv, "SelfHost", StringComparison.InvariantCultureIgnoreCase))
            {
                //// Port will be different on both environments, remove to compare.
                baseData = Regex.Replace(baseData, "127\\.0\\.0\\.1:\\d+\\/", "127.0.0.1/");
                testData = Regex.Replace(testData, "127\\.0\\.0\\.1:\\d+\\/", "127.0.0.1/");

                if (baseData == testData)
                {
                    return true;
                }

                //// Port will be different on both environments, remove to compare.
                baseData = Regex.Replace(baseData, "localhost:\\d+\\/", "localhost/");
                testData = Regex.Replace(testData, "localhost:\\d+\\/", "localhost/");

                if (baseData == testData)
                {
                    return true;
                }

                //// CorrelationId will be different on both requests, remove to compare.
                baseData = correlationRegex.Replace(
                    baseData,
                    @"""CorrelationId"":""GUID""");

                testData = correlationRegex.Replace(
                    testData,
                    @"""CorrelationId"":""GUID""");

                if (baseData == testData)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

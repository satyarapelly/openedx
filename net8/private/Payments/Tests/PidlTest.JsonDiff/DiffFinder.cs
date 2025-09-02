// <copyright file="DiffFinder.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>
namespace PidlTest.JsonDiff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Contains helper functions related to Json data
    /// </summary>
    public static class DiffFinder
    {
        /// <summary>
        /// Compares PIDL json objects
        /// </summary>
        /// <param name="baseline">the baseline/benchmark JSON payload</param>
        /// <param name="undertest">the new/updated JSON payload</param>
        /// <returns>result differneces</returns>
        public static List<DiffDetails> GetPidlDiffs(JToken baseline, JToken undertest)
        {
            // If either baseline or target are empty and the other has an array of node, the first comparison will validate to true.
            // This is an edge case that needs to be handle before recursing into the trees.
            if (baseline.Type == JTokenType.String && undertest.Type == JTokenType.Array)
            {
                var truncateJson = undertest.ToString().Substring(0, 500);

                return new List<DiffDetails>
                {
                    new DiffDetails()
                    {
                        Description = "Edited",
                        DiffType = DiffType.edit,
                        JPath = baseline.Path,
                        Expected = baseline.ToString(),
                        Actual = truncateJson + "[TRUNCATED]"
                    }
                };
            }
            else if (baseline.Type == JTokenType.Array && undertest.Type == JTokenType.String)
            {
                var truncateJson = baseline.ToString().Substring(0, 500);

                return new List<DiffDetails>
                {
                    new DiffDetails()
                    {
                        Description = "Edited",
                        DiffType = DiffType.edit,
                        JPath = baseline.Path,
                        Expected = truncateJson + "[TRUNCATED]",
                        Actual = undertest.ToString()
                    }
                };
            }

            return GetPidlDiffsInternal(baseline, undertest);
        }

        /// <summary>
        /// Compares PIDL json objects
        /// </summary>
        /// <param name="baseline">the baseline/benchmark JSON payload</param>
        /// <param name="undertest">the new/updated JSON payload</param>
        /// <returns>result differneces</returns>
        private static List<DiffDetails> GetPidlDiffsInternal(JToken baseline, JToken undertest)
        {
            List<DiffDetails> foundDiffs = new List<DiffDetails>();

            if (baseline != null && undertest != null)
            {
                if (baseline is JValue && undertest is JValue)
                {
                    if (!JValue.DeepEquals(baseline, undertest))
                    {
                        AddDiff("Edited", DiffType.edit, baseline.Path, baseline.ToString(), undertest.ToString(), ref foundDiffs);
                    }
                }
                else if (baseline.Type == JTokenType.Object && undertest.Type == JTokenType.Object)
                {
                    JObject baselineJObject = baseline as JObject;
                    JObject underTestJObject = undertest as JObject;

                    foreach (JProperty prop in baselineJObject.Properties())
                    {
                        if (!Constants.DiffTest.SkipPropertyComparision.Contains(prop.Name))
                        {
                            if (underTestJObject[prop.Name] != null)
                            {
                                foundDiffs.AddRange(GetPidlDiffsInternal(baselineJObject[prop.Name], underTestJObject[prop.Name]));
                            }
                            else
                            {
                                AddDiff("Deleted", DiffType.delete, prop.Path, baselineJObject[prop.Name].ToString(), string.Empty, ref foundDiffs);
                            }
                        }
                    }

                    foreach (JProperty prop in underTestJObject.Properties())
                    {
                        if (!Constants.DiffTest.SkipPropertyComparision.Contains(prop.Name))
                        {
                            if (baselineJObject[prop.Name] == null)
                            {
                                AddDiff("Added", DiffType.add, prop.Path, string.Empty, underTestJObject[prop.Name].ToString(), ref foundDiffs);
                            }
                        }
                    }
                }
                else if (baseline.Type == JTokenType.Array && undertest.Type == JTokenType.Array)
                {
                    JArray baselineToken = baseline as JArray;
                    JArray underTestToken = undertest as JArray;

                    int m = baselineToken.Count();
                    int n = underTestToken.Count();
                    Dictionary<int, int> coordinate = new Dictionary<int, int>();
                    int pos = 0;

                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (baselineToken[i].ToString() == underTestToken[j].ToString())
                            {
                                coordinate.Add(i, j);
                                break;
                            }
                        }
                    }

                    foreach (int key in coordinate.Keys)
                    {
                        if (pos > coordinate[key])
                        {
                            AddDiff("Moved", DiffType.move, underTestToken[coordinate[key]].Parent.Path, underTestToken[coordinate[key]].ToString(), underTestToken[coordinate[key]].ToString(), ref foundDiffs);
                            break;
                        }
                        else
                        {
                            pos = coordinate[key];
                        }
                    }

                    if (baselineToken.Count() != underTestToken.Count())
                    {
                        for (int i = 0; i < m; i++)
                        {
                            if (!coordinate.ContainsKey(i))
                            {
                                AddDiff("Deleted", DiffType.delete, baselineToken[i].Path, baselineToken[i].ToString(), string.Empty, ref foundDiffs);
                            }
                        }

                        for (int j = 0; j < n; j++)
                        {
                            if (!coordinate.ContainsValue(j))
                            {
                                AddDiff("Added", DiffType.add, underTestToken[j].Path, string.Empty, underTestToken[j].ToString(), ref foundDiffs);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < m; i++)
                        {
                            if (!coordinate.ContainsKey(i))
                            {
                                foundDiffs.AddRange(GetPidlDiffsInternal(baselineToken[i], underTestToken[i]));
                            }
                        }
                    }
                }
            }

            return foundDiffs;
        }

        /// <summary>
        /// Update the test result differences added/deleted/edited
        /// </summary>
        /// <param name="description">change description</param>
        /// <param name="type">change type</param>
        /// <param name="jsonPath">JSON node path</param>
        /// <param name="expectVal">node val on prod</param>
        /// <param name="actualtVal">node val on ppe</param>
        /// <param name="results">difference results</param>
        private static void AddDiff(string description, DiffType type, string jsonPath, string expectVal, string actualtVal, ref List<DiffDetails> results)
        {
            results.Add(
                new DiffDetails()
                {
                    Description = description,
                    DiffType = type,
                    JPath = jsonPath,
                    Expected = expectVal,
                    Actual = actualtVal
                });
        }
    }
}

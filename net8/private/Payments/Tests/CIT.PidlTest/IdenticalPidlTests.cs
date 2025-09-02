using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PidlTest.JsonDiff;

namespace CIT.PidlTest.DiffTest
{
    [TestClass]
    public class IdenticalPidlTests
    {
        JToken baselineTest = LoadJson.ReadJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\ACH.Add.US.en-us.Baseline.json"));
        JToken underTest = LoadJson.ReadJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\ACH.Add.US.en-us.json"));

        [TestMethod]
        public void IdenticalPidls()
        {
            var results = DiffFinder.GetPidlDiffs(baselineTest, underTest);

            Assert.AreEqual(0, results.Count);
        }        
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PidlTest.JsonDiff;
using System.Collections.Generic;

namespace CIT.PidlTest.DiffTest
{
    [TestClass]
    public class ModifiedPidlTests
    {
        [TestMethod]
        public void ObjectPropertyAdded()
        {
            string left = "{\"a\" : 1, \"b\" : 2}";
            string right = "{\"a\" : 1, \"b\" : 2, \"c\": 3}";

            var results = CompareJsonStrings(left, right);

            Assert.AreEqual(1, results.Count);

            Assert.AreEqual(results[0].JPath, "c");
            Assert.AreEqual(results[0].Description, "Added");

            Assert.AreEqual(results[0].Expected, string.Empty);
            Assert.AreEqual(results[0].Actual, "3");           
        }

        [TestMethod]
        public void ObjectPropertyModified()
        {
            string left = "{\"a\" : 1, \"b\" : 2, \"c\" : 3}";
            string right = "{\"a\" : 1, \"b\" : 4, \"c\": 5}";

            var results = CompareJsonStrings(left, right);

            Assert.AreEqual(2, results.Count);

            Assert.AreEqual(results[0].JPath, "b");
            Assert.AreEqual(results[0].Description, "Edited");
            Assert.AreEqual(results[0].Expected, "2");
            Assert.AreEqual(results[0].Actual, "4");

            Assert.AreEqual(results[1].JPath, "c");
            Assert.AreEqual(results[1].Description, "Edited");
            Assert.AreEqual(results[1].Expected, "3");
            Assert.AreEqual(results[1].Actual, "5");
        }

        [TestMethod]
        public void ObjectPropertyDeleted()
        {
            string left = "{\"a\" : 1, \"b\" : 2}";
            string right = "{\"a\" : 1}";

            var results = CompareJsonStrings(left, right);

            Assert.AreEqual(1, results.Count);

            Assert.AreEqual(results[0].JPath, "b");
            Assert.AreEqual(results[0].Description, "Deleted");

            Assert.AreEqual(results[0].Expected, "2");
            Assert.AreEqual(results[0].Actual, string.Empty);
        }

        [TestMethod]
        public void ArrayElementAdded()
        {
            string left = "{\"a\" : 1, \"b\" : 2, \"c\" : [{\"d\" : 3}]}";
            string right = "{\"a\" : 1, \"b\" : 2, \"c\" : [{\"d\" : 3}, {\"e\" : 4}]}";

            var results = CompareJsonStrings(left, right);

            Assert.AreEqual(1, results.Count);

            Assert.AreEqual(results[0].JPath, "c[1]");
            Assert.AreEqual(results[0].Description, "Added");
            Assert.AreEqual(results[0].Expected, string.Empty);
        }

        [TestMethod]
        public void ArrayElementModified()
        {
            string left = "{\"a\" : 1, \"b\" : 2, \"c\" : [{\"d\" : 3, \"e\" : 4}]}";
            string right = "{\"a\" : 1, \"b\" : 2, \"c\" : [{\"d\" : 3, \"e\" : 5}]}";
            
            var results = CompareJsonStrings(left, right);

            Assert.AreEqual(1, results.Count);

            Assert.AreEqual(results[0].JPath, "c[0].e");
            Assert.AreEqual(results[0].Description, "Edited");
            Assert.AreEqual(results[0].Expected, "4");
            Assert.AreEqual(results[0].Actual, "5");
        }

        [TestMethod]
        public void ArrayElementDeleted()
        {
            string left = "[{\"a\" : 1 }, {\"b\" : 2 } , {\"c\" : 3}]";
            string right = "[{\"a\" : 1 }, {\"b\" : 2 }]";

            var results = CompareJsonStrings(left, right);

            Assert.AreEqual(1, results.Count);

            Assert.AreEqual(results[0].JPath, "[2]");
            Assert.AreEqual(results[0].Description, "Deleted");
            Assert.AreEqual(results[0].Actual, string.Empty);
        }

        [TestMethod]
        public void ArrayElementReordered()
        {
            string left = "[{\"a\" : 1 }, {\"b\" : 2 } , {\"c\" : 3}]";
            string right = "[{\"a\" : 1 }, {\"c\" : 3}, {\"b\" : 2 }]";

            var results = CompareJsonStrings(left, right);

            Assert.AreEqual(1, results.Count);

            Assert.AreEqual(results[0].JPath, string.Empty);
            Assert.AreEqual(results[0].Description, "Moved");
        }

        private static List<DiffDetails> CompareJsonStrings(string left, string right)
        {
            JToken leftToken = JToken.Parse(left);
            JToken rightToken = JToken.Parse(right);

            return DiffFinder.GetPidlDiffs(leftToken, rightToken);
        }
    }
}

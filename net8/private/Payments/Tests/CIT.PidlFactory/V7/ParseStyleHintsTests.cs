using Microsoft.Commerce.Payments.PidlFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIT.PidlFactory.V7
{
    [TestClass]
    public class ParseStyleHintsTests
    {
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(",")]
        [DataRow(", ,")]
        [DataRow("\t")]
        [DataRow("\r")]
        [DataRow("\n")]
        [DataTestMethod]
        public void NullOrEmptyStyleHintsReturnNull(string commaSeparatedStylesList)
        {
            var styles = PidlFactoryHelper.ParseStyleHints(commaSeparatedStylesList);

            Assert.IsNull(styles);
        }

        [DataRow("style-one,style-two")]
        [DataRow(" style-one, style-two")]
        [DataRow("style-one ,style-two ")]
        [DataRow("style-one,style-two\t")]
        [DataRow("style-one,style-two\r")]
        [DataRow("style-one,style-two\n")]
        [DataRow("style-one,style-two\r\n")]
        [DataTestMethod]
        public void ParsedStyleHintsAreTrimmed(string commaSeparatedStylesList)
        {
            List<string> styles = PidlFactoryHelper.ParseStyleHints(commaSeparatedStylesList).ToList<string>();

            Assert.AreEqual(2, styles.Count, "Styles parsed count");
            Assert.AreEqual("style-one", styles[0], "First style in the list");
            Assert.AreEqual("style-two", styles[1], "Second style in the list");
        }

        [DataRow("style-one,,style-two")]
        [DataRow("style-one, ,style-two")]
        [DataRow("style-one,\t,style-two")]
        [DataRow("style-one,\r,style-two")]
        [DataRow("style-one,\n,style-two")]
        [DataRow("style-one,\r\n,style-two")]
        [DataTestMethod]
        public void EmptyStyleHintsAreRemoved(string commaSeparatedStylesList)
        {
            List<string> styles = PidlFactoryHelper.ParseStyleHints(commaSeparatedStylesList).ToList<string>();

            Assert.AreEqual(2, styles.Count, "Styles parsed count");
            Assert.AreEqual("style-one", styles[0], "First style in the list");
            Assert.AreEqual("style-two", styles[1], "Second style in the list");
        }
    }
}

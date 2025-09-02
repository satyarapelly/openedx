// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]    
    public class PidlTransformationFactoryTests
    {
        [TestMethod]
        public void PidlTransformationFactoryTransformPropertyValidPhoneNumbersAreTransformed()
        {
            PidlTransformationParameter pidlTransformationParameter = new PidlTransformationParameter();
            PidlTransformationResult<string> pidlTransformationResult = null;

            pidlTransformationParameter.TransformationTarget = "forSubmit";
            pidlTransformationParameter.PropertyName = "msisdn";
            pidlTransformationParameter.PidlIdentity = new Dictionary<string, string>();
            pidlTransformationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.DescriptionType] = "data";
            pidlTransformationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.Type] = "mobile_billing_non_sim_details";

            using (StreamReader sw = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\PhoneNumberTransformationTestInfo.csv")))
            {
                // Skip headers
                sw.ReadLine();
                string line;
                while ((line = sw.ReadLine()) != null)
                {
                    // Columns in csv file lines are currently in this order: Input, Country, ExpectedTransformedValue, Notes
                    string[] rowValues = line.Split(',');

                    pidlTransformationParameter.Value = rowValues[0];
                    pidlTransformationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.Country] = rowValues[1];
                    pidlTransformationParameter.PidlIdentity[TestConstants.DescriptionIdentityFields.Operation] = rowValues[2];

                    List<string> exposedFlightFeatures = new List<string>(rowValues[4].Split(';'));

                    pidlTransformationResult = PIDLTransformationFactory.TransformProperty(pidlTransformationParameter, exposedFlightFeatures);

                    Assert.AreEqual(PidlExecutionResultStatus.Passed, pidlTransformationResult.Status, string.Format("Failure on test case, input: {0}, country: {1}, and expected transform: {2}. Here are some notes: {3}", rowValues[0], rowValues[1], rowValues[3], rowValues[5]));
                    Assert.AreEqual(rowValues[3], pidlTransformationResult.TransformedValue, string.Format("Failure on test case, input: {0}, country: {1}, and expected transform: {2}. Here are some notes: {3}", rowValues[0], rowValues[1], rowValues[3], rowValues[5]));
                }
            }
        }
    }
}

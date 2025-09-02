// <copyright file="PxPidlSteps.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Spec.PXService.Steps
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using TechTalk.SpecFlow;
    using Tests.Common.Model;
    using Tests.Common.Model.Pidl;
    using Tests.Common.Model.PX;

    public class PxPidlSteps
    {
        public PxPidlSteps()
        {
            PxRequestExecutor = new PxRequestExecutor();
        }

        public List<PIDLResource> PidlDescriptions { get; set; }

        public PxRequestExecutor PxRequestExecutor { get; private set; }

        [Given(@"user session data is")]
        public void GivenUserSessionDataIs(Table table)
        {
            var session = new ExpandoObject() as IDictionary<string, object>;

            foreach (var tableRow in table.Rows)
            {
                var key = tableRow["key"];
                var value = tableRow["value"];
                session.Add(key, value);
            }
            
            this.PxRequestExecutor.PxServiceRequestBuilder.PaymentSessionOrData = JsonConvert.SerializeObject(session);
        }

        [Given(@"user wants to select a payment method")]
        public void GivenUserSelectAPaymentMethod()
        {
            this.PxRequestExecutor.PxServiceRequestBuilder.Operation = "select";
        }

        [Given(@"user is located in (.*)")]
        public void GivenUserIsLocatedIn(string country)
        {
            this.PxRequestExecutor.PxServiceRequestBuilder.Country = country;
        }

        [Given(@"user locale is (.*)")]
        public void GivenUserLocaleIs(string language)
        {
            this.PxRequestExecutor.PxServiceRequestBuilder.Language = language;
        }

        [Given(@"user has an existing order (.*)")]
        public void GivenAnTheUserHasAnExistingOrder(string orderId)
        {
            if (!orderId.IsNullOrEmpty())
            {
                this.PxRequestExecutor.PxServiceRequestBuilder.OrderId = $"{{{orderId}}}";
            }
        }

        [StepArgumentTransformation("(enabled|disabled)")]
        [Given(@"prerequisites completion is (.*)")]
        public void GivenRequestSpecifiesRequirementCompletion(string completePrerequisitesFlag)
        {
            var isCompletePrerequitesEnabled = completePrerequisitesFlag.ToBool("enabled", "disabled");

            if (isCompletePrerequitesEnabled)
            {
                this.PxRequestExecutor.PxServiceRequestBuilder.CompletePrerequisites = "true";
            }
        }

        [Given(@"user with an MSA profile email: (.*) and puid: (.*)")]
        public void GivenAUserWithAnMSAProfileEmailAndPuid(string email, string puid)
        {
            this.PxRequestExecutor.PxServiceRequestBuilder.SetMSAProfile(email, puid);
        }

        [StepArgumentTransformation("(enabled|disabled)")]
        [Given(@"shipping flight is (.*)")]
        public void GivenShippingFlightIsEnabled(string p0)
        {
            var isShippingV3Enabled = p0.ToBool("enabled", "disabled");

            if (isShippingV3Enabled)
            {
                this.PxRequestExecutor.PxServiceRequestBuilder.IsShippingV3Enabled = true;
            }
        }

        [Given(@"user (.*) is signed in")]
        public void GivenUserIsSignedIn(string p0)
        {
            this.PxRequestExecutor.PxServiceRequestBuilder.AccountId = p0;
        }

        [When(@"payment method descriptions load")]
        public async Task WhenThePaymentMethodsPageLoadsUp()
        {
            this.PidlDescriptions = await this.PxRequestExecutor.ExecuteRequest<List<PIDLResource>>("paymentMethodDescriptions");
        }

        [When(@"challenge descriptions load")]
        public async Task WhenTheChallengeDescriptions()
        {
            this.PidlDescriptions = await this.PxRequestExecutor.ExecuteRequest<List<PIDLResource>>("challengeDescriptions");
        }

        [Then(@"challenge is succesfull")]
        public void ThenChallengeIsSuccesfull()
        {
            foreach (PIDLResource resource in this.PidlDescriptions)
            {
                var session = JsonConvert.DeserializeObject<PaymentSession>(this.PidlDescriptions[0].ClientAction.Context.ToString());
                Assert.AreEqual(session.ChallengeStatus, PaymentChallengeStatus.Succeeded);
            }
        }
        
        [Then(@"multiple payment methods are available")]
        public void ThenMultiplePaymentMethodsAreAvailable()
        {
            PropertyDisplayHint paymentMethodDisplayHint = PidlDescriptions[0].GetDisplayHintById("paymentMethod") as PropertyDisplayHint;
            Assert.IsNotNull(paymentMethodDisplayHint);
            Assert.IsTrue(paymentMethodDisplayHint.PossibleValues.Count > 1);
        }

        public void AssertCompleteRequirementsIsLinked(ButtonDisplayHint submitButton, PIDLResource linkedPidl, string submitUrl)
        {
            Assert.IsNotNull(submitButton, "Submit button is expected");
            RestLink submitButtonContext = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(submitButton.Action.Context));
            Assert.IsTrue(string.Equals("POST", submitButtonContext.Method, StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(string.Equals(submitUrl, submitButtonContext.Href, StringComparison.OrdinalIgnoreCase));

            if (this.PxRequestExecutor.PxServiceRequestBuilder.IsShippingV3Enabled)
            {
                Assert.IsTrue(string.Equals(((PropertyDescription)linkedPidl.DataDescription["addressType"]).DefaultValue, "shipping_v3", StringComparison.OrdinalIgnoreCase));
                Assert.IsTrue(linkedPidl.DataDescription.ContainsKey("first_name"));
                Assert.IsTrue(linkedPidl.DataDescription.ContainsKey("last_name"));
            }
            else
            {
                Assert.IsTrue(string.Equals(((PropertyDescription)linkedPidl.DataDescription["addressType"]).DefaultValue, "billing", StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
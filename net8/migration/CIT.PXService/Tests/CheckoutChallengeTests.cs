namespace CIT.PXService.Tests
{
    using Microsoft.Commerce.Payments.Common.Helper;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;

    [TestClass]
    public class CheckoutChallengeTests
    {
        [Ignore]
        [TestMethod]
        public void GetChallengeRedirectAndStatusCheckDescriptionForCheckoutTest(string partner)
        {
            string checkoutId = "459ffe98-64cd-4ade-9edb-06bda672e259";
            string paymentProviderId = "stripe";
            string checkoutRedirectUrl = string.Empty;
            string partnerRedirectUrl = "https://teams.microsoft.com";

            List<PIDLResource> checkoutChallengePidl = PIDLResourceFactory.GetChallengeRedirectAndStatusCheckDescriptionForCheckout(checkoutId, partner, paymentProviderId, checkoutRedirectUrl, partnerRedirectUrl);

            Assert.AreEqual(checkoutChallengePidl[0].ClientAction.ActionType, Microsoft.Commerce.Payments.PXCommon.ClientActionType.Pidl);

            DisplayHintAction pollAction = checkoutChallengePidl[0].DisplayPages[0].Action;
            Assert.IsNotNull(pollAction);
            var pollActionContext = checkoutChallengePidl[0].ClientAction.Context as PollActionContext;
            Assert.IsTrue(string.Equals(pollActionContext.Href, "https://{pifd-endpoint}/checkoutsEx/459ffe98-64cd-4ade-9edb-06bda672e259/status?paymentProviderId=stripe"));

            DisplayHintAction action;
            pollActionContext.ResponseActions.TryGetValue("Failed", out action);
            Assert.AreEqual(action.ActionType.ToString(), "handleFailure");
        }
    }
}
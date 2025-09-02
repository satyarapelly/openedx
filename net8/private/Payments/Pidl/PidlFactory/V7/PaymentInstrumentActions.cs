// <copyright file="PaymentInstrumentActions.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    public enum PIActionType
    {
        AddResource,
        UpdateResource,
        ReplaceResource,
        DeleteResource,
        HandleChallenge,
        SelectResourceType,
        SelectSingleResource,
        SelectResource,
        CollectResourceInfo,
        AddMoney,
        Redeem,
        Shop,
        ViewMandate,
        EditPaymentInstrument,
        AddPaymentInstrument,
        ApplyPaymentInstrument
    }

    public static class PaymentInstrumentActions
    {
        private static Dictionary<PIActionType, PaymentInstrumentAction> paymentInstrumentActions = new Dictionary<PIActionType, PaymentInstrumentAction>
            {
                { PIActionType.EditPaymentInstrument, new PaymentInstrumentAction("editPaymentInstrument", "Expired") },
                { PIActionType.UpdateResource, new PaymentInstrumentAction("updateResource", "Expired") },
                { PIActionType.HandleChallenge, new PaymentInstrumentAction("handleChallenge", "Verify") },
                { PIActionType.SelectResource, new PaymentInstrumentAction("selectResource", "Change") }
            };

        public static Dictionary<PIActionType, PaymentInstrumentAction> GetPaymentInstrumentActions()
        {
            return paymentInstrumentActions;
        }

        public static string ToString(PIActionType action)
        {
            string actionStr = action.ToString();
            return char.ToLower(actionStr[0]) + actionStr.Substring(1);
        }
    }
}

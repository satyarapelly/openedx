// <copyright file="InlineLocalCardDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the InlineLocalCardDetails, which makes local card details template inline.
    /// </summary>
    internal class InlineLocalCardDetails : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                MakeInlineLocalCardDetails
            };
        }

        internal static void MakeInlineLocalCardDetails(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (string.Equals(featureContext.OperationType, V7.Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidlResource in inputResources)
                {                    
                    PropertyDisplayHint listPI = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentInstrumentListPi) as PropertyDisplayHint;

                    if (listPI != null)
                    {
                        var options = listPI.PossibleOptions;

                        // for each option in options iterate through values
                        foreach (var option in options)
                        {
                            SelectOptionDescription value = option.Value;
                            GroupDisplayHint content = value?.DisplayContent;

                            // return element from content.members where displayId is paymentInstrumentItemWalletCardGroup
                            GroupDisplayHint walletCardGroup = content?.Members?.FirstOrDefault(m => m.HintId == Constants.DisplayHintIds.PaymentInstrumentItemWalletCardGroup) as GroupDisplayHint;
                            GroupDisplayHint detailsGroup = walletCardGroup?.Members?.FirstOrDefault(m => m.HintId == Constants.DisplayHintIds.PaymentInstrumentItemWalletDetailsGroup) as GroupDisplayHint;

                            // Adjust paymentInstrumentItemWalletDetailsGroup members to group the last two members.  This makes it possible for
                            // partner to style in their desired way (see PR https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX/pullrequest/10606998)
                            if (detailsGroup != null && detailsGroup.Members.Count == 3)
                            {
                                GroupDisplayHint paymentInstrumentItemWalletColumnGroup = new GroupDisplayHint()
                                {
                                    HintId = Constants.DisplayHintIds.PaymentInstrumentItemWalletColumnGroup,
                                    ContainerDisplayType = Constants.DisplayHintTypes.Group,
                                    Members = new List<DisplayHint>()
                                {
                                    detailsGroup.Members[1],
                                    detailsGroup.Members[2]
                                }
                                };
                                detailsGroup.Members.RemoveRange(1, 2);
                                detailsGroup.Members.Add(paymentInstrumentItemWalletColumnGroup);
                            }
                        }
                    }
                }
            }
        }
    }
}
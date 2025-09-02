// <copyright file="CustomizeElementLocation.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    /// <summary>
    /// Class representing the CustomizeElementLocation, which is to change the location of PIDL elements
    /// </summary>
    internal class CustomizeElementLocation : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ChangeElementLocation
            };
        }

        internal static void ChangeElementLocation(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeElementLocation, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    foreach (PIDLResource pidlResource in inputResources)
                    {
                        if (displayHintCustomizationDetail.MoveOrganizationNameBeforeEmailAddress)
                        {
                            ReorderDisplayLocations(pidlResource, Constants.DisplayHintIds.HapiEmail, Constants.DisplayHintIds.HapiCompanyName);
                        }

                        if (displayHintCustomizationDetail.MoveCardNumberBeforeCardHolderName)
                        {
                            if (featureContext?.PaymentMethodfamily != null && !string.Equals(featureContext?.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase))
                            {
                                return;
                            }

                            ExchangeCardHolderNameAndCardNumberLocation(pidlResource);
                        }

                        // The feature MoveLastNameBeforeFirstName is implemented to change the sequence position of the last name before the first name.
                        if (displayHintCustomizationDetail?.MoveLastNameBeforeFirstName != null && bool.Parse(displayHintCustomizationDetail?.MoveLastNameBeforeFirstName.ToString()))
                        {
                            if (featureContext?.ResourceType != null && string.Equals(featureContext?.TypeName, Constants.AddressTypes.HapiV1, StringComparison.OrdinalIgnoreCase))
                            {
                                ExchangeFirstNameAndLastNumberLocation(pidlResource);
                            }
                        }
                    }
                }
            }
        }

        private static void ExchangeCardHolderNameAndCardNumberLocation(PIDLResource pidlResource)
        {
            if (pidlResource?.DisplayPages != null)
            {
                foreach (PageDisplayHint displayPage in pidlResource.DisplayPages)
                {
                    if (displayPage.DisplayName == Constants.PageDisplayHintIds.AccountDetailsPageDisplayName)
                    {
                        var numberDisplayHint = pidlResource.Identity["type"] == "amex" ? Constants.DisplayHintIds.AmexNumberDisplayHintId : Constants.DisplayHintIds.NumberDisplayHintId;

                        SwapFieldMembersPositions(displayPage.Members, Constants.DisplayHintIds.NameDisplayHintId, numberDisplayHint);
                    }
                    else if (displayPage.DisplayName == Constants.PageDisplayHintIds.AccountSummaryPageDisplayName)
                    {
                        GroupDisplayHint creditCardGroupMembers = displayPage.Members.Find(displayHint => displayHint.HintId == Constants.DisplayHintIds.CreditCardMembersHintId) as GroupDisplayHint;

                        if (creditCardGroupMembers != null)
                        {
                            SwapFieldMembersPositions(creditCardGroupMembers.Members, Constants.DisplayHintIds.CreditCardGroupLine1HintId, Constants.DisplayHintIds.CreditCardGroupLine2HintId);
                        }
                    }
                }
            }
        }

        // This method is utilized to swap the sequence of the first name and last name.
        private static void ExchangeFirstNameAndLastNumberLocation(PIDLResource pidlResource)
        {
            if (pidlResource?.DisplayPages != null)
            {
                foreach (PageDisplayHint displayPage in pidlResource.DisplayPages)
                {
                    SwapFieldMembersPositions(displayPage.Members, Constants.DisplayHintIds.HapiFirstName, Constants.DisplayHintIds.HapiLastName);

                    GroupDisplayHint firstNameAndLastNameMemberGroup = displayPage.Members.Find(
                        displayHint => displayHint.HintId == Constants.GroupDisplayHintIds.HapiFirstNameLastNameGroup) as GroupDisplayHint;

                    if (firstNameAndLastNameMemberGroup != null)
                    {
                        SwapFieldMembersPositions(firstNameAndLastNameMemberGroup.Members, Constants.DisplayHintIds.HapiFirstName, Constants.DisplayHintIds.HapiLastName);
                    }
                }
            }
        }

        // Swap the sequence of the members.
        private static void SwapFieldMembersPositions(List<DisplayHint> members, string firstMembersHintId, string lastMembersHintId)
        {
            int firstMemberSequencePosition = members.FindIndex(displayHint => displayHint.HintId == firstMembersHintId);
            int lastMemberSequencePosition = members.FindIndex(displayHint => displayHint.HintId == lastMembersHintId);

            // only swap if first member is currently before last member.
            if (lastMemberSequencePosition != -1 && firstMemberSequencePosition != -1 && firstMemberSequencePosition < lastMemberSequencePosition)
            {
                DisplayHint temp = members[lastMemberSequencePosition];
                members[lastMemberSequencePosition] = members[firstMemberSequencePosition];
                members[firstMemberSequencePosition] = temp;
            }
        }

        private static void ReorderDisplayLocations(PIDLResource pidlResource, string firstDisplayHint, string secondDisplayHint)
        {
            foreach (PageDisplayHint displayPage in pidlResource.DisplayPages ?? Enumerable.Empty<PageDisplayHint>())
            {
                SwapFieldMembersPositions(displayPage.Members, firstDisplayHint, secondDisplayHint);
            }
        }
    }
}
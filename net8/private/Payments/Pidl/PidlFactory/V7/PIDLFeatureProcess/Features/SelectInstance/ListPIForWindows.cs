// <copyright file="ListPIForWindows.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class ListPIForWindows : IFeature
    {
        private static Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints = new Dictionary<string, Dictionary<string, IEnumerable<string>>>()
        {
            {
                "paymentInstrumentListPi", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "height-auto", "height-inner-auto", "selection-height-auto", "selection-border-bottom-grey", "selection-padding-vertical-none" } }
                }
            },
            {
                "paymentInstrumentItemGroup", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "height-fill" } }
                }
            },
            {
                "paymentInstrumentItemCCGroup", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "height-fill", "alignment-vertical-center", "margin-start-small" } }
                }
            },
            {
                "paymentInstrumentItemEwalletGroup", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "height-fill", "alignment-vertical-center", "margin-start-small" } }
                }
            },
            {
                "paymentInstrumentItemWalletCardGroup", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "height-fill", "alignment-vertical-center", "margin-start-small" } }
                }
            },
            {
                "paymentInstrumentItemNonCCNonCheckGroup", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "height-fill", "alignment-vertical-center", "margin-start-small" } }
                }
            },
            {
                "newPaymentMethodGroup", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "height-fill", "alignment-vertical-center", "margin-start-small" } }
                }
            },
            {
                "paymentInstrumentItemCCLogo", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "image-small" } }
                }
            },
            {
                "paymentInstrumentItemCCInfoGroup", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "gap-small" } }
                }
            },
            {
                "paymentInstrumentItemWalletDetailsGroup", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "gap-small", "alignment-vertical-center" } }
                }
            },
            {
                "paymentInstrumentItemCCDetailsGroup", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "paymentInstrumentItemAccountHolderName_", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "text-truncate", "margin-end-medium" } }
                }
            },
            {
                "paymentInstrumentItemAdditionalInfoText_", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "width-fill", "text-truncate", "text-alignment-center" } }
                }
            },
            {
                "expiredTextGroup", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "width-triquarter", "alignment-end" } }
                }
            },
            {
                "expiredText_", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "text-alert", "margin-end-medium", "text-truncate" } }
                }
            },
            {
                "circlePlusIcon_", new Dictionary<string, IEnumerable<string>>
                {
                    { string.Empty, new List<string> { "font-size-small", "margin-end-x-small", "margin-start-small", "text-line-height-none", "font-family-segoe-mdl2-assets" } }
                }
            },
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ProcessListPIForWindows,
            };
        }

        internal static void ProcessListPIForWindows(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext == null || featureContext.ResourceType != Constants.ResourceTypes.PaymentMethod)
            {
                return;
            }

            foreach (PIDLResource pidl in inputResources)
            {
                List<DisplayHint> paymentInstrumentItemEwalletGroup = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(pidl, "paymentInstrumentItemEwalletGroup");
                List<DisplayHint> newPaymentMethodGroup = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(pidl, "newPaymentMethodGroup");

                FeatureHelper.ConvertToGroupDisplayHint(paymentInstrumentItemEwalletGroup, "inline");
                FeatureHelper.ConvertToGroupDisplayHint(newPaymentMethodGroup, "inline");

                TextDisplayHint circlePlusIcon = new TextDisplayHint()
                {
                    HintId = Constants.ListPIDisplayHintIds.CirclePlusIcon,
                    DisplayContent = Constants.UnicodeValues.PlusCircle,
                };

                foreach (DisplayHint displayHint in newPaymentMethodGroup)
                {
                    ContainerDisplayHint newPMGroup = displayHint as ContainerDisplayHint;

                    if (newPMGroup != null)
                    {
                        newPMGroup.Members.Insert(0, circlePlusIcon);
                    }
                }

                FeatureHelper.PassStyleHints(pidl, styleHints, featureContext);

                List<DisplayHint> paymentInstrumentItemWalletCardGroup = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(pidl, "paymentInstrumentItemWalletCardGroup");
                foreach (DisplayHint piItemWalletGroupChild in paymentInstrumentItemWalletCardGroup)
                {
                    List<DisplayHint> paymentInstrumentItemAccountHolderName_ = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(piItemWalletGroupChild as ContainerDisplayHint, "paymentInstrumentItemAccountHolderName_");

                    foreach (DisplayHint displayHint in paymentInstrumentItemAccountHolderName_)
                    {
                        displayHint.StyleHints = new List<string> { "text-truncate" };
                    }
                }
            }
        }
    }
}

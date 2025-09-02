// <copyright file="QRCodeRedirection.cs" company="Microsoft">Copyright (c) Microsoft 2023. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration.Generators
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PXCommon;

    public class QRCodeRedirection : IPIDLGenerator<ClientAction>
    {
        public static bool ShouldUseVenmoQRCodeTemplate(PaymentExperienceSetting setting, string family, string type)
        {
            try
            {
                return string.Equals(setting?.RedirectionPattern, Constants.RedirectionPatterns.QRCode, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(type, Constants.PaymentMethodType.Venmo, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(family, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public ClientAction Generate(PIDLGeneratorContext context)
        {
            ClientAction clientAction = new ClientAction(ClientActionType.Pidl);

            clientAction.Context = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(
                context.PaymentInstrument,
                context.Language,
                challengeDescriptionType: context.DescriptionType,
                context.Partner,
                context.ClassicProduct, 
                context.BillableAccountId,
                context.EmailAddress,
                context.CompletePrerequisites,
                context.Country,
                context.ExposedFlightFeatures,
                context.SessionId,
                context.Scenario,
                shortUrl: context.ShortUrl?.ToString(),
                setting: context.PartnerSetting,
                context: context);

            return clientAction;
        }
    }
}

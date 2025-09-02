// <copyright file="ClientActionGenerationFactory.cs" company="Microsoft">Copyright (c) Microsoft 2023. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration.Generators;
    using Microsoft.Commerce.Payments.PXCommon;

    internal class ClientActionGenerationFactory : IPIDLGenerationFactory<ClientAction>
    {
       private readonly Dictionary<string, IPIDLGenerator<ClientAction>> clientActionGenerators = new Dictionary<string, IPIDLGenerator<ClientAction>>()
        {
            { V7.Constants.RedirectionPatterns.QRCode, new QRCodeRedirection() },
            { V7.Constants.RedirectionPatterns.FullPage, new FullPageRedirection() },
            { V7.Constants.RedirectionPatterns.Inline, new InlineRedirection() },
            { V7.Constants.RedirectionPatterns.IFrame, new IFrameRedirection() },
        };

        public IPIDLGenerator<ClientAction> GetPIDLGenerator(PIDLGeneratorContext context)
        {
            string redirectPattern = TemplateHelper.GetRedirectionPatternFromPartnerSetting(context.PartnerSetting, context.ResourceType, context.PaymentMethodType);
            IPIDLGenerator<ClientAction> generator;
            this.clientActionGenerators.TryGetValue(redirectPattern, out generator);
            return generator;
        }
    }
}

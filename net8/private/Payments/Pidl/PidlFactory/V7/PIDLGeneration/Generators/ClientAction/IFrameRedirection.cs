// <copyright file="IFrameRedirection.cs" company="Microsoft">Copyright (c) Microsoft 2023. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration.Generators
{
    using Microsoft.Commerce.Payments.PXCommon;

    internal class IFrameRedirection : IPIDLGenerator<ClientAction>
    {
        public ClientAction Generate(PIDLGeneratorContext context)
        {
            ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
            clientAction.Context = PIDLResourceFactory.GetCc3DSIframeRedirectAndStatusCheckDescriptionForPI(
                context.PaymentInstrument, 
                context.Language, 
                context.Partner, 
                context.Scenario, 
                context.ClassicProduct, 
                context.CompletePrerequisites, 
                context.Country, 
                context.PidlBaseUrl);

            return clientAction;
        }
    }
}

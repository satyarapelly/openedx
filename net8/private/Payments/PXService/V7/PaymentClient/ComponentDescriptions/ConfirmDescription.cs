// <copyright file="ConfirmDescription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.V7;

    public class ConfirmDescription : ComponentDescription
    {
        private string descriptionType = V7.Constants.DescriptionTypes.ConfirmDescription;

        public override string DescriptionType
        {
            get
            {
                return this.descriptionType;
            }
        }

        public override Task<List<PIDLResource>> GetDescription()
        {
            List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetPaymentClientDescriptions(this.descriptionType, this.Country, this.descriptionType, this.Operation, this.Language, this.Partner, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);

            if (this.ExposedFlightFeatures?.Contains(Flighting.Features.PXConfirmDfpIframe, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                DescriptionHelper.AddDFPIframe(retVal, this.UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions?.PaymentRequestId : this.CheckoutRequestClientActions?.CheckoutRequestId, this.ExposedFlightFeatures?.ToList());
            }

            retVal.ForEach(pidl =>
            {
                // Update payment method family, payment method type and Selected PIID property is optional.
                var selectedPIID = pidl.DataDescription[V7.Constants.PropertyDescriptionIds.SelectedPIID] as PropertyDescription;
                selectedPIID.IsOptional = true;

                var paymentMethodFamily = pidl.DataDescription[V7.Constants.PropertyDescriptionIds.PaymentMethodFamily] as PropertyDescription;
                paymentMethodFamily.IsOptional = true;

                var paymentMethodType = pidl.DataDescription[V7.Constants.PropertyDescriptionIds.PaymentMethodType] as PropertyDescription;
                paymentMethodType.IsOptional = true;

                // Update the submit URL with request id.
                UpdateSubmitURL(pidl, V7.Constants.ButtonDisplayHintIds.SaveButton, GlobalConstants.HTTPVerbs.POST, string.Format(V7.Constants.SubmitUrls.CheckoutRequestsExConfirm, this.RequestId, this.Partner));

                // Set isHidden property to true to hide the display description.
                pidl.DisplayPages.ForEach(page =>
                {
                    page.Members.ForEach(member =>
                    {
                        member.IsHidden = true;
                    });
                });
            });

            return Task.FromResult(retVal);
        }
    }
}
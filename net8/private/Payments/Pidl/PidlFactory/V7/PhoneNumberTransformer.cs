// <copyright file="PhoneNumberTransformer.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using libphonenumber;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// This class is responsible for transforming Phone numbers
    /// </summary>
    public sealed class PhoneNumberTransformer : PidlPropertyTransformer<string, string>
    {
        public PhoneNumberTransformer() :
            base()
        {
            this.InitializeStrategy();
        }

        protected override void InitializeStrategy()
        {
            this.Strategies[Constants.TransformationType.ToPhoneNumberE164] = (input, context) =>
            {
                var countryCode = context;
                PhoneNumber phoneNumber;

                try
                {
                    phoneNumber = PhoneNumberUtil.Instance.Parse(input.ToString(), countryCode.ToUpper());
                }
                catch (com.google.i18n.phonenumbers.NumberParseException ex)
                {
                    string errorMessage;

                    if (ex.getErrorType() == com.google.i18n.phonenumbers.NumberParseException.ErrorType.TOO_LONG)
                    {
                        errorMessage = PidlModelHelper.GetLocalizedString("Phone number is too long.");
                    }
                    else if (ex.getErrorType() == com.google.i18n.phonenumbers.NumberParseException.ErrorType.TOO_SHORT_AFTER_IDD ||
                        ex.getErrorType() == com.google.i18n.phonenumbers.NumberParseException.ErrorType.TOO_SHORT_NSN)
                    {
                        errorMessage = PidlModelHelper.GetLocalizedString("Phone number is too short.");
                    }
                    else
                    {
                        errorMessage = PidlModelHelper.GetLocalizedString("Invalid phone number.");
                    }

                    return new PidlTransformationResult<string>()
                    {
                        Status = PidlExecutionResultStatus.Failed,
                        ErrorCode = Constants.ErrorCodes.PIDLInvalidPhoneNumberForCountry,
                        ErrorMessage = errorMessage
                    };
                }

                return new PidlTransformationResult<string>()
                {
                    Status = PidlExecutionResultStatus.Passed,
                    TransformedValue = phoneNumber.Format(PhoneNumberUtil.PhoneNumberFormat.E164)
                };
            };

            this.Strategies[Constants.TransformationType.ToPhoneNumberE164VNext] = (input, context) =>
            {
                var countryCode = context;
                PhoneNumbers.PhoneNumber phoneNumber;

                try
                {
                    phoneNumber = PhoneNumbers.PhoneNumberUtil.GetInstance().Parse(input.ToString(), countryCode.ToUpper());
                }
                catch (PhoneNumbers.NumberParseException ex)
                {
                    string errorMessage;

                    if (ex.ErrorType == PhoneNumbers.ErrorType.TOO_LONG)
                    {
                        errorMessage = PidlModelHelper.GetLocalizedString(Constants.PhoneNumberErrorMessages.TooLong);
                    }
                    else if (ex.ErrorType == PhoneNumbers.ErrorType.TOO_SHORT_AFTER_IDD || ex.ErrorType == PhoneNumbers.ErrorType.TOO_SHORT_NSN)
                    {
                        errorMessage = PidlModelHelper.GetLocalizedString(Constants.PhoneNumberErrorMessages.TooShort);
                    }
                    else
                    {
                        errorMessage = PidlModelHelper.GetLocalizedString(Constants.PhoneNumberErrorMessages.Invalid);
                    }

                    return new PidlTransformationResult<string>()
                    {
                        Status = PidlExecutionResultStatus.Failed,
                        ErrorCode = Constants.ErrorCodes.PIDLInvalidPhoneNumberForCountry,
                        ErrorMessage = errorMessage
                    };
                }

                return new PidlTransformationResult<string>()
                {
                    Status = PidlExecutionResultStatus.Passed,
                    TransformedValue = PhoneNumbers.PhoneNumberUtil.GetInstance().Format(phoneNumber, PhoneNumbers.PhoneNumberFormat.E164)
                };
            };
        }

        protected override string GetName()
        {
            return Constants.TransformerNames.PhoneNumberTransformer;
        }
    }
}
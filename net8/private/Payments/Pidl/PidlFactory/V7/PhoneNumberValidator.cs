// <copyright file="PhoneNumberValidator.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using libphonenumber;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// This class is responsible for validating Phone numbers
    /// </summary>
    public sealed class PhoneNumberValidator : PidlPropertyValidator<string, string>
    {
        public PhoneNumberValidator() :
            base()
        {
            this.InitializeStrategy();
        }

        protected override void InitializeStrategy()
        {
            this.Strategies[Constants.ServerValidationType.PhoneNumber] = (input, context) =>
            {
                var countryCode = context;

                if (!PIDLResourceFactory.IsCountryValid(countryCode))
                {
                    return new PidlExecutionResult()
                    {
                        Status = PidlExecutionResultStatus.Failed,
                        ErrorCode = Constants.ErrorCodes.PIDLCountryCodeNotSupported,
                        ErrorMessage = string.Format("Country : {0} not supported", countryCode)
                    };
                }

                List<string> supportedRegions = PhoneNumberUtil.Instance.GetSupportedRegions().ToList();
                if (supportedRegions.Find(
                    (c) =>
                    {
                        return string.Equals(c, countryCode, StringComparison.InvariantCultureIgnoreCase);
                    }) == null)
                {
                    return new PidlExecutionResult()
                    {
                        Status = PidlExecutionResultStatus.Failed,
                        ErrorCode = Constants.ErrorCodes.PIDLCountryCodeNotSupported,
                        ErrorMessage = string.Format("Country : {0} not supported", countryCode)
                    };
                }

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
                    
                    return new PidlExecutionResult()
                    {
                        Status = PidlExecutionResultStatus.Failed,
                        ErrorCode = Constants.ErrorCodes.PIDLInvalidPhoneNumberForCountry,
                        ErrorMessage = errorMessage
                    };
                }

                if (!phoneNumber.IsValidNumberForRegion(countryCode.ToUpper()))
                {
                    return new PidlExecutionResult()
                    {
                        Status = PidlExecutionResultStatus.Failed,
                        ErrorCode = Constants.ErrorCodes.PIDLInvalidPhoneNumberForCountry,
                        ErrorMessage = PidlModelHelper.GetLocalizedString("Invalid phone number.")
                    };
                }

                return new PidlExecutionResult()
                {
                    Status = PidlExecutionResultStatus.Passed,
                };
            };

            this.Strategies[Constants.ServerValidationType.PhoneNumberVNext] = (input, context) =>
            {
                var countryCode = context;

                if (!PIDLResourceFactory.IsCountryValid(countryCode))
                {
                    return new PidlExecutionResult()
                    {
                        Status = PidlExecutionResultStatus.Failed,
                        ErrorCode = Constants.ErrorCodes.PIDLCountryCodeNotSupported,
                        ErrorMessage = string.Format("Country : {0} not supported", countryCode)
                    };
                }

                List<string> supportedRegions = PhoneNumbers.PhoneNumberUtil.GetInstance().GetSupportedRegions().ToList();
                if (supportedRegions.Find(
                    (c) =>
                    {
                        return string.Equals(c, countryCode, StringComparison.InvariantCultureIgnoreCase);
                    }) == null)
                {
                    return new PidlExecutionResult()
                    {
                        Status = PidlExecutionResultStatus.Failed,
                        ErrorCode = Constants.ErrorCodes.PIDLCountryCodeNotSupported,
                        ErrorMessage = string.Format("Country : {0} not supported", countryCode)
                    };
                }

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

                    return new PidlExecutionResult()
                    {
                        Status = PidlExecutionResultStatus.Failed,
                        ErrorCode = Constants.ErrorCodes.PIDLInvalidPhoneNumberForCountry,
                        ErrorMessage = errorMessage
                    };
                }

                if (!PhoneNumbers.PhoneNumberUtil.GetInstance().IsValidNumberForRegion(phoneNumber, countryCode.ToUpper()))
                {
                    return new PidlExecutionResult()
                    {
                        Status = PidlExecutionResultStatus.Failed,
                        ErrorCode = Constants.ErrorCodes.PIDLInvalidPhoneNumberForCountry,
                        ErrorMessage = PidlModelHelper.GetLocalizedString("Invalid phone number.")
                    };
                }

                return new PidlExecutionResult()
                {
                    Status = PidlExecutionResultStatus.Passed,
                };
            };
        }

        protected override string GetName()
        {
            return Constants.ValidatorNames.PhoneNumberValidator;
        }
    }
}
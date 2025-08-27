// <copyright file="ErrorDetailsFilter.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PXCommon;

    public class ErrorDetailsFilter : ConfigurationObject
    {
        public ErrorDetailsFilter(List<string[]> rows, ParsedConfigurationComponent parsedComponent, Dictionary<string, int> columnNames)
        {
            this.ErrorCodeDetails = new Dictionary<string, ErrorDetails>();

            foreach (ParsedConfigurationComponent serviceError in parsedComponent["ServiceError"].Take(parsedComponent["ServiceError"].Count - 1))
            {
                ErrorDetails errorDetails = ErrorDetails.ConstructFromConfiguration<ErrorDetails>(rows, serviceError, columnNames);

                for (int index = serviceError.Range.Item1; index <= serviceError.Range.Item2; index++)
                {
                    string errorCodeValue = ResourceLifecycleStateManager.ParseConstant(rows[index][(int)ResourceLifecycleStateManager.ErrorConfigColumn.ErrorCode]);

                    if (!string.IsNullOrEmpty(errorCodeValue))
                    {
                        this.ErrorCodeDetails[errorCodeValue] = errorDetails;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            this.GenericErrorCodeDetails = ErrorDetails.ConstructFromConfiguration<ErrorDetails>(rows, parsedComponent["ServiceError"].Last(), columnNames);
        }

        public Dictionary<string, ErrorDetails> ErrorCodeDetails { get; private set; }

        public ErrorDetails GenericErrorCodeDetails { get; private set; }

        public async Task UpdateErrorAsync(ResourceLifecycleStateManager.ErrorResourceState state)
        {
            ErrorDetails error = this.GenericErrorCodeDetails;

            if (this.ErrorCodeDetails.ContainsKey(state.ResponseException.Error.ErrorCode))
            {
                error = this.ErrorCodeDetails[state.ResponseException.Error.ErrorCode];
            }

            state.ResponseException.Error.Message = (error.Message == V7.Constants.ClientActionContract.NoMessage) ? error.Message : LocalizationRepository.Instance.GetLocalizedString(error.Message, state.Language);

            if (!string.IsNullOrEmpty(error.DetailsErrorCode) || !string.IsNullOrEmpty(error.DetailsMessage) || !string.IsNullOrEmpty(error.DetailsTarget))
            {
                state.ResponseException.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = string.IsNullOrEmpty(error.DetailsErrorCode) ? state.ResponseException.Error.ErrorCode : error.DetailsErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(error.DetailsMessage, state.Language),
                    Target = error.DetailsTarget,
                });
            }

            PXCommon.ClientAction clientAction = null;
            PimsModel.V4.PaymentInstrument reacquiredPi = null;

            switch (error.ClientAction)
            {
                case ResourceLifecycleStateManager.ErrorClientAction.Jumpback:
                    HashSet<PimsModel.V4.PaymentMethod> paymentMethodHashSet = new HashSet<PimsModel.V4.PaymentMethod>();
                    paymentMethodHashSet.Add(state.PI.PaymentMethod);
                    clientAction = new PXCommon.ClientAction(ClientActionType.Pidl);
                    clientAction.Context = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethodHashSet, "cn", state.PaymentMethodFamily, state.PaymentMethodType, "add", state.Language, state.Partner, null, state.ClassicProduct, state.BillableAccountId, null, state.CompletePrerequisites);
                    state.ResponseException.Error.ClientAction = clientAction;
                    break;
                case ResourceLifecycleStateManager.ErrorClientAction.DirectDebitAch:
                    reacquiredPi = await ResourceLifecycleStateManager.ServiceSettings.PIMSAccessor.GetPaymentInstrument(state.AccountId, state.Piid, state.TraceActivityId);
                    clientAction = new PXCommon.ClientAction(ClientActionType.Pidl);
                    clientAction.Context = PIDLResourceFactory.Instance.GetPicvChallengeDescriptionForPI(reacquiredPi, V7.Constants.PidlResourceDescriptionType.AchPicVChallenge, state.Language, state.Partner, state.ClassicProduct, state.BillableAccountId);
                    state.ResponseException.Error.ClientAction = clientAction;
                    break;
                case ResourceLifecycleStateManager.ErrorClientAction.DirectDebitSepa:
                    reacquiredPi = await ResourceLifecycleStateManager.ServiceSettings.PIMSAccessor.GetPaymentInstrument(state.AccountId, state.Piid, state.TraceActivityId);
                    clientAction = new PXCommon.ClientAction(ClientActionType.Pidl);
                    clientAction.Context = PIDLResourceFactory.Instance.GetPicvChallengeDescriptionForPI(reacquiredPi, V7.Constants.PidlResourceDescriptionType.SepaPicVChallenge, state.Language, state.Partner, state.ClassicProduct, state.BillableAccountId);
                    state.ResponseException.Error.ClientAction = clientAction;
                    break;
                default:
                    break;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ErrorCodes[");

            if (this.ErrorCodeDetails.Count > 0)
            {
                sb.Append(this.ErrorCodeDetails.Keys.Aggregate((a, n) => a + "," + n));

                if (!string.IsNullOrEmpty(this.GenericErrorCodeDetails.Message))
                {
                    sb.Append(",Else");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.GenericErrorCodeDetails.Message))
                {
                    sb.Append("Else");
                }
            }

            sb.Append(']');

            return sb.ToString();
        }
    }
}
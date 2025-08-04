// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    internal static class Constants
    {
        internal static class TestScenarioManagers
        {
            public const string Account = "Account";
            public const string PartnerSettings = "PartnerSettings";
            public const string PIMS = "PIMS";
            public const string MSRewards = "MSRewards";
            public const string Catalog = "Catalog";
            public const string IssuerService = "IssuerService";
            public const string ChallengeManagement = "ChallengeManagement";
            public const string PaymentThirdParty = "PaymentThirdParty";
            public const string Purchase = "Purchase";
            public const string Risk = "Risk";
            public const string SellerMarketPlace = "SellerMarketPlace";
            public const string TokenPolicy = "TokenPolicy";
            public const string TransactionService = "TransactionService";
            public const string PaymentOchestrator = "PaymentOchestrator";
            public const string FraudDetection = "FraudDetection";
        }
        
        internal static class PartnerSettingsApiName
        {
            public const string GetPartnerSettings = "getPartnerSettings";
        }

        internal static class Placeholders
        {
            public const string AccountId = "<AccountId>";
            public const string AddressId = "<AddressId>";
            public const string ProfileId = "<ProfileId>";
            public const string ShipToAddressId = "<shipToAddressId>";
            public const string Id = "<id>";
            public const string PaymentRequestId = "<paymentRequestId>";
        }

        internal static class PIMSApiName
        {
            public const string GetPI = "getPI";
            public const string GetPIExtendedView = "getPIExtendedView";
            public const string GetSeCardPersos = "getSeCardPersos";
            public const string AddPI = "addPI";
            public const string AddPIWithoutJarvisAccount = "AddPIWithoutJarvisAccount";
            public const string ResumeAddPI = "resumeAddPI";
            public const string GetPM = "getPM";
            public const string GetEligiblePaymentMethods = "getEligiblePaymentMethods";
            public const string ListPI = "listPI";
            public const string ListEmpOrgPI = "listEmpOrgPI";
            public const string SearchByAccountNumber = "searchByAccountNumber";
            public const string UpdatePI = "updatePI";
            public const string ReplacePI = "replacePI";
            public const string ValidateCvv = "validateCvv";
            public const string Validate = "validate";
            public const string LinkTransaction = "linkTransaction";
            public const string GetSessionDetails = "getSessionDetails";
            public const string GetChallengeContext = "getChallengeContext";
        }

        internal static class MSRewardsApiName
        {
            public const string GetUserInfo = "getUserInfo";
            public const string GetUserInfoUserIdEmpty = "getUserInfoUserIdEmpty";
            public const string RedeemRewards = "redeemRewards";
            public const string RedeemRewardsUserIdEmpty = "redeemRewardsUserIdEmpty";
        }

        internal static class CatalogApiName
        {
            public const string GetProducts = "getProducts";
        }

        internal static class AccountApiName
        {
            public const string GetProfiles = "getProfiles";
            public const string PutProfile = "putProfile";
            public const string GetCustomers = "getCustomers";
            public const string GetAddresses = "getAddresses";
            public const string GetAddress = "getAddress";
            public const string PostAddressValidate = "postAddressValidate";
            public const string PostAddressValidateAVS = "postAddressValidateAVS";
            public const string PostAddress = "postAddress";
            public const string LegacyValidateAddress = "legacyValidateAddress";
            public const string PatchAddress = "patchAddress";
            public const string PatchProfile = "patchProfile";
        }

        internal static class IssuerServiceApiName
        {
            public const string Apply = "apply";
            public const string Eligibility = "applyEligibility";
            public const string Initialize = "initialize";
            public const string ApplicationDetails = "applicationDetails";
        }

        internal static class ChallengeManagementApiName
        {
            public const string CreateChallenge = "createChallenge";
            public const string GetChallengeStatus = "getChallengeStatus";
            public const string CreateChallengeSession = "createChallengeSession";
            public const string GetChallengeSessionData = "getChallengeSessionData";
            public const string UpdateChallengeSession = "updateChallengeSession";
        }

        internal static class PaymentThirdPartyApiName
        {
            public const string GetPaymentRequest = "getPaymentRequest";
            public const string GetCheckout = "getCheckout";
            public const string Charge = "charge";
            public const string Status = "status";
            public const string Completed = "completed";
        }

        internal static class PurchaseApiName
        {
            public const string GetOrder = "getOrder";
            public const string CheckPi = "checkPi";
            public const string ListOrder = "listOrder";
            public const string ListSub = "listSub";
            public const string GetSub = "getSub";
            public const string RedeemCSVToken = "redeemCSVToken";
        }

        internal static class RiskApiName
        {
            public const string RiskEvaluation = "riskevaluation";
        }

        internal static class SellerMarketPlaceApiName
        {
            public const string GetSeller = "getSeller";
        }

        internal static class TokenPolicyApiName
        {
            public const string GetTokenPolicyDescription = "getTokenPolicyDescription";
        }

        internal static class StoredValueApiName
        {
            public const string GetGiftCatalog = "getGiftCatalog";
            public const string PostFunding = "postFunding";
            public const string GetFundingStatus = "getFundingStatus";
        }

        internal static class WalletApiName
        {
            public const string GetProviderData = "getProviderData";
        }

        internal static class TransactionServiceApiName
        {
            public const string Payments = "payments";
            public const string TransactionValidate = "transactionValidate";
        }

        internal static class PaymentOchestratorApiName
        {
            public const string AttachAddress = "attachaddress";
            public const string PRAttachAddress = "prAttachaddress";
            public const string AttachProfile = "attachprofile";
            public const string PRAttachProfile = "prAttachProfile";
            public const string AttachPaymentInstruments = "attachpaymentinstruments";
            public const string PRAttachPaymentInstruments = "prAttachpaymentinstruments";
            public const string Confirm = "confirm";
            public const string PRConfirm = "prConfirm";
            public const string GetClientAction = "clientaction";
            public const string GetClientActions = "getClientActions";
        }

        internal static class PayerAuthApiName
        {
            public const string CreatePaymentSessionId = "createPaymentSessionId";
            public const string Get3DSMethodUrl = "get3DSMethodUrl";
            public const string Authenticate = "authenticate";
            public const string Result = "result";
            public const string CompleteChallenge = "completeChallenge";
        }
        
        internal static class FraudDetectionApiName
        {
            public const string BotCheck = "botCheck";
        }

        internal static class DefaultTestScenarios
        {
            public const string AccountEmulator = "px.account.v2.us.full.profile.default.address";
            public const string IssuerServiceEmulator = "px.issuerservice.default";
            public const string ChallengeManagementServiceEmulator = "px.challengemanagementservice.default";
            public const string FraudDetectionEmulator = "px.frauddetection.approved";
            public const string CatalogEmulator = "px.listtrx.catalogservice.success";
            public const string MSRewardsEmulator = "px.msrewards.success";
            public const string PartnerSettingsEmulator = "px.partnersettings.success";
            public const string POEmulator = "px.po.attachAddress";
            public const string PaymentThirdPartyEmulatorCheckout = "px.3pp.stripe.guestcheckout.success";
            public const string PaymentThirdPartyEmulatorPayment = "px.stripe.guestcheckout.success";
            public const string RiskEmulator = "px.risk.approved.success";
            public const string SellerMarketEmulator = "px.sellermarket.stripe.us";
            public const string TokenPolicyEmulator = "px.tops.redeem.giftcard.success";
        }
        
        internal static class TestScenarios
        {
            public const string PX = "px";
            public const string PXAccount = "px.account";
            public const string PXPims = "px.pims";
            public const string PXIssuerService = "px.issuerservice";
            public const string PXPartnerSettings = "px.partnersettings";
            public const string PXRiskRejectedSuccess = "px.risk.rejected.success";
            public const string PXRiskBadRequestFailed = "px.risk.badrequest.failed";
            public const string PXRiskServerErrorFailed = "px.risk.servererror.failed";
            public const string PXMsRewardsEditPhone = "px.msrewards.editphone";
            public const string PXMsRewardsChallenge = "px.msrewards.challenge";
            public const string PXPurchasefdListtrxSuccess = "px.purchasefd.listtrx.success";
            public const string PXPurchasefdRedeemcsvSuccess = "px.purchasefd.redeemcsv.success";
            public const string PXPurchasefdConfirmPaymentPolling = "px.purchasefd.confirmpayment.polling";
            public const string PXPayerAuthPSD2ChallengeSkipfp = "px.payerauth.psd2.challenge.skipfp";
            public const string PXPayerAuthPSD2ChallengeSkipcreq = "px.payerauth.psd2.challenge.skipcreq";
            public const string PXPayerAuthPSD2ChallengeCancelled = "px.payerauth.psd2.challenge.cancelled";
            public const string PXPayerAuthPSD2ChallengeTimedout = "px.payerauth.psd2.challenge.timedout";
            public const string PXPayerAuthPSD2ChallengeFailed = "px.payerauth.psd2.challenge.failed";
            public const string PXPayerAuthPSD2ChallengeSuccess = "px.payerauth.psd2.challenge.success";
            public const string PXPIMS3DSChallengeTimedout = "px.pims.3ds.challenge.timedout";
            public const string PXPayerAuth3DS1ChallengeSuccess = "pxpayerauthemulator.3ds1.challenge.success";
            public const string PXPayerAuthXboxRedemRewardsSuccess = "px.payerauth.xboxredeemrewards.success";
        }
    }
}
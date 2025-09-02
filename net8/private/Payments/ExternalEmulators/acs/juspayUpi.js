const GetCallbackUrl = (req) => {
    var sessionId = req.params['sessionid'];
    var environment = req.params['environment'];
    var mrn = req.params['mrn'];

    var callbackQueryParams = `status=CHARGED&order_id=${mrn}&signature=dummysignature`

    if (environment.toLowerCase() == "prod"){
        return `https://paymentsredirectionservice.cp.microsoft.com/redirectionservice/CoreRedirection/Callback/${sessionId}?${callbackQueryParams}`;
    }
    else
    {
        return `https://paymentsredirectionservice.cp.microsoft-int.com/redirectionservice/CoreRedirection/Callback/${sessionId}?${callbackQueryParams}`;
    }
}

module.exports = { GetCallbackUrl };
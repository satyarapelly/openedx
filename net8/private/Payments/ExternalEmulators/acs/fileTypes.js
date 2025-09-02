
module.exports = {

    resolveFileType:  function (piFlowsMap, piExpiry, challengeDataEntry, challengeHTMLDataEntry, oobContinue, areqTs)
    {
        var d = new Date();
        challengeDataEntry = (challengeDataEntry || '').toLowerCase();
        challengeHTMLDataEntry = (challengeHTMLDataEntry || '').toLowerCase();
        var challengeOobContinue = (oobContinue || '').toString().toLowerCase();
        var exceedsThreshold = ((d.getTime() - d.getMilliseconds())/1000 - areqTs) > 10  ? 'true' : 'false';

        var key1 = [piExpiry, challengeDataEntry, challengeHTMLDataEntry, challengeOobContinue, exceedsThreshold].join('_');
        var key2 = [piExpiry, challengeDataEntry, challengeHTMLDataEntry, challengeOobContinue, '*'].join('_');
        var key3 = [piExpiry, '*', challengeHTMLDataEntry, challengeOobContinue, '*'].join('_');
        var key4 = [piExpiry, challengeDataEntry, '*', challengeOobContinue, '*'].join('_');
        var fileType = piFlowsMap[key1] || piFlowsMap[key2] || piFlowsMap[key3] || piFlowsMap[key4];

        console.log("areq._ts: " + areqTs);
        console.log("currentTime: " + d.getTime() + " , " + d.getMilliseconds());
        console.log("key1: " + key1);
        console.log("fileType: " + fileType);
        console.log("piFlowsMap: " + piFlowsMap);

        if(!fileType)
        {
            fileType = 'single_select';
            if (challengeDataEntry == 'email' || challengeDataEntry == 'sms')
            fileType = 'otp';
            else if (challengeDataEntry == 'multi')
            fileType = 'multi_select';
            else if (challengeDataEntry == 'oob')
            fileType = 'oob';
            else if (challengeDataEntry == "html")
            fileType = "html";
            else if (challengeDataEntry && (challengeDataEntry.startsWith('final') || challengeDataEntry == '456'))
            fileType = 'final';
        }
        return fileType;
    },

    formatAccount: function(amount, currency) {
        var currencySymPound = "£";
        var currencySymEuro = "€";
        var currencySymDollar = "$";

        var currencySym = currencySymPound;
        var numOfDecimal = 2;
        if (currency == "978") {
          numOfDecimal = 2;
          currencySym = currencySymEuro;
        } 
        else if ( currency == "826") {
            numOfDecimal = 2;
            currencySym = currencySymPound;
        }
        else if ( currency == "USD") {
            numOfDecimal = 2;
            currencySym = currencySymDollar;
        }
        return currencySym + (parseInt(amount) / 100).toFixed(numOfDecimal);        
    }
};    
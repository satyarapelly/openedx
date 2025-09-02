const express = require('express')
const bodyParser = require('body-parser');
const url = require('url');
const appInsights = require("applicationinsights");
const CosmosClient = require("@azure/cosmos").CosmosClient;
const { DefaultAzureCredential } = require ('@azure/identity');
const dbconfig = require('./dbconfig');
const TransactionDao = require('./transactionDao');
const readlineModule = require('readline');
const fs = require('fs');
const base64Url = require('base64url');
const util = require('util');
const cryptoUtil = require('./cryptoUtil');
const akvClient = require("./akvClient");
const fileTypes = require('./fileTypes');
const uuid = require('uuid/v4');
const escapeHtml = require('escape-html');
const router = express.Router()

const credentials = new DefaultAzureCredential({ managedIdentityClientId: "d417dccc-67c5-4887-9e15-3b3a97732261" }); // CodeQL [SM05138] Not used in production, only for testing purposes
const dbClient = new CosmosClient({ endpoint: dbconfig.host, aadCredentials: credentials });
const dao = new TransactionDao(dbClient, dbconfig.databaseId, dbconfig.collectionId);
console.log("db host: " + dbconfig.host);

const METHOD_RESPONSE_TEMPLATE = '<html>' +
'<body onload=\'document.forms[0].submit();\'>' + 
'<form action="%s" method="POST">' + 
'<input type="hidden" name="x-ms-test" value="%s"/>' +
'<input type="hidden" name="%s" value="%s"/></form></body></html>';
const CRES_RESPONSE_TEMPLATE = '<html>' +
'<body onload=\'document.forms[0].submit();\'>' + 
'<form action="%s" method="POST">' + 
'<input type="hidden" name="x-ms-test" value="%s"/>' +
'<input type="hidden" name="%s" value="%s"/><input type="hidden" name="%s" value="%s"/></form></body></html>';
const CHALLENGE_HTML = '<html><body><form action="challenge" method="POST"><h3>ACS Bank Emulator</h3>' +
'<h5>Identity Check</h5>' +
'<input type="hidden" name="x-ms-test" value="%s"/>' +
'<input type="hidden" name="%s" value="%s"/>' +
'<label style="color: red">%s</label><br/>' +
'<label>We have sent you a OTP Code, please enter the Code in the textbox</label><br/>' +
'<input type="text" name="otpCode" value=""/><br/>&nbsp;<br/>' +
'<input type="submit" name="action" value="Submit"/>&nbsp;' +
'<input type="submit" name="action" value="Cancel"/>' +
'</form></body></html>';

// Based 64 encoded {"scenarios":"px-service-psd2-e2e-emulator","contact":"mdollarpurchase"}
const DEFAULT_TEST_HEADER = "eyJzY2VuYXJpb3MiOiJweC1zZXJ2aWNlLXBzZDItZTJlLWVtdWxhdG9yIiwiY29udGFjdCI6Im1kb2xsYXJwdXJjaGFzZSJ9";
const DEFAULT_MESSAGE_VERSION = "2.2.0";

// configure body-parser for express
router.use(bodyParser.urlencoded({extended:false}));
router.use(bodyParser.json());
router.use(bodyParser.text(({ inflate: true, limit: '200kb', type: 'application/jose'})));

// middleware that is specific to this router
router.use(function timeLog (req, res, next) {
  console.log('Time: ', Date.now());
  next();
});

// Read piflows configuration
const piFlowsReader = readlineModule.createInterface({ input: fs.createReadStream('piflows.csv')});
const piFlowsMap = {};
piFlowsReader.on('line', function(line){
  if(line){
    const index = line.lastIndexOf(',');
    const key = line.substr(0, index).replace(/,/g, '_').toLowerCase();
    const value = line.substr(index+1).toLowerCase();
    piFlowsMap[key] = value;
  }
});

//initialize the keys
let signaturePrivKey = "";
let signaturePubKey = "";
const akv = new akvClient();

akv.decryptWithAKV("acs_signature_data.dat").then((key) => {
  signaturePrivKey = key;
});
akv.decryptWithAKV("acs_signature_public_key.dat").then((key) => {
  signaturePubKey = key.toString("utf-8");
});

router.post('/supportedversions', function(req, res) {
  appInsights.defaultClient.trackNodeHttpRequest({request: req, response: res});
  console.log('Body: ', req.body);

  const resp = {
    threeDSServerTransID: uuid(),
    acsStartProtocolVersion: DEFAULT_MESSAGE_VERSION,
    acsEndProtocolVersion:DEFAULT_MESSAGE_VERSION,
    threeDSMethodURL: "https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/acs/fingerprint",
    dsStartProtocolVersion:DEFAULT_MESSAGE_VERSION,
    dsEndProtocolVersion: DEFAULT_MESSAGE_VERSION
  }

  res.status(200);
  res.send(resp); 
});

router.post('/fingerprint', function(req, res) {
  appInsights.defaultClient.trackNodeHttpRequest({request: req, response: res});
  console.log('Data: ', req.body.threeDSMethodData);
  console.log('Body: ', req.body);
  
  if (req.body.threeDSMethodData != undefined)
  {
    const methodData = JSON.parse(base64Url.decode(req.body.threeDSMethodData));
    console.log('Method Data: ' + methodData.threeDSServerTransID);

    const encodedMsTestHeader = 'x-ms-test' in req.body ? escapeHtml(req.body['x-ms-test']) : DEFAULT_TEST_HEADER;
    const msTestHeader = base64Url.decode(encodedMsTestHeader);
    console.log('x-ms-test: ' + msTestHeader);
    const resp = util.format(METHOD_RESPONSE_TEMPLATE,
      escapeHtml(methodData.threeDSMethodNotificationURL),
      encodedMsTestHeader,
      'threeDSMethodData',
      escapeHtml(req.body.threeDSMethodData)
    );
    res.set('Content-Type', 'text/html; charset=utf-8');
    res.send(resp);
  }
  else
  {
    appInsights.defaultClient.trackRequest({name:"POST /fingerprint", url: req.path, duration:0, resultCode:400, success:true});
    res.status(400);
    res.send("Bad Request");
  }
});


// define the home page route
router.post('/creq', function (req, res) {
    appInsights.defaultClient.trackNodeHttpRequest({request: req, response: res});
    console.log('threeDSSessionData: ', req.body.threeDSSessionData);
    console.log('creq: ', req.body.creq);
    
    const referer = req.headers.referer;
    console.log('Referer: ' + referer);
    
    if (req.body.threeDSSessionData != undefined)
    {
      try {
        const sessionData = JSON.parse(base64Url.decode(req.body.threeDSSessionData));
        console.log('Method Data: ' + sessionData.threeDSServerTransID);
        console.log('sessionData: ' + JSON.stringify(sessionData));

        // Validate sessionData structure
        if (!sessionData.acsTransID || typeof sessionData.acsTransID !== 'string') {
          throw new Error('Invalid session data structure');
        }

        const scrubbedData = JSON.stringify(sessionData);

        const encodedMsTestHeader = 'x-ms-test' in req.body ? escapeHtml(req.body['x-ms-test']) : DEFAULT_TEST_HEADER;
        const msTestHeader = base64Url.decode(encodedMsTestHeader);
        console.log('x-ms-test: ' + msTestHeader);

        dao.getItem(sessionData.acsTransID, function(err, areq)
        {
          console.log("AReq dn status: " + areq != undefined);

          if (areq != undefined)
          {
            let fileType = fileTypes.resolveFileType(piFlowsMap, areq.cardExpiryDate, '', '', '', areq._ts);
            fileType =  fileType.startsWith("html") ? "single_select" : fileType;

            let respHtml = fs.readFileSync('cres_challenge_' + fileType + '.html', 'utf8');

            // Safely encode all dynamic content before inserting into HTML
            const safeSessionData = escapeHtml(Buffer.from(scrubbedData).toString("base64"));
            const safeAmount = escapeHtml(fileTypes.formatAccount(areq.purchaseAmount, areq.purchaseCurrency));

            respHtml = respHtml.replace(/@@threeDSSessionData@@/g, safeSessionData);
            respHtml = respHtml.replace(/@@mstestheader@@/g, encodedMsTestHeader);
            respHtml = respHtml.replace(/@@amount@@/g, safeAmount);

            res.set('Content-Type', 'text/html; charset=utf-8');
            res.send(respHtml);
          }
          else  {
            appInsights.defaultClient.trackRequest({name:"POST /creq", url: req.path, duration:0, resultCode:400, success:true});
            res.status(400);
            res.send("Bad Request");
          }
        });
      } catch (e) {
        console.error('Error processing request:', e);
        appInsights.defaultClient.trackException({exception: e});
        res.status(400);
        res.send("Bad Request: Invalid input");
      }
    }
    else
    {
      appInsights.defaultClient.trackRequest({name:"POST /creq", url: req.path, duration:0, resultCode:400, success:true});
      res.status(400);
      res.send("Bad Request");
    }
});

// define the about route
router.post('/challenge', function (req, res) {
  appInsights.defaultClient.trackNodeHttpRequest({request: req, response: res});
  console.log('Data: ', req.body.threeDSSessionData);
  console.log('Body: ', req.body);
  console.log('id: ' + req.params.id);  
  
  if (req.body.threeDSSessionData != undefined)
  {
    const creq = JSON.parse(Buffer.from(req.body.threeDSSessionData, 'base64'));
    console.log('Method Data: ' + creq.threeDSServerTransID);
    console.log('CReq: ' + JSON.stringify(creq));

    const cres = { threeDSServerTransID: creq.threeDSServerTransID, acsTransID: creq.acsTransID, messageType: "CRes", challengeCompletionInd: "Y" };
    const cresJson = JSON.stringify(cres);
    const cresEncoded = Buffer.from(cresJson).toString("base64");

    dao.getItem(creq.acsTransID, function(err, areq) {

      if (areq != undefined)
      {
        areq.acsCounterAtoS = areq.acsCounterAtoS + 1;
        let dataEntry = req.body.challengeDataEntry;
        let timedOut = false;
        let oob = false;
        if (req.body.continue == 'continue') {
          oob = true;
          dataEntry = '';
        } 
        if (req.body.timedout == "true") {
          timedOut = true;
          dataEntry = '456';
        }
        if (req.body.resend == "Resend Code") {
          dataEntry = 'resend';
        }

        // There is only one value is possible, so assign empty string to it if dataEntry is an array
        if (Array.isArray(dataEntry) && dataEntry.length > 1) {
           dataEntry = '';
        }

        let fileType = fileTypes.resolveFileType(piFlowsMap, areq.cardExpiryDate, dataEntry, '', oob, areq._ts);
        fileType =  fileType.startsWith("html") ? "single_select" : fileType;
        fileType = dataEntry == 'resend' ? "otp" : fileType;

        if (fileType == "otp" || fileType == "multi_select" || dataEntry == 'resend') {
          areq.otpTryCount = areq.otpTryCount + 1;
        }

        if (req.body.cancel == "Cancel") {
          areq.challengeCancel = '01';
          fileType = 'failed';
          areq.transStatus = 'N';
          areq.transStatusReason = '01';
          areq.challengeCancel = '01';          
        } 
        else if (fileType == 'final') {
          areq.transStatus = 'Y';

          //the page timed out
          if (timedOut) {
            areq.transStatus = 'R';
            areq.transStatusReason = '14';            
          }
        } 
        else if (dataEntry != 'resend' && areq.otpTryCount > 3) {
          //there has been too many incorrect attempts by the user 
          fileType = 'failed';
          areq.transStatus = 'R';
          areq.transStatusReason = '01';
        }

        let respHtml = fs.readFileSync('cres_challenge_' + fileType + '.html', 'utf8');

        const encodedMsTestHeader = 'x-ms-test' in req.body ? req.body['x-ms-test'] : DEFAULT_TEST_HEADER;

        respHtml = respHtml.replace(/@@actionUrl@@/g, areq.notificationURL);
        respHtml = respHtml.replace(/@@threeDSSessionData@@/g, req.body.threeDSSessionData);
        respHtml = respHtml.replace(/@@mstestheader@@/g, encodedMsTestHeader);
        respHtml = respHtml.replace(/@@cres@@/g, cresEncoded);
        respHtml = respHtml.replace(/@@amount@@/g, fileTypes.formatAccount(areq.purchaseAmount, areq.purchaseCurrency));

        res.status(200);
        res.set('Content-Type', 'text/html; charset=utf-8');
        res.send(respHtml); // CodeQL [SM01523] Not used in production, only for testing purposes;

        dao.updateItem(areq, function (err) {} );    

      } else 
      {
        appInsights.defaultClient.trackEvent("HTTP 400: Looking up threeDSServerTransID failed: " + creq.threeDSServerTransID);
        res.status(400);
        res.send("Bad Request: HTTP 400: Looking up threeDSServerTransID failed");
      }
    });
  }
  else
  {
    appInsights.defaultClient.trackRequest({name:"POST /challenge", url: req.path, duration:0, resultCode:400, success:true});
    res.status(400);
    res.send("Bad Request");
  }  
});

router.post('/sdk/challenge', function (req, res) {
  appInsights.defaultClient.trackNodeHttpRequest({request: req, response: res});
  const jose = req.body;
  console.log('Body: ', jose);

  appInsights.defaultClient.trackTrace({request: req});
  if (req.headers["content-type"] != undefined &&
      req.headers["content-type"].startsWith("application/jose") &&
      (typeof jose === 'string' && !jose.includes("errorCode")))
  {
    appInsights.defaultClient.trackTrace({ message: req.body });

    const joseTokens = jose.split(".");
    const header = JSON.parse(base64Url.decode(joseTokens[0]));
    const iv = base64Url.toBuffer(joseTokens[2]);
    const encContent = base64Url.toBuffer(joseTokens[3]);

    const acsTransId = header.kid;

    console.log('Header: ' + JSON.stringify(header));
    console.log('Acs TransId: ' + acsTransId);

    let sharedKey, acsCounterAtoS, sdkTransID;
    dao.getItem(acsTransId, function(err, areq) 
    {
      console.log("AReq dn status: " + areq != undefined);
      if (areq != undefined)
      {
        sharedKey = base64Url.toBuffer(areq.sharedKey);
        acsCounterAtoS = areq.acsCounterAtoS;
        sdkTransID = areq.sdkTransID;
        areq.acsCounterAtoS = acsCounterAtoS + 1;

        const creqData = cryptoUtil.decryptContent(sharedKey.slice(16, 32), iv, encContent);
        const creq = JSON.parse(creqData);

        console.log("Creq : " + creq);

        let fileType = 'single_select';
        if(creq.challengeCancel || '' == '01') {
          console.log("Challenge cancelled");
          fileType = 'cancelled';
          areq.transStatus = 'N';
          areq.transStatusReason = '01';
          areq.challengeCancel = '01';
        }
        else {
          fileType = fileTypes.resolveFileType(piFlowsMap, areq.cardExpiryDate, creq.challengeDataEntry, creq.challengeHTMLDataEntry, creq.oobContinue, areq._ts);

          if (fileType == "otp" || fileType == "multi_select") {
            areq.otpTryCount = areq.otpTryCount + 1;
          }

          if (fileType == 'final') {
            areq.transStatus = 'Y';
          } 
          else if (areq.otpTryCount > 3) {
            //there has been too many incorrect attempts by the user 
            fileType = 'failed';
            areq.transStatus = 'R';
            areq.transStatusReason = '10';
          }
        }

        const resultFileName = 'cres_challenge_' + fileType + '.json';
        let cres = JSON.parse(fs.readFileSync(resultFileName, 'utf8'));
        cres.threeDSServerTransID = creq.threeDSServerTransID;
        cres.acsTransID = creq.acsTransID;
        let counter = "0000" + acsCounterAtoS;
        cres.acsCounterAtoS =  counter.slice(-3);
        cres.sdkTransID = creq.sdkTransID;

        const amount = escapeHtml(fileTypes.formatAccount(areq.purchaseAmount, areq.purchaseCurrency));
        cres.challengeInfoText = util.format(cres.challengeInfoText, amount);

        if (fileType == 'otp' && creq.challengeDataEntry && creq.challengeDataEntry != '456')
        {
          cres.challengeInfoText = escapeHtml(cres.challengeInfoText + '\n You entered the wrong code, please try again or press Resend Code.');
          cres.challengeInfoTextIndicator = 'Y';
        } else if (fileType == "multi_select" && areq.otpTryCount > 0) {
          cres.challengeInfoText += '\n You have selected an incorrect option, please try again.';
          cres.challengeInfoTextIndicator = 'Y';
        }

        cres.messageVersion = creq.messageVersion || DEFAULT_MESSAGE_VERSION;

        console.log(cres);

        const content = JSON.stringify(cres);
        const responseData = cryptoUtil.createJose(sharedKey, content, acsTransId);
          
        res.status(200);
        res.send(responseData);

        dao.updateItem(areq, function (err) {} );    
        return;
      }
    });        
  }
  else if (typeof jose === 'string' && jose.includes("errorCode")) {
    const error = req.body;
    dao.getItem(error.acsTransID, function(err, areq) 
    {
      console.log("AReq dn status: " + areq != undefined);
      if (areq != undefined)
      {    
        areq.transStatus = 'N';
        areq.transStatusReason = '24';

        dao.updateItem(areq, function (err) {} );    

        res.status(200);
        res.send("ok");        
        return;        
      }
    });
  } else
  {
    res.status(400);
    res.send("Bad Request");
  }
});

router.post('/auth', function (req, res) {
  appInsights.defaultClient.trackNodeHttpRequest({request: req, response: res});
  console.log('Body: ', req.body);

  appInsights.defaultClient.trackTrace({request: req});
  if (req.headers["content-type"] != undefined && 
      req.headers["content-type"].startsWith("application/json"))
  {
    let areq = req.body;

    areq.threeDSServerTransID = areq.threeDSServerTransID || areq.sdkTransID;

    console.log('Body: ', areq.threeDSServerTransID);

    const deviceChannel = areq.deviceChannel == '01' ? 'app' : 'brow';
    let ares = JSON.parse(fs.readFileSync('auth-response-' + deviceChannel + '.json', 'utf8'));

    areq.id = areq.threeDSServerTransID;
    areq.acsTransID = areq.threeDSServerTransID;
    areq.acsCounterAtoS = 0;
    areq.transStatus = 'N';

    ares.acsTransID = areq.threeDSServerTransID;   
    ares.threeDSServerTransID = areq.threeDSServerTransID;
    ares.dsTransID = uuid();
    ares.messageVersion = areq.messageVersion || DEFAULT_MESSAGE_VERSION;

    if (areq.deviceChannel == '01')
    {
      const sdkPubKey = cryptoUtil.deserializePublicKey(areq.sdkEphemPubKey);
      const keyInfo = cryptoUtil.createSharedSecret(sdkPubKey, areq.sdkReferenceNumber);

      areq.sharedKey = base64Url.encode(keyInfo.sharedKey);

      let keyPayload = {};
      keyPayload.acsEphemPubKey = cryptoUtil.serializePublicKey(keyInfo.acsKey);
      keyPayload.sdkEphemPubKey = cryptoUtil.serializePublicKey(keyInfo.sdkKey);
      keyPayload.acsURL = "https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/acs/sdk/challenge";
      let jwsPayload = base64Url.encode(JSON.stringify(keyPayload));

      ares.acsSignedContent = cryptoUtil.createAcsSignedContent(jwsPayload, signaturePrivKey, signaturePubKey);
    }

    // check if the card expiry is 0131, if so
    // return that challenge is not needed with transStatus = "Y"
    if (areq.cardExpiryDate == "3101") {
      ares.transStatus = "Y";
      ares.acsChallengeMandated = "N";
      ares.eci = "05";

      const authValue = base64Url.encode("12345678901234567890");
      ares.authenticationValue = authValue;
    }

    dao.updateItem(areq, function (err) {
      if (err) {
        res.status(500);
        res.send("Internal Server Error");
        console.log("updateItem Error: " + err)
      }
    });

    res.status(200);
    res.send(ares);
  }
  else
  {
    res.status(400);
    res.send("Bad Request");
  }  
});

router.post('/result', function (req, res) {
  appInsights.defaultClient.trackNodeHttpRequest({request: req, response: res});
  console.log('Body: ', req.body);

  appInsights.defaultClient.trackTrace({request: req});
  if (req.headers["content-type"] != undefined && 
      req.headers["content-type"].startsWith("application/json"))
  {
    const resultReq = req.body;
    dao.getItem(resultReq.threeDSServerTransID, function(err, areq) 
    {
      console.log("AReq dn status: " + areq != undefined);
      if (areq != undefined)
      {        
        const authValue = base64Url.encode("12345678901234567890");
        const result =
        {
            messageType:"RReq",
            threeDSServerTransID: areq.threeDSServerTransID,
            interactionCounter:"00",
            messageCategory: "02",
            dsTransID: areq.acsTransID,
            acsTransID: areq.acsTransID,
            messageVersion: DEFAULT_MESSAGE_VERSION,
            eci: "05",
            transStatus: areq.transStatus,
            transStatusReason: areq.transStatusReason,
            challengeCancel: areq.challengeCancel,
            authenticationMethod: "02",
            authenticationType: "02",
        };

        if (areq.transStatus == "Y")
          result.authenticationValue = authValue;

        res.status(200);
        res.send(result);
    } 
    else {
      const result =
      {
          messageType:"RReq",
          threeDSServerTransID: resultReq.threeDSServerTransID,
          interactionCounter:"00",
          messageCategory: "02",
          acsTransID: resultReq.threeDSServerTransID,
          messageVersion: DEFAULT_MESSAGE_VERSION,
          eci: "05",
          transStatus: "N",
          transStatusReason: "10",
      };    
      res.status(200);
      res.send(result);
    }});

  } else {
    res.status(400);
    res.send("Bad Request");
  }  
});

router.post('/setstatus', function (req, res) {
  appInsights.defaultClient.trackNodeHttpRequest({request: req, response: res});
  console.log('Body: ', req.body);

  appInsights.defaultClient.trackTrace({request: req});
  if (req.headers["content-type"] != undefined && 
      req.headers["content-type"].startsWith("application/json"))
  {
    const statusReq = req.body;
    dao.getItem(statusReq.threeDSServerTransID, function(err, areq) 
    {
      console.log("AReq dn status: " + areq != undefined);
      if (areq != undefined)
      {        
        areq.transStatus = statusReq.transStatus;
        areq.transStatusReason = statusReq.transStatusReason;
        areq.challengeCancel = statusReq.challengeCancel;

        dao.updateItem(areq, function (err) {
          if (err) {
            res.status(500);
            res.send("Internal Server Error");
            console.log(err);
          }});

        res.status(200);
        res.send("OK");
      } 
    });
  } else {
    res.status(400);
    res.send("Bad Request");
  }    
});

module.exports = router;
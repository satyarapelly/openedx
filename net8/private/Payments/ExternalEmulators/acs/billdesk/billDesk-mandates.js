var express = require('express')
var bodyParser = require('body-parser');
const appInsights = require("applicationinsights");
const { DefaultAzureCredential } = require ('@azure/identity');
const CosmosClient = require("@azure/cosmos").CosmosClient;
var dbconfig = require('./../dbconfig');
var TransactionDao = require('./../transactionDao');
var fs = require('fs');
var uuid = require('uuid/v4');
const moment = require("moment");
const escape = require('escape-html');

var router = express.Router()

const credentials = new DefaultAzureCredential({ managedIdentityClientId: "d417dccc-67c5-4887-9e15-3b3a97732261" }); // CodeQL [SM05138] Not used in production, only for testing purposes;
var dbClient = new CosmosClient({ endpoint: dbconfig.host, aadCredentials: credentials });
var dao = new TransactionDao(dbClient, dbconfig.databaseId, dbconfig.collectionId);
console.log("db host: " + dbconfig.host);

// configure body-parser for express
router.use(bodyParser.urlencoded({ extended: false }));
router.use(bodyParser.json());
router.use(bodyParser.text(({ inflate: true, limit: '200kb', type: 'application/jose' })));

// middleware that is specific to this router
router.use(function timeLog(req, res, next) {
  console.log('Time: ', Date.now());
  next();
});

/*
  /mandates/create incoming params:
    mercid                  mandatory       unique id defined by billdesk for each merchant
    verification_type       mandatory       only applies for payment_method_type "bankaccount". values: "enach_nb", "enach_dc"
    customer_refid          mandatory       unique id for customer in the merchant system
    subscription_refid      mandatory       unique id assigned by merchant for the subscription
    subscription_desc       mandatory       customer friendly description for the subscription
    start_date              mandatory       
    end_date                mandatory
    amount_type             mandatory
    amount                  mandatory
    currency                mandatory       
    recurrence_rule         conditional     applicable for UPI values: "on", "before", "after"
    debit_day               conditional
*/
router.post('/mandates/create', function (req, res) {
  appInsights.defaultClient.trackNodeHttpRequest({ request: req, response: res });
  appInsights.defaultClient.trackTrace({ request: req });

  let mRes = JSON.parse(fs.readFileSync('billdesk/mandates-create-response.json', 'utf8'));
  const isJson = isJSON(req);
  const isJose = isJOSE(req);

  if (req.headers["content-type"] != undefined && (isJson || isJose)) {
    
    var mReq = req.body;

    if (!isJson) {
      try {
        mReq = JSON.parse(req.body);
      } catch (e) {
        res.status(400);
        res.send("Bad Request: Request body must be JSON");

        return;
      }
    }

    const validateErrorMsg = validateMandateCreateParams(mReq);

    if (validateErrorMsg !== "") {
      res.status(400);
      res.send(`Bad Request: ${validateErrorMsg}`);

      return;
    }

    mRes.objectid = mReq.objectid;
    mRes.mandateid = uuid();
    mRes.id = mRes.mandateid;
    mRes.mercid = mReq.mercid;
    mRes.customer_refid = mReq.customer_refid;
    mRes.subscription_refid = mReq.subscription_refid;
    mRes.subscription_desc = mReq.subscription_desc;
    mRes.start_date = mReq.start_date;
    mRes.end_date = mReq.end_date;
    mRes.amount_type = mReq.amount_type;
    mRes.amount = mReq.amount;
    mRes.currency = mReq.currency;
    mRes.frequency = mReq.frequency;
    mRes.additional_info = mReq.additional_info;
    mRes.payment_method_type = mReq.payment_method_type;
    mRes.status = "pending";
    mRes.ru = mReq.ru;
    mRes.createdon = moment().format("YYYY-MM-DDThh:mm:ssZ");
    mRes.customer = mReq.customer;
    mRes.device = mReq.device;

    switch(mReq.payment_method_type) {
      case "card":
        const cardRes = {};
        cardRes.holder_name = mReq.card && mReq.card.holder_name
        cardRes.masked_value = `xxxxxxxxxxxx${mReq.card && mReq.card.number 
          ? mReq.card.number.slice(-4) : 1234}`
        cardRes.paymentaccountid = "PA1CE000033241";
        mRes.card = cardRes;
        break;
      case "upi":
        const upiRes = {};
        upiRes.vpa = mReq.upi && mReq.upi.vpa;
        upiRes.psp = "abc";
        upiRes.paymentaccountid = "PA26A800033556";
        mRes.upi = upiRes;
        mRes.bank_umrn = "130a977ccabb11e7abc4cec278b6b50a@mypsp";
        mRes.debit_day = mReq.debit_day;
        mRes.recurrence_rule = mReq.recurrence_rule;
        break;
      case "bankaccount":
        const bankAccountRes = {};
        bankAccountRes.type = mReq.bankaccount && mReq.bankaccount.type;
        bankAccountRes.holder_name = mReq.bankaccount && mReq.bankaccount.holder_name;
        bankAccountRes.dest_bank_id = mReq.bankaccount && mReq.bankaccount.dest_bank_id;
        bankAccountRes.masked_value = `xxxxxxxx${mReq.bankaccount && mReq.bankaccount.number
          ? mReq.bankaccount.number.slice(-4) : 1234}`
        bankAccountRes.paymentaccountid = "PA23C700027699";
        mRes.bankaccount = bankAccountRes;
        mRes.verification_type = mReq.verification_type;
        mRes.debit_day = mReq.debit_day;
        break;
    }
      
    dao.updateItem(mRes, function (err) { });

    res.status(200);
    res.send(mRes);
  }
  else {
    res.status(400);
    res.send("Bad Request. content-type header is not defined or content-type header does not start with 'application/json' or 'application/jose");
  }
});

/*
  /mandates/update
    mandateid               mandatory     generated by billdesk in mandates/create call
    mercid                  mandatory     unique id defined by billdesk for each merchant
    response_parameters     mandatory     response params received from issuer (where next_step is "redirect") or the OTP provided by card holder (where next_step is "otp")
    card
*/
router.post('/mandates/update', function (req, res) {
  // For now return Dummy response sample from BillDesk doc
  appInsights.defaultClient.trackNodeHttpRequest({ request: req, response: res });
  appInsights.defaultClient.trackTrace({ request: req });

  const isJson = isJSON(req);
  const isJose = isJOSE(req);

  if (req.headers["content-type"] != undefined && (isJson || isJose)) {
    var mReq = req.body;

    if (!isJson) {
      try {
        mReq = JSON.parse(req.body);
      } catch (e) {
        res.status(400);
        res.send("Bad Request: Request body must be JSON");

        return;
      }
    }

    const validateErrorMsg = validateMandateUpdateParams(mReq);

    if (validateErrorMsg !== "") {
      res.status(400);
      res.send(`Bad Request: ${validateErrorMsg}`);

      return;
    }

    const mandateId = mReq.mandateid;

    dao.getItem(mandateId, function (err, mandate) {
      if (!mandate) {
        res.status(404);
        res.setHeader('Content-Type', 'text/plain');
        res.send(`Data for mandate id: ${escape(mandateId)} not found.`);

        return;
      }

      const mRes = mandate;
      
      switch (mandate.payment_method_type) {
        case "card":
        const cardRes = {};
        cardRes.holder_name = mReq.card && mReq.card.holder_name
        cardRes.masked_value = `xxxxxxxxxxxx${mReq.card && mReq.card.number 
          ? mReq.card.number.slice(-4) : 1234}`
        cardRes.paymentaccountid = "PA1CE000033241";
        mRes.card = cardRes;
        break;
      case "upi":
        const upiRes = {};
        upiRes.vpa = mReq.upi && mReq.upi.vpa;
        upiRes.psp = "abc";
        upiRes.paymentaccountid = "PA26A800033556";
        mRes.upi = upiRes;
        break;
      case "bankaccount":
        const bankAccountRes = {};
        bankAccountRes.type = mReq.bankaccount && mReq.bankaccount.type;
        bankAccountRes.holder_name = mReq.bankaccount && mReq.bankaccount.holder_name;
        bankAccountRes.dest_bank_id = mReq.bankaccount && mReq.bankaccount.dest_bank_id;
        bankAccountRes.masked_value = `xxxxxxxx${mReq.bankaccount && mReq.bankaccount.number
          ? mReq.bankaccount.number.slice(-4) : 1234}`
        bankAccountRes.paymentaccountid = "PA23C700027699";
        mRes.bankaccount = bankAccountRes;
        break;
      }
      
      dao.updateItem(mRes, function (err) { });

      res.status(200);
      res.send(mRes);
    });
  }
  else {
    res.status(400);
    res.send("Bad Request. content-type header is not defined or content-type header does not start with 'application/json' or 'application/jose");
  }
});

function validateMandateCreateParams(mreq) {
  let errorMsg = "";

  if (mreq.mercid === undefined) {
    errorMsg += `\n"mercid" is a required field`;
  }

  if (mreq.payment_method_type === "bankaccount" && mreq.verification_type === undefined) {
    errorMsg += `\n"verification_type" is a required field`;
  }

  if (mreq.customer_refid === undefined) {
    errorMsg += `\n"customer_refid" is a required field`;
  }

  if (mreq.subscription_refid === undefined) {
    errorMsg += `\n"subscription_refid" is a required field`;
  }

  if (mreq.subscription_desc === undefined) {
    errorMsg += `\n"subscription_desc" is a required field`;
  }

  if (mreq.start_date === undefined) {
    errorMsg += `\n"start_date" is a required field`;
  }

  if (mreq.end_date === undefined) {
    errorMsg += `\n"end_date" is a required field`;
  }

  if (mreq.amount_type === undefined) {
    errorMsg += `\n"amount_type" is a required field`;
  }

  if (mreq.amount === undefined) {
    errorMsg += `\n"amount" is a required field`;
  }

  if (mreq.currency === undefined) {
    errorMsg += `\n"currency" is a required field`;
  }

  if (mreq.start_date === undefined) {
    errorMsg += `\n"start_date" is a required field`;
  }

  if (mreq.frequency === undefined) {
    errorMsg += `\n"frequency" is a required field`;
  }

  if (mreq.payment_method_type === undefined) {
    errorMsg += `\n"payment_method_type" is a required field`;
  }

  if (mreq.device === undefined) {
    errorMsg += `\n"device" is a required field`;
  }

  return errorMsg
}

function validateMandateUpdateParams(mreq) {
  let errorMsg = "";

  if (mreq.mandateid === undefined) {
    errorMsg += `\n"mandateid" is a required field`;
  }

  if (mreq.mercid === undefined) {
    errorMsg += `\n"mercid" is a required field`;
  }

  if (mreq.response_parameters === undefined) {
    errorMsg += `\n"response_parameters" is a required field`;
  }

  if (mreq.card === undefined && mreq.upi === undefined && mreq.bankaccount === undefined) {
    errorMsg += `\nExpected a "card", "upi", or "bankaccount" object`;
  }

  return errorMsg
}

function isJSON(req) {
  return req.headers["content-type"].startsWith("application/json");
}

function isJOSE(req) {
  return req.headers["content-type"].startsWith("application/jose");
}

module.exports = router;

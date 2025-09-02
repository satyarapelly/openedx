const express = require('express');
const route = require("./acs");
const billDeskRoute = require("./billdesk/billDesk");
const billDeskMandatesRoute = require("./billdesk/billDesk-mandates");
const upi = require("./juspayUpi");

let appInsights = require("applicationinsights");

appInsights.setup("824d3504-5e69-4060-b012-637173a39c42").start();

let client = appInsights.defaultClient;

const app = express()

const port = 3000

app.use('/images', express.static(__dirname + '/images'));
app.use('/css', express.static(__dirname + '/css'));

app.get('/', (req, res) => { 
    appInsights.defaultClient.trackNodeHttpRequest({Request: req, Response: res});
    res.send('Hello World!');  
});
app.use('/acs', route);
app.use('/payments/ve1_2', billDeskRoute);
app.use('/pgsi/ve1_2', billDeskMandatesRoute);

// Adding upi route here to support UPI redirection in the emulator. In order to help partner teams to test the upi charge end to end through emulators. This emulates the juspay's waiting page - does not actually wait but just returns a successful charge status.
app.get('/upiRedirectPage/:sessionid/:environment/:mrn', (req, resp) => {
    let callbackUrl = upi.GetCallbackUrl(req);
    resp.redirect(callbackUrl);
})

app.get('/probe', (req, res) => { 
    res.send('OK');  
})

app.listen(port, () => console.log(`Example app listening on port ${port}!`));
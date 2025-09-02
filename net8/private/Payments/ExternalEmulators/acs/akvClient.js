const { DefaultAzureCredential } = require ('@azure/identity');
const { SecretClient } = require('@azure/keyvault-secrets');
const akvConfig = require('./akvConfig');
const crypto = require('crypto');
const cryptoUtil = require('./cryptoUtil');
const fs = require('fs');

function akvClient() {
}

module.exports = akvClient;

akvClient.prototype = {

    // This is method is used generate secret keys
    // The generated keys can be stored in the AKV for the ACS Emulator Private Key Encryption/Decryption
    generateRandomKey: function() {
        var key = crypto.randomBytes(16);
        var encodedKey = key.toString("base64")
        console.log("key: " + encodedKey);

        var iv = crypto.randomBytes(16);
        var encodedIV = iv.toString("base64");
        console.log("iv: " + encodedIV);
    },

    encryptWithAKV: async function(keyFile, targetFile) {
        // create Managed Identity and bind to the app
        const credentials = new DefaultAzureCredential({ managedIdentityClientId: "d417dccc-67c5-4887-9e15-3b3a97732261" }); // CodeQL [SM05138] Not used in production, only for testing purposes
        const client = new SecretClient(akvConfig.akv_url, credentials);

        const keyData = await client.getSecret("acs-signature-encryption-key", { version: "1f4a6ceb3c4941b18e49d6765cde9581" }); // DevSkim: ignore DS117838 as this is not a secret
        const ivData = await client.getSecret("acs-signature-encryption-iv", { version: "796585485bf444b992718fa728727f60" }); // DevSkim: ignore DS117838,DS173237 as this is not a secret

        const key = Buffer.from(keyData.value, "base64");
        const iv = Buffer.from(ivData.value, "base64");

        var content = fs.readFileSync(keyFile, "utf-8");
        var decKey = cryptoUtil.encryptContent(key, iv, content);
        fs.writeFileSync(targetFile, decKey.toString("base64"), "utf-8");
    },

    decryptWithAKV: async function (contentFile) {
        const credentials = new DefaultAzureCredential({ managedIdentityClientId: "d417dccc-67c5-4887-9e15-3b3a97732261" }); // CodeQL [SM05138] Not used in production, only for testing purposes
        const client = new SecretClient(akvConfig.akv_url, credentials);

        const keyData = await client.getSecret("acs-signature-encryption-key", { version: "1f4a6ceb3c4941b18e49d6765cde9581" }); // DevSkim: ignore DS117838 as this is not a secret
        const ivData = await client.getSecret("acs-signature-encryption-iv", { version: "796585485bf444b992718fa728727f60" }); // DevSkim: ignore DS117838,DS173237 as this is not a secret

        const key = Buffer.from(keyData.value, "base64");
        const iv = Buffer.from(ivData.value, "base64");

        var endData = fs.readFileSync(contentFile, "utf-8");
        var content = cryptoUtil.decryptContent(key, iv, Buffer.from(endData, "base64"));

        return content;
    }

};

var akv = new akvClient();

// NOTE: Don't delete this code
// This is a test function and doesn't get executed in the production, however it is needed to update the certificates.
// a. make sure that you place the acsPriv.key (Private Key Cert) and acsCert.pem (Public Certificate) in the folder before running this test
// b. Remove the PLACE HOLDER lines from the acsCert.pem before executing step c
// c. How to run, node advClient.js 
// (async function() {
//     await akv.encryptWithAKV("acsPriv.key", "acs_signature_data.dat");
//     await akv.encryptWithAKV("acsCert.pem", "acs_signature_public_key.dat");

//     var pubKey = "";
//     akv.decryptWithAKV("acs_signature_public_key.dat").then((key) => {
//         pubKey = key;
//     });

//     var privKey = "";
//     await akv.decryptWithAKV("acs_signature_data.dat").then((key) => {
//         privKey = key;
//     });

//     setTimeout(() => {
//         var content = cryptoUtil.createAcsSignedContent(content, privKey, pubKey);
//         console.log(content);
//     }, 1000);

// })().then(k => {
//     console.log();
// });

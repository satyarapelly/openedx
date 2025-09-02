const crypto = require('crypto');
const base64Url = require('base64url');
const util = require('util');
const fs = require('fs');

module.exports = {

    serializePublicKey:  function (publiceKey)
    {
        var x = publiceKey.slice(1, 33);
        var y = publiceKey.slice(33, 65);
        
        var eccPublicKey = {};
        
        eccPublicKey.kty = 'EC';
        eccPublicKey.crv = 'P-256';
        eccPublicKey.x = base64Url.encode(x);
        eccPublicKey.y = base64Url.encode(y);
        
        return eccPublicKey;
    },

    deserializePublicKey: function (jwk)
    {
        var x = base64Url.toBuffer(jwk.x);
        var y = base64Url.toBuffer(jwk.y);

        var bufArray = [Buffer.from([0x04]), x, y];
        var publicKey = Buffer.concat(bufArray);
        return publicKey;
    },

    convertInt32ToBuffer: function (value)
    {
        var buffer = Buffer.alloc(4);
        buffer.writeInt32BE(value);
        return buffer;
    },

    convertLongToBuffer: function (value)
    {
        var left = this.convertInt32ToBuffer(0);
        var valueBuf = this.convertInt32ToBuffer(value);
        return Buffer.concat([left, valueBuf]);
    },

    createSecretPrepend: function ()
    {
        return this.convertInt32ToBuffer(1);
    },

    createSecretAppend: function (sdkRef)
    {
        var algIdLen =  this.convertInt32ToBuffer(0);
        const partyULen = this.convertInt32ToBuffer(0);
        var partyVLen = this.convertInt32ToBuffer(sdkRef.length);
        var partyV = new Buffer(sdkRef);

        const keyDataLen = 256;
        const suppPubInfoLen = this.convertInt32ToBuffer(keyDataLen);
        const suppPrivInfoLen = Buffer.alloc(0);
        
        var append = Buffer.concat([algIdLen, partyULen, partyVLen, partyV, suppPubInfoLen, suppPrivInfoLen]);
        return append;
    },

    createSharedSecret: function (otherPartyKey, sdkReferenceNo)
    {
        const ecdh = crypto.createECDH('prime256v1');
        var pubKey = ecdh.generateKeys();

        var shallowSharedKey = ecdh.computeSecret(otherPartyKey);
        var hash = crypto.createHash('sha256');

        var secretAppend = this.createSecretAppend(sdkReferenceNo);
        var secretPrepend = this.createSecretPrepend();

        var keyMetrial = Buffer.concat([secretPrepend, shallowSharedKey, secretAppend]);
        hash.update(keyMetrial);
        var derivedKey = hash.digest();

        var keyInfo = { };
        keyInfo.sharedKey = derivedKey,
        keyInfo.acsKey = pubKey;
        keyInfo.sdkKey = otherPartyKey;

        return keyInfo;
    },

    createSignature: function (hmacKey, content)
    {
        var hmac = crypto.createHmac("sha256", hmacKey);
        hmac.update(content);
        var signature = hmac.digest();
        return signature.slice(0, 16);
    },

    encryptContent: function (encKey, iv, content)
    {
        var encipher = crypto.createCipheriv('aes-128-cbc', encKey, iv);
        encipher.setAutoPadding(true);
        var encBuf1 = encipher.update(content);
        var encBuf2 = encipher.final();        
        return Buffer.concat([encBuf1, encBuf2]);
    },

    decryptContent: function (encKey, iv, encContent)
    {
        var cbc = crypto.createDecipheriv('aes-128-cbc', encKey, iv);
        cbc.setAutoPadding(true);
        var decBuf1 = cbc.update(encContent);
        var decBuf2 =  cbc.final();
        return Buffer.concat([decBuf1, decBuf2]);
    },

    createAcsSignedContent: function (content, privateKey, publicKey)
    {
        const header = {"alg":"PS256","x5c":[publicKey]};
        var headerString = base64Url.encode(JSON.stringify(header));
        var message = util.format('%s.%s', headerString, content);

        const signer = crypto.createSign('RSA-SHA256');
        signer.update(message);
        signer.end();

        var privateKeyObj = crypto.createPrivateKey(privateKey);
        privateKeyObj.padding = crypto.constants.RSA_PKCS1_PSS_PADDING;
        privateKeyObj.saltLength = crypto.constants.RSA_PSS_SALTLEN_DIGEST;
        
        const signature = signer.sign(privateKeyObj);
        const encodedSignature = base64Url.encode(signature);

        var jws = util.format('%s.%s.%s', headerString, content, encodedSignature);  
        return jws;  
    },

    createJose: function (sharedKey, content, kid)
    {
        var iv = crypto.randomBytes(16);
        var encContent = this.encryptContent(sharedKey.slice(16, 32), iv, content);

        var header = {};
        header.alg = 'dir';
        header.kid = kid;
        header.enc = 'A128CBC-HS256';
        var headerStr = base64Url.encode(JSON.stringify(header));
        var headerBuf = Buffer.from(headerStr);
        var headerLen = this.convertLongToBuffer(headerBuf.byteLength * 8);

        var macContent = Buffer.concat([headerBuf, iv, encContent, headerLen]);
        var signature = this.createSignature(sharedKey.slice(0, 16), macContent);
    
        var jose = util.format('%s..%s.%s.%s', headerStr, base64Url.encode(iv), base64Url.encode(encContent), base64Url.encode(signature));  
        return jose;
    }  
};

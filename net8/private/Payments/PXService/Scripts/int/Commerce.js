/// <reference path="jquery-1.7.1.intellisense.js" />

//  
//  Clients which can consume PIDL, can make use of Commerce.js for
//  1) reading the user input data from the html document  
//  2) validating user input data
//  3) generating the user display data which can be displayed back to user for confirmation
//  4) tokenizing the user input data
//  5) generating the POST payload to the Service API
//  
//  The Clients can make use of the funtions from Commerce.js in the below sequence (pseudocode):
//  
//  // Read the user input data from the html document.
//  var userInputData = Commerce.readUserInputData(pidlDocument, inputIdPrefix, inputIdSuffix, onReadFail, operationType);
//  // Validate the user input data read from the html document.
//  Commerce.validateUserInputData(pidlDocument, userInputData, onValidationFail, operationType);
//  // Transform the user input data read from the html document.
//  Commerce.transformUserInputData(pidlDocument, userInputData, onFail, onComplete, operationType);
//  // Get the summary data to be displayed to the user and use the details from userDisplayData to display back to the user for confirmation.
//  var userDisplayData = commerce2.getUserDisplayData(pidlDocument, userInputData, onGetUserDisplayDataFail);
//  // Tokenize the user input data i.e. credit card number and CVV would be tokenized if pidlDocument is a payment method description of credit card family.
//  Commerce.tokenizeUserInputData(pidlDocument, userInputData, onTokenizationFail, onTokenizationComplete, operationType);
//  // POST the user input data (userInputData) to the Service API.

//  The Clients can make use of the funtions for multipage pidls in Commerce.js in the below sequence (pseudocode):
//  
//  // Read the user input data from the html document.
//  var userInputData = {}
// while(!lastpage) {
//  userInputData = Commerce.readUserInputDataPartial(pidlDocument,userInputData, inputIdPrefix, inputIdSuffix, onReadFail, operationType);
//  Commerce.validateUserInputDataPartial(pidlDocument,userInputData, inputIdPrefix, inputIdSuffix, onValidationFail, operationType);
//  page++;
//  }
//  // Validate the user input data read from the html document.
//  Commerce.validateUserInputData(pidlDocument, userInputData, onValidationFail, operationType);
//  // Transform the user input data read from the html document.
//  Commerce.transformUserInputData(pidlDocument, userInputData, onFail, onComplete, operationType);
//  // Get the summary data to be displayed to the user and use the details from userDisplayData to display back to the user for confirmation.
//  var userDisplayData = commerce2.getUserDisplayData(pidlDocument, userInputData, onGetUserDisplayDataFail);
//  // Tokenize the user input data i.e. credit card number and CVV would be tokenized if pidlDocument is a payment method description of credit card family.
//  Commerce.tokenizeUserInputData(pidlDocument, userInputData, onTokenizationFail, onTokenizationComplete, operationType);
//  // POST the user input data (userInputData) to the Service API.

this.commerce2 = function () {

    var internal = function () {
        return {
            constants: {
                pidlTransformationTargets: {
                    ForSubmit: "forSubmit",
                    ForDisplay: "forDisplay"
                },
                pidlResultType: {
                    Error: "error",
                    Fail: "failed",
                    Passed: "passed"
                },
            },

            config: {
                tranformationEndpoint: "https://paymentinstruments-int.mp.microsoft.com/v6.0",
                validationEndpoint: "https://paymentinstruments-int.mp.microsoft.com/v6.0",
                tokenizationEndpoint: "https://tokenization.cp.microsoft-int.com/tokens/",
            }
        }
    }();

    // Make sure JQuery is loaded
    if (typeof jQuery === "undefined") {
        throw new Error("JQuery is required by the commerce2.js script.");
    }

    var PIDLServiceURLPart = "InstrumentManagementService";
    var AddressDescriptionURLPart = "address-descriptions";
    var SupportedPIDLOperations = {
        Add: "Add",
        Update: "Update"
    }

    function getDataDescriptionFromPidlDocumentAndPidlIdentity(pidlDocument, pidlIdentity) {
        if (typeof pidlDocument === "undefined") {
            throw new Error("Input parameter 'pidlDocument' in commerce2.getDataDescriptionFromPidlDocumentAndPidlIdentity is undefined.");
        }

        if (typeof pidlIdentity === "undefined") {
            throw new Error("Input parameter 'pidlIdentity' in commerce2.getDataDescriptionFromPidlDocumentAndPidlIdentity is undefined.");
        }

        if (pidlDocument.length < 1) {
            throw new Error("Input parameter 'pidlDocument' in commerce2.getDataDescriptionFromPidlDocumentAndPidlIdentity has to have atleast 1 InfoDescription element.");
        }

        // retVal will contain the data description if found.  Otherwise, it is returned undefined.
        var retVal;

        for (var i = 0; i < pidlDocument.length; i++) {
            if (commerce2.areIdentitiesEqual(pidlDocument[i].identity, pidlIdentity)) {
                retVal = pidlDocument[i].data_description;
                break;
            }
        }

        return retVal;
    }

    function getDataDescriptionFromPidlDocumentAndUserInputData(pidlDocument, userInputData) {
        if (typeof pidlDocument === "undefined") {
            throw new Error("Input parameter 'pidlDocument' in commerce2.getDataDescriptionFromPidlDocumentAndUserInputData is undefined.");
        }

        if (typeof userInputData === "undefined") {
            throw new Error("Input parameter 'userInputData' in commerce2.getDataDescriptionFromPidlDocumentAndUserInputData is undefined.");
        }

        if (pidlDocument.length < 1) {
            throw new Error("Input parameter 'pidlDocument' in commerce2.getDataDescriptionFromPidlDocumentAndUserInputDatas has to have atleast 1 InfoDescription element.");
        }

        // retVal will contain the data description if found.  Otherwise, it is returned undefined.
        var retVal;

        for (var i = 0; i < pidlDocument.length; i++) {
            var keyPropertyName = getPropertyNameFromDataDescription(pidlDocument[i].data_description, "is_key", true);
            var keyPropertyValue = getPropertyValueFromInputData(userInputData, keyPropertyName);
            var keyPropertyDescriptor = getPropertyFromDataDescription(pidlDocument[i].data_description, keyPropertyName);
            var validationRegex = new RegExp(keyPropertyDescriptor.validation.regex);
            if (validationRegex.test(keyPropertyValue) === true) {
                retVal = pidlDocument[i].data_description;
                break;
            }
        }

        return retVal;
    }

    function getClientContextFromPidlDocumentAndUserInputData(pidlDocument, userInputData) {
        if (typeof pidlDocument === "undefined") {
            throw new Error("Input parameter 'pidlDocument' in commerce2.getClientContextFromPidlDocumentAndUserInputData is undefined.");
        }

        if (typeof userInputData === "undefined") {
            throw new Error("Input parameter 'userInputData' in commerce2.getClientContextFromPidlDocumentAndUserInputData is undefined.");
        }

        if (pidlDocument.length < 1) {
            throw new Error("Input parameter 'pidlDocument' in commerce2.getClientContextFromPidlDocumentAndUserInputData has to have atleast 1 InfoDescription element.");
        }

        // retVal will contain the client context if found.  Otherwise, it is returned undefined.
        var retVal;

        for (var i = 0; i < pidlDocument.length; i++) {
            var keyPropertyName = getPropertyNameFromDataDescription(pidlDocument[i].data_description, "is_key", true);
            var keyPropertyValue = getPropertyValueFromInputData(userInputData, keyPropertyName);
            var keyPropertyDescriptor = getPropertyFromDataDescription(pidlDocument[i].data_description, keyPropertyName);
            var validationRegex = new RegExp(keyPropertyDescriptor.validation.regex);
            if (validationRegex.test(keyPropertyValue) === true) {
                retVal = pidlDocument[i].clientContext;
                break;
            }
        }

        return retVal;
    }

    function getPropertyFromDataDescription(dataDescription, propertyName) {
        var retVal;
        for (var currProperty in dataDescription) {
            if (currProperty === propertyName) {
                retVal = dataDescription[currProperty];
                break;
            }
                // The property in the data_description could be a set of sub PIDL objects. Example: 'details' is a sub PIDL within Payment Method Descriptions PIDL.
            else if (Object.prototype.toString.call(dataDescription[currProperty]) === "[object Array]") {
                for (var i = 0; i < dataDescription[currProperty].length; i++) {
                    retVal = getPropertyFromDataDescription(dataDescription[currProperty][i].data_description, propertyName);
                    if (retVal !== undefined) {
                        break;
                    }
                }

                if (retVal !== undefined) {
                    break;
                }
            }
        }

        return retVal;
    }

    function getPropertyNameFromDataDescription(dataDescription, propertyAttributeName, propertyAttributeValue) {
        var retVal;
        for (var propertyName in dataDescription) {
            // The property in the data_description could be a set of sub PIDL objects. Example: 'details' is a sub PIDL within Payment Method Descriptions PIDL.
            if (Object.prototype.toString.call(dataDescription[propertyName]) === "[object Array]") {
                for (var i = 0; i < dataDescription[propertyName].length; i++) {
                    retVal = getPropertyNameFromDataDescription(dataDescription[propertyName][i].data_description, propertyAttributeName, propertyAttributeValue);
                    if (retVal !== undefined) {
                        break;
                    }
                }

                if (retVal !== undefined) {
                    break;
                }
            }
            else if (dataDescription[propertyName].hasOwnProperty(propertyAttributeName) && dataDescription[propertyName][propertyAttributeName] === propertyAttributeValue) {
                retVal = propertyName;
                break;
            }
        }

        return retVal;
    }

    function getPropertyValueFromInputData(userInputData, propertyName) {
        var retVal;
        for (var currProperty in userInputData) {
            if (currProperty === propertyName) {
                retVal = userInputData[currProperty];
                break;
            }
                // The property in the data_description could be a sub PIDL objects. Example: 'details' is a sub PIDL within Payment Method Descriptions PIDL.
            else if (Object.prototype.toString.call(userInputData[currProperty]) === "[object Object]") {
                retVal = getPropertyValueFromInputData(userInputData[currProperty], propertyName);
                if (retVal !== undefined) {
                    break;
                }
            }
        }

        return retVal;
    }

    function setPropertyValueOfInputData(userInputData, propertyName, propertyValue) {
        var retVal;
        for (var currProperty in userInputData) {
            if (currProperty === propertyName) {
                userInputData[currProperty] = propertyValue;
                break;
            }
                // The property in the data_description could be a sub PIDL objects. Example: 'details' is a sub PIDL within Payment Method Descriptions PIDL.
            else if (Object.prototype.toString.call(userInputData[currProperty]) === "[object Object]") {
                setPropertyValueOfInputData(userInputData[currProperty], propertyName, propertyValue);
            }
        }

        return retVal;
    }

    function resolveInfoDescriptionId(pidlDocument, keyPropertyValue, downloadIfNecessary) {
        var retVal;
        for (var i = 0; i < pidlDocument.length; i++) {
            var keyPropertyName = getPropertyNameFromDataDescription(pidlDocument[i].data_description, "is_key", true);
            var keyPropertyDescriptor = getPropertyFromDataDescription(pidlDocument[i].data_description, keyPropertyName);
            var validationRegex = new RegExp(keyPropertyDescriptor.validation.regex);
            if (validationRegex.test(keyPropertyValue) === true) {
                retVal = pidlDocument[i].identity;
                break;
            }
        }

        if (typeof retVal === "undefined" && pidlDocument[0].identity.description_type === "address" && downloadIfNecessary === true) {
            var pidlDocEndPoint;
            var selfUrl = pidlDocument[0].links.self.href;
            var pidlServiceRegex = new RegExp(PIDLServiceURLPart, 'i');
            var selfUrl = selfUrl.substring(0, selfUrl.search(PIDLServiceURLPart));
            pidlDocEndPoint = selfUrl + PIDLServiceURLPart + '/' + AddressDescriptionURLPart + '?type=' + pidlDocument[0].identity.type + "&country=" + keyPropertyValue;

            $.ajax({
                url: pidlDocEndPoint,
                type: "GET",
                dataType: "json",
                contentType: "application/json",
                async: false,
                headers: pidlDocument[0].links.self.headers,
                success: function (pidlDoc, textStatus, jqXHR) {
                    pidlDocument[pidlDocument.length] = pidlDoc;
                    retVal = pidlDoc.identity;
                },
                error: function (jqxhr, textStatus, error) {
                }
            });
        }

        return retVal;
    }

    function validatePidlOperation(pidlOperation) {
        if (pidlOperation !== SupportedPIDLOperations.Add && pidlOperation !== SupportedPIDLOperations.Update) {
            throw new Error("Input parameter 'pidlOperation' is not valid.");
        }
    }

    function checkForNullOrUndefined(value) {
        if ((value === undefined) || value == null) {
            return false;
        }

        return true;
    }

    function checkForValidTransformationTarget(propertyDescription, transformationTarget) {
        if (propertyDescription.hasOwnProperty("transformation")) {
            if (transformationTarget === internal.constants.pidlTransformationTargets.ForSubmit) {
                if (propertyDescription.transformation.forSubmit) {
                    return true;
                }
            }
            else if (transformationTarget === internal.constants.pidlTransformationTargets.ForDisplay) {
                if (propertyDescription.transformation.forDisplay) {
                    return true;
                }
            }
            else {
                return false;
            }
        }
        else {
            return false;
        }
    }

    function handleAjaxFailure(jqxhr, textStatus, error) {

        var msg;

        if (jqxhr.status === 0) {
            msg = 'Network connection failed';
        } else if (jqxhr.status == 404) {
            msg = 'Requested page not found. [404]';
        } else if (jqxhr.status == 500) {
            msg = 'Internal Server Error [500].\n' + jqxhr.responseText;
        } else if (exception === 'parsererror') {
            msg = 'Requested JSON parse failed.';
        } else if (exception === 'timeout') {
            msg = 'Time out error.';
        } else if (exception === 'abort') {
            msg = 'Ajax request aborted.';
        } else {
            msg = 'Uncaught Error.\n' + jqxhr.responseText;
        }

        return {
            status: internal.constants.pidlResultType.Error,
            errorMessage: msg
        }
    }

    function transformUserInputDataFromService(pidlDocument, propertyName, transformationTarget, propertyValue, url, onTransformationComplete) {
        var transformationServiceEndPoint = internal.config.tranformationEndpoint;
        var transformationUrl = transformationServiceEndPoint + url;

        if (propertyValue === undefined) {
            throw new Error("DataDescription contains a field '" + propertyName + "' which is missing in the userInputData passed in.");
        }

        var pidlIdentity;
        if (pidlDocument.length == 1) {
            pidlIdentity = pidlDocument[0].identity;
        } else if (pidlDocument.length > 1) {
            throw new Error("Multiple matching Pidl Document for property '" + propertyName);
        }

        var transformationResult;
        var postData = '{ "value": "' + propertyValue + '", "pidlIdentity": ' + JSON.stringify(pidlIdentity) + ', "propertyName":"' + propertyName + '", "transformationTarget":"' + transformationTarget + '" }';
        (function (propertyName) {
            $.ajax({
                url: transformationUrl,
                type: "POST",
                dataType: "json",
                xhrFields: { withCredentials: true },
                contentType: "application/json",
                data: postData,
                success: function (transformedData, textStatus, jqXHR) {
                    if (transformedData.status.toLowerCase() === internal.constants.pidlResultType.Passed) {
                        transformationResult = {
                            transformedValue: transformedData.transformedValue,
                            status: internal.constants.pidlResultType.Passed,
                        };
                    }
                    else {
                        transformationResult = {
                            status: internal.constants.pidlResultType.Fail,
                            errorCode: transformedData.errorCode,
                            errorMessage: transformedData.errorMessage
                        };
                    }
                    onTransformationComplete(transformationResult);
                },
                error: function (jqxhr, textStatus, error) {
                    transformationResult = handleAjaxFailure(jqxhr, textStatus, error);
                    onTransformationComplete(transformationResult);
                }
            });
        })(propertyName);
    }

    function validateUserInputDataFromService(pidlDocument, propertyName, propertyValue, url, OnValidationComplete) {
        var validationServiceEndPoint = internal.config.validationEndpoint;
        var validationUrl = validationServiceEndPoint + url;

        if (!propertyValue) {
            throw new Error("DataDescription contains a field '" + propertyName + "' which is missing a value to validate");
        }

        var pidlIdentity;
        if (pidlDocument.length == 1) {
            pidlIdentity = pidlDocument[0].identity;
        } else if (pidlDocument.length > 1) {
            throw new Error("Multiple matching Pidl Document for property '" + propertyName);
        }

        var postData = '{ "value": "' + propertyValue + '", "pidlIdentity": ' + JSON.stringify(pidlIdentity) + ', "propertyName":"' + propertyName + '" }';
        var validationResult;
        (function (propertyName) {
            $.ajax({
                url: validationUrl,
                type: "POST",
                dataType: "json",
                xhrFields: { withCredentials: true },
                contentType: "application/json",
                data: postData,
                success: function (validatedData, textStatus, jqXHR) {
                    if (validatedData.status.toLowerCase() === internal.constants.pidlResultType.Passed) {
                        validationResult = {
                            status: internal.constants.pidlResultType.Passed,
                        };
                    }
                    else {
                        validationResult = {
                            status: internal.constants.pidlResultType.Fail,
                            errorCode: validatedData.errorCode
                        };
                    }
                    OnValidationComplete(validationResult);
                },
                error: function (jqxhr, textStatus, error) {
                    validationResult = handleAjaxFailure(jqxhr, textStatus, error);
                    OnValidationComplete(validationResult);
                }
            });
        })(propertyName);
    }

    function validateUserInputDataInternal(pidlDocument, userInputData, onValidationFail, pidlOperation, onComplete, onFail) {

        if (!pidlDocument) {
            throw new Error("Input parameter 'pidlDocument' in commerce2.validateUserInputData is null or undefined.");
        }

        if (!userInputData) {
            throw new Error("Input parameter 'userInputData' in commerce2.validateUserInputData is null or undefined.");
        }

        if (!onValidationFail) {
            throw new Error("Input parameter 'onValidationFail' in commerce2.validateUserInputData is null or  undefined.");
        }

        if (!pidlOperation) {
            pidlOperation = SupportedPIDLOperations.Add;
        }
        else {
            validatePidlOperation(pidlOperation);
        }

        var dataDescription;

        if (pidlDocument.length == 1) {
            dataDescription = pidlDocument[0].data_description;
        } else if (pidlDocument.length > 1) {
            dataDescription = getDataDescriptionFromPidlDocumentAndUserInputData(pidlDocument, userInputData);
            if (!dataDescription) {
                // If the data description is not resolved then the key property is not yet collected from the user, take the first pidl in that case
                dataDescription = pidlDocument[0].data_description;
            }
        }

        var validationCallPending = 0;
        var isValidationPending = false;

        for (var propertyName in dataDescription) {
            if (Object.prototype.toString.call(dataDescription[propertyName]) === '[object Array]') {
                validationCallPending++
                isValidationPending = validateUserInputDataInternal(
                    dataDescription[propertyName],
                    userInputData[propertyName],
                    function (propName, errorCode) {
                        onValidationFail(propName, errorCode);
                    },
                pidlOperation,
                function () {
                    validationCallPending--;
                    if (validationCallPending === 0) {
                        if (isValidationPending) {
                            if (onComplete) {
                                onComplete();
                            }
                        }
                    }
                },
                function (e) {
                    if (onFail) {
                        onFail(e)
                    }
                    else {
                        throw e;
                    }
                }) || isValidationPending;
            }
            else {
                if (pidlOperation === SupportedPIDLOperations.Update &&
                    (dataDescription[propertyName].hasOwnProperty("is_updatable") === false || dataDescription[propertyName]["is_updatable"] === false)) {
                    continue;
                }

                var propertyDescription = dataDescription[propertyName];
                var propertyValue = getPropertyValueFromInputData(userInputData, propertyName);

                if (!propertyValue) {
                    if (propertyDescription.hasOwnProperty("is_optional") === false || propertyDescription["is_optional"] === false) {
                        onValidationFail(propertyName, "required_field_empty");
                        continue;
                    }
                    else {
                        continue;
                    }
                }

                var isValidationPending = validatePropertyFromPropertyDescription(
                    pidlDocument,
                    propertyName,
                    propertyDescription,
                    propertyValue,
                    function (validationResult) {
                        if (validationResult.hasOwnProperty("mode")) {
                            if (validationResult.mode === "service") {
                                validationCallPending--;
                                if (validationCallPending == 0) {
                                    if (onComplete) {
                                        onComplete();
                                    }
                                }
                            }
                        }

                        if (validationResult.status === internal.constants.pidlResultType.Fail) {
                            var errorCode = null;
                            if (validationResult.errorCode) {
                                errorCode = validationResult.errorCode;
                            }
                            onValidationFail(propertyName, errorCode);
                        }

                        if (validationResult.status === internal.constants.pidlResultType.Error) {
                            if (onFail) {
                                onFail(new Error(validationResult.errorMessage));
                            }
                            else {
                                throw new Error(validationResult.errorMessage);
                            }
                        }
                    });

                if (isValidationPending) {
                    validationCallPending++;
                }
            }
        }

        if (validationCallPending == 0) {
            if (onComplete) {
                onComplete();
            }
        }

        if (validationCallPending > 0) {
            return true;
        }

        return false;
    }

    function validatePropertyInternal(pidlDocument, propertyName, userInputData, onValidationFail, onValidationComplete, onFail) {

        if (!pidlDocument) {
            throw new Error("Input parameter 'pidlDocument' in commerce2.validateProperty is null or undefined.");
        }

        if (!propertyName) {
            throw new Error("Input parameter 'propertyName' in commerce2.validateProperty is null or undefined.");
        }

        if (!userInputData) {
            throw new Error("Input parameter 'userInputData' in commerce2.validateProperty is null or undefined.");
        }

        if (!onValidationFail) {
            throw new Error("Input parameter 'onValidationFail' in commerce2.validateProperty is null or  undefined.");
        }

        if (!onValidationComplete) {
            throw new Error("Input parameter 'onValidationComplete' in commerce2.validateProperty is null or  undefined.");
        }

        var dataDescription;
        var foundProperty = false;

        if (pidlDocument.length == 1) {
            dataDescription = pidlDocument[0].data_description;
        } else if (pidlDocument.length > 1) {
            dataDescription = getDataDescriptionFromPidlDocumentAndUserInputData(pidlDocument, userInputData);
            if (!dataDescription) {
                // If the data description is not resolved then the key property is not yet collected from the user, take the first pidl in that case
                dataDescription = pidlDocument[0].data_description;
            }
        }

        for (var property in dataDescription) {
            if (Object.prototype.toString.call(dataDescription[property]) === '[object Array]') {
                foundProperty = validatePropertyInternal(
                    dataDescription[property],
                    propertyName,
                    userInputData,
                    function (propName, errorCode) {
                        onValidationFail(propName, errorCode);
                    },
                onValidationComplete,
                function (e) {
                    if (onFail) {
                        onFail(e)
                    }
                    else {
                        throw e;
                    }
                }) || foundProperty;
            }
            else {
                if (property === propertyName) {
                    var propertyDescription = dataDescription[property];
                    var propertyValue = getPropertyValueFromInputData(userInputData, propertyName);
                    validatePropertyFromPropertyDescription(
                        pidlDocument,
                        propertyName,
                        propertyDescription,
                        propertyValue,
                        function (validationResult) {
                            if (validationResult.status === internal.constants.pidlResultType.Passed) {
                                onValidationComplete();
                            }

                            if (validationResult.status === internal.constants.pidlResultType.Fail) {
                                var errorCode = null;
                                if (validationResult.errorCode) {
                                    errorCode = validationResult.errorCode;
                                }

                                onValidationFail(propertyName, errorCode);
                            }

                            if (validationResult.status === internal.constants.pidlResultType.Error) {
                                if (onFail) {
                                    onFail(new Error(validationResult.errorMessage));
                                }
                                else {
                                    throw new Error(validationResult.errorMessage);
                                }
                            }
                        });

                    return true;
                }
            }
        }

        return foundProperty;
    }

    function validatePropertyFromPropertyDescription(pidlDocument, propertyName, propertyDescription, propertyValue, onPropertyValidationComplete) {

        var validationResult = {};

        var isValidationPending = false;

        // This is a required property.  So, whether the DataDescription has a validation rule or not, 
        // this property cannot be left blank by the user.
        if (!propertyValue) {
            if (propertyDescription.hasOwnProperty("is_optional") === false || propertyDescription["is_optional"] === false) {
                validationResult = {
                    status: internal.constants.pidlResultType.Fail,
                    errorCode: "required_field_empty",
                };
                onPropertyValidationComplete(validationResult);
                return false;
            }
            else {
                validationResult = {
                    status: internal.constants.pidlResultType.Passed,
                };
                onPropertyValidationComplete(validationResult);
                return false;
            }
        }

        if (propertyDescription.hasOwnProperty("validation")) {
            if (propertyDescription.validation.hasOwnProperty("regex")) {
                var validationRegex = new RegExp(propertyDescription.validation.regex);
                if (validationRegex.test(propertyValue) === false) {

                    validationResult = {
                        status: internal.constants.pidlResultType.Fail,
                    };
                }
                else {
                    validationResult = {
                        status: internal.constants.pidlResultType.Passed
                    }
                }

                validationResult.mode = "regex";
            }
            else if (propertyDescription.validation.hasOwnProperty("url")) {
                isValidationPending = true;
                validateUserInputDataFromService(
                    pidlDocument,
                    propertyName,
                    propertyValue,
                    propertyDescription.validation.url,
                    function (result) {
                        validationResult = result;
                        validationResult.mode = "service";
                        onPropertyValidationComplete(validationResult);
                    });
            }
        } else {
            validationResult = {
                status: internal.constants.pidlResultType.Passed
            }
        }

        if (!isValidationPending) {
            onPropertyValidationComplete(validationResult);
        }

        return isValidationPending;
    }

    function transformPropertyInternal(pidlDocument, propertyName, userInputData, transformationTarget, onFail, onTransformationComplete) {

        if (!pidlDocument) {
            throw new Error("Input parameter 'pidlDocument' in commerce2.transformPropertyInternal is null or undefined.");
        }

        if (!propertyName) {
            throw new Error("Input parameter 'propertyName' in commerce2.transformPropertyInternal is null or undefined.");
        }

        if (!userInputData) {
            throw new Error("Input parameter 'userInputData' in commerce2.transformPropertyInternal is null or undefined.");
        }

        if (!onFail) {
            throw new Error("Input parameter 'onTransformationFail' in commerce2.transformPropertyInternal is null or  undefined.");
        }

        if (!onTransformationComplete) {
            throw new Error("Input parameter 'onTransformationComplete' in commerce2.transformPropertyInternal is null or  undefined.");
        }

        var dataDescription;
        var foundProperty = false;

        if (pidlDocument.length == 1) {
            dataDescription = pidlDocument[0].data_description;
        } else if (pidlDocument.length > 1) {
            dataDescription = getDataDescriptionFromPidlDocumentAndUserInputData(pidlDocument, userInputData);
            if (!dataDescription) {
                // If the data description is not resolved then the key property is not yet collected from the user, take the first pidl in that case
                dataDescription = pidlDocument[0].data_description;
            }
        }

        for (var property in dataDescription) {
            if (Object.prototype.toString.call(dataDescription[property]) === '[object Array]') {
                foundProperty = transformPropertyInternal(
                    dataDescription[property],
                    propertyName,
                    userInputData,
                    transformationTarget,
                    function (e) {
                        if (onFail) {
                            onFail(e)
                        }
                        else {
                            throw e;
                        }
                    },
                    onTransformationComplete) || foundProperty;
            }
            else {
                if (property === propertyName) {
                    var propertyDescription = dataDescription[property];
                    var propertyValue = getPropertyValueFromInputData(userInputData, propertyName);

                    transformPropertyFromPropertyDescription(
                        pidlDocument,
                        propertyName,
                        propertyDescription,
                        transformationTarget,
                        propertyValue,
                        function (transformationResult) {
                            if (transformationResult.status === internal.constants.pidlResultType.Passed) {
                                setPropertyValueOfInputData(userInputData, propertyName, transformationResult.transformedValue);
                                onTransformationComplete();
                            }

                            if ((transformationResult.status === internal.constants.pidlResultType.Error)
                                || (transformationResult.status === internal.constants.pidlResultType.Fail)) {
                                if (onFail) {
                                    onFail(new Error(transformationResult.errorMessage));
                                }
                                else {
                                    throw new Error(transformationResult.errorMessage);
                                }
                            }
                        });

                    if (checkForValidTransformationTarget(propertyDescription, transformationTarget)) {
                        return true;
                    }
                }
            }
        }

        return foundProperty;
    }

    function transformUserInputDataInternal(pidlDocument, userInputData, transformationTarget, onFail, onComplete, pidlOperation, isPartial) {

        if (!pidlDocument) {
            throw new Error("Input parameter 'pidlDocument' in commerce2.transformUserInputDataInternal is null or undefined.");
        }

        if (!userInputData) {
            throw new Error("Input parameter 'userInputData' in commerce2.transformUserInputDataInternal is null or undefined.");
        }

        if (!onFail) {
            throw new Error("Input parameter 'onFail' in commerce2.transformUserInputDataInternal is null or  undefined.");
        }

        if (!pidlOperation) {
            pidlOperation = SupportedPIDLOperations.Add;
        }
        else {
            validatePidlOperation(pidlOperation);
        }

        var dataDescription;

        if (pidlDocument.length == 1) {
            dataDescription = pidlDocument[0].data_description;
        } else if (pidlDocument.length > 1) {
            dataDescription = getDataDescriptionFromPidlDocumentAndUserInputData(pidlDocument, userInputData);
            if (!dataDescription) {
                // If the data description is not resolved then the key property is not yet collected from the user, take the first pidl in that case
                dataDescription = pidlDocument[0].data_description;
            }
        }

        var transformationCallPending = 0;
        var isTransformationPending = false;
        var propertyBag;

        if (isPartial) {
            propertyBag = userInputData;
        }
        else {
            propertyBag = dataDescription
        }

        for (var propertyName in propertyBag) {
            if (Object.prototype.toString.call(dataDescription[propertyName]) === '[object Array]') {
                transformationCallPending++;
                isTransformationPending = transformUserInputDataInternal(
                    dataDescription[propertyName],
                    userInputData[propertyName],
                    transformationTarget,
                    function (e) {
                        if (onFail) {
                            onFail(e)
                        }
                        else {
                            throw e;
                        }
                    }, function () {
                        transformationCallPending--;
                        if (transformationCallPending === 0) {
                            if (isTransformationPending) {
                                if (onComplete) {
                                    onComplete();
                                }
                            }
                        }
                    },
                pidlOperation,
                isPartial) || isTransformationPending;
            }
            else {
                if (pidlOperation === SupportedPIDLOperations.Update &&
                    (dataDescription[propertyName].hasOwnProperty("is_updatable") === false || dataDescription[propertyName]["is_updatable"] === false)) {
                    continue;
                }

                var propertyDescription = dataDescription[propertyName];
                var propertyValue = getPropertyValueFromInputData(userInputData, propertyName);

                if (!propertyValue) {

                    if (isPartial) {
                        continue;
                    }

                    if (propertyDescription.hasOwnProperty("is_optional") === false || propertyDescription["is_optional"] === false) {
                        var errorMessage = propertyName + " is a required property per the pidlDocument but is missing in the userInputData object passed in.";
                        if (onFail) {
                            onFail(new Error(errorMessage));
                        }
                        else {
                            throw new Error(errorMessage);
                        }
                    }
                }

                var isTransformationPending = transformPropertyFromPropertyDescription(
                    pidlDocument,
                    propertyName,
                    propertyDescription,
                    transformationTarget,
                    propertyValue,
                    function (transformationResult) {

                        if (transformationResult.status === internal.constants.pidlResultType.Passed) {
                            setPropertyValueOfInputData(userInputData, propertyName, transformationResult.transformedValue);
                        }

                        if ((transformationResult.status === internal.constants.pidlResultType.Error)
                            || (transformationResult.status === internal.constants.pidlResultType.Fail)) {
                            if (onFail) {
                                onFail(new Error(transformationResult.errorMessage));
                            }
                            else {
                                throw new Error(transformationResult.errorMessage);
                            }
                        }

                        if (transformationResult.hasOwnProperty("mode")) {
                            if (transformationResult.mode === "service") {
                                transformationCallPending--;
                                if (transformationCallPending == 0) {
                                    if (onComplete) {
                                        onComplete();
                                    }
                                }
                            }
                        }
                    });

                if (isTransformationPending) {
                    transformationCallPending++;
                }
            }
        }

        if (transformationCallPending == 0) {
            if (onComplete) {
                onComplete();
            }
        }

        if (transformationCallPending > 0) {
            return true;
        }

        return false;
    }

    function transformPropertyFromPropertyDescription(pidlDocument, propertyName, propertyDescription, transformationTarget, propertyValue, onPropertyTransformationComplete) {

        var transformationResult = {};

        var isTransformationPending = false;
        if (!propertyValue) {
            return false;
        }

        if (propertyDescription.hasOwnProperty("transformation")) {
            var target;
            if (transformationTarget === internal.constants.pidlTransformationTargets.ForSubmit) {
                if (propertyDescription.transformation.forSubmit) {
                    target = propertyDescription.transformation.forSubmit
                }
                else {
                    onPropertyTransformationComplete(transformationResult);
                    return isTransformationPending;
                }
            }
            else if (transformationTarget === internal.constants.pidlTransformationTargets.ForDisplay) {
                if (propertyDescription.transformation.forDisplay) {
                    target = propertyDescription.transformation.forDisplay
                } else {
                    onPropertyTransformationComplete(transformationResult);
                    return isTransformationPending;
                }
            }
            else {
                throw new Error(propertyName + "has invalid transformation type.");
            }

            if (target.hasOwnProperty("inputregex")) {
                var inputRegex = new RegExp(target.inputregex);

                if (target.hasOwnProperty("transformregex")) {
                    var outputValue = propertyValue.replace(inputRegex, target.transformregex);
                    transformationResult = {
                        status: internal.constants.pidlResultType.Passed,
                        transformedValue: outputValue
                    };
                }

                transformationResult.mode = "regex";
            }
            else if (target.hasOwnProperty("url")) {
                isTransformationPending = true;
                transformUserInputDataFromService(
                    pidlDocument,
                    propertyName,
                    transformationTarget,
                    propertyValue,
                    target.url,
                    function (result) {
                        transformationResult = result;
                        transformationResult.mode = "service";
                        onPropertyTransformationComplete(transformationResult);
                    });
            }
        }

        if (!isTransformationPending) {
            onPropertyTransformationComplete(transformationResult);
        }

        return isTransformationPending;
    }

    function readUserInputDataInternal(pidlDocument, userInputData, inputIdPrefix, inputIdSuffix, onFail, pidlOperation, isPartial) {
        // Validate input parameters
        if (!pidlDocument) {
            throw new Error("Input parameter 'pidlDocument' in commerce2.readUserInputData is undefined.");
        }

        if (!checkForNullOrUndefined(inputIdPrefix)) {
            throw new Error("Input parameter 'inputIdPrefix' in commerce2.readUserInputData is undefined.");
        }

        if (!checkForNullOrUndefined(inputIdSuffix)) {
            throw new Error("Input parameter 'inputIdSuffix' in commerce2.readUserInputData is undefined.");
        }

        if (!onFail) {
            throw new Error("Input parameter 'onFail' in commerce2.readUserInputData is undefined.");
        }

        if (!pidlOperation) {
            pidlOperation = SupportedPIDLOperations.Add;
        }
        else {
            validatePidlOperation(pidlOperation);
        }

        if (pidlDocument.length < 1) {
            throw new Error("Input parameter 'pidlDocument' in commerce2.readUserInputData has to have atleast 1 InfoDescription element.");
        }

        var dataDescription;
        if (pidlDocument.length == 1) {
            dataDescription = pidlDocument[0].data_description;
        }
        else {
            // The pidlDocument represents a collection of infoDescriptions.  We need to resolve
            // it to the correct infoDescription based on the keyProperty's value.
            var keyPropertyName = getPropertyNameFromDataDescription(pidlDocument[0].data_description, "is_key", true);
            var keyProperty = $("#" + inputIdPrefix + keyPropertyName + inputIdSuffix);
            // The KeyProperty is not in DOM then default to the first pidl datadescription
            if (keyProperty.length == 0) {
                dataDescription = pidlDocument[0].data_description;
            }
            else {
                var keyPropertyValue = $("#" + inputIdPrefix + keyPropertyName + inputIdSuffix).val();
                var pidlIdentity = commerce2.resolveInfoDescriptionId(pidlDocument, keyPropertyValue);

                if (!pidlIdentity) {
                    onFail(new Error("The " + keyPropertyName + " entered was not resolved to any pidl document"));
                    return;
                }

                dataDescription = getDataDescriptionFromPidlDocumentAndPidlIdentity(pidlDocument, pidlIdentity);
            }
        }

        for (var propertyName in dataDescription) {
            if (Object.prototype.toString.call(dataDescription[propertyName]) === "[object Array]") {

                // For partial read case check if the input data already has a value. If it has a value then pass it as it is in the recursive read
                var input = userInputData[propertyName];

                if (!input) {
                    input = {};
                }

                userInputData[propertyName] = readUserInputDataInternal(dataDescription[propertyName], input, inputIdPrefix, inputIdSuffix, onFail, pidlOperation, isPartial);
            }
            else {
                if (pidlOperation === SupportedPIDLOperations.Update &&
                    (dataDescription[propertyName].hasOwnProperty("is_updatable") === false || dataDescription[propertyName]["is_updatable"] === false)) {
                    continue;
                }

                if (dataDescription[propertyName]["type"] === "hidden") {
                    if (!userInputData[propertyName]) {
                        userInputData[propertyName] = dataDescription[propertyName]["default_value"];
                    }
                    continue;
                }

                var propertyDescription = dataDescription[propertyName];

                var id = inputIdPrefix + propertyName + inputIdSuffix;
                var inputElement = $("#" + id);

                if (inputElement.length === 0) {
                    if (!isPartial) {
                        onFail(new Error("Element with id: '" + id + "' was not found in the html document."));
                        return;
                    }
                    else {
                        continue;
                    }
                }

                if (propertyDescription.hasOwnProperty("is_optional") === false || (propertyDescription["is_optional"] === false)) {
                    // This is a required field.  Assign whatever value found in the html element
                    // to the return object.
                    userInputData[propertyName] = inputElement.val();
                    if (dataDescription[propertyName]["type"] === "bool") {
                        if (inputElement.is(":checked")) {
                            userInputData[propertyName] = true;
                        } else {
                            userInputData[propertyName] = false;
                        }
                    }
                }
                else {
                    if (inputElement.val() !== "" && inputElement.val() !== undefined) {
                        // This is an optional field as indicated by the dataDescription object. Assign the 
                        // value of this field to the return object only if it is not empty and not undefined.
                        userInputData[propertyName] = inputElement.val();
                    }
                }
            }
        }

        return userInputData;
    }

    return {
        // function commerce2.areIdentitiesEqual
        //   This function takes identities of two pidlObjects and returns a boolean indicating if the
        //   two pidlObjects are the same
        //
        // parameters identityA and identityB
        //   The identity objects of the two pidlObjects that need to be compared
        //
        areIdentitiesEqual: function (identityA, identityB) {
            if (typeof identityA == "undefined" || typeof identityB == "undefined") {
                return false;
            }

            if (identityA.length != identityB.length) {
                return false;
            }

            var retVal = true;
            for (var key in identityA) {
                if (identityA[key] != identityB[key]) {
                    retVal = false;
                    break;
                }
            }

            return retVal;
        },

        // function commerce2.resolveInfoDescriptionId
        //   This function takes a pidlDocument (which is an array of pidlResources) and a keyPropertyValue
        //   and helps decide which of the pidlResources in the arrays is applicable (based 
        //   on the keyPropertyValue).
        //
        // parameter pidlDocument
        //   An array of pidlObjects.  For example, when the client gets a pidlDocument for credit cards,
        //   the array contains one pidlResource each for visa, amex, mastercard, discover etc.
        //
        // parameter keyPropertyValue
        //   A value that the user has entered that can help determine which specific element in the array
        //   is applicable.  e.g. in the case of credit cards, the credit card number would be the keyPropertyValue
        //   since the credit card number identifies wiht certainty, the type of credit card.
        //
        resolveInfoDescriptionId: function (pidlDocument, keyPropertyValue) {
            return resolveInfoDescriptionId(pidlDocument, keyPropertyValue, true);
        },

        // function commerce2.readUserInputData
        //   This function reads user input data from input elements on the html document
        //   and returns a userInputData object that can then be passed to other functions like 
        //   commerce2.validateUserInputData and commerce2.tokenizeUserInputData.  The return object
        //   has the same attributes as the pidlDocument[i].data_description object except that  
        //   the values of these attributes contain user input data.
        //
        // parameters inputIdPrefix and inputIdSuffix
        //   It is expected that the ids of input fileds on the html document are of the form
        //   inputIdPrefix + property names in pidlDocument + inputIdSuffix
        //
        // parameter pidlOperation
        //   Use one of the values from commerce2.PIDLOperations to pass to this parameter. The scenario in which this method is 
        //   called has to be sent as this parameter i.e. "Add" / "Update". Depending on the scenario, only certain fields are 
        //   read based on pidlDocument.
        //
        readUserInputData: function (pidlDocument, inputIdPrefix, inputIdSuffix, onFail, pidlOperation) {
            var userInputData = {};
            userInputData = readUserInputDataInternal(
                pidlDocument,
                userInputData,
                inputIdPrefix,
                inputIdSuffix,
                onFail,
                pidlOperation);
            return userInputData;
        },

        // function commerce2.readUserInputDataPartial
        //   This function reads partial user input data from input elements on the html document
        //   and returns a partial userInputDataPartial object that can then be passed to other functions like 
        //   commerce2.validateUserInputDataPartial The return object
        //   has the same partial attributes as the pidlDocument[i].data_description object except that  
        //   the values of these attributes contain user input data.
        //
        // parameters inputIdPrefix and inputIdSuffix
        //   It is expected that the ids of input fileds on the html document are of the form
        //   inputIdPrefix + property names in pidlDocument + inputIdSuffix
        //
        // parameter pidlOperation
        //   Use one of the values from commerce2.PIDLOperations to pass to this parameter. The scenario in which this method is 
        //   called has to be sent as this parameter i.e. "Add" / "Update". Depending on the scenario, only certain fields are 
        //   read based on pidlDocument.
        //
        readUserInputDataPartial: function (pidlDocument, userInputDataPartial, inputIdPrefix, inputIdSuffix, onFail, pidlOperation) {
            return readUserInputDataInternal(
                pidlDocument,
                userInputDataPartial,
                inputIdPrefix,
                inputIdSuffix,
                onFail,
                pidlOperation,
                true);
        },

        // function commerce2.getUserDisplayData
        //   This function constructs and returns an object with the properties and values that can be displayed back to the 
        //   user. The return object has the same attributes as the userInputData object except:
        //   i.   the 'hidden' properties would not be present
        //   ii.  the tokenizable property values are masked appropriately i.e. credit card number and CVV
        //   iii. the (localized) values of the 'possible_values' from the pidlDocument are filled using their corresponding 
        //        keys provided in userInputData. For example, if the provided pidlDocument is generated with language set to Spanish(ES) 
        //        then 'Estados Unidos' would be filled for country in the userDisplayData if the user input data contains the country 
        //        key as 'US' in userInputData.
        //
        // parameter pidlDocument
        //   This is the object returned by the GET /payment-method-description call
        //
        // parameter userInputData
        //   This is an object with user's input data in a format specified by the pidlDocument.
        //   Call commerce2.readUserInputData function to get this object.
        //
        // parameter onFail
        //   This is the callback function that is called for the errors encountered in this function. Example: When there
        //   is inconsistency between the data in userInputData and pidlDocument
        //
        getUserDisplayData: function (pidlDocument, userInputData, onFail, onComplete, pidlOperation) {
            var userDisplayData = {}
            userDisplayData = $.extend(true, {}, userInputData);
            transformUserInputDataInternal(
                pidlDocument,
                userDisplayData,
                internal.constants.pidlTransformationTargets.ForDisplay,
                onFail,
                function onTransformationComplete() {
                    onComplete(userDisplayData);
                },
                pidlOperation);
        },

        // function commerce2.getUserDisplayDataPartial
        //   This function constructs and returns an object with the properties and values that can be displayed back to the 
        //   user. This variant of the function can work on partial data and does not require the entire data to be present.
        // parameter pidlDocument
        //   This is the object returned by the GET /payment-method-description call
        //
        // parameter userInputData
        //   This is an object with user's input data in a format specified by the pidlDocument.
        //   Call commerce2.readUserInputData function to get this object.
        //
        // parameter onFail
        //   This is the callback function that is called for the errors encountered in this function. Example: When there
        //   is inconsistency between the data in userInputData and pidlDocument
        //
        getUserDisplayDataPartial: function (pidlDocument, userInputData, onFail, onComplete, pidlOperation) {
            var userDisplayData = {}
            userDisplayData = $.extend(true, {}, userInputData);
            transformUserInputDataInternal(
                pidlDocument,
                userDisplayData,
                internal.constants.pidlTransformationTargets.ForDisplay,
                onFail,
                function onTransformationComplete() {
                    onComplete(userDisplayData);
                },
                pidlOperation,
                true);
        },

        // function commerce2.validateUserInputData
        //   This function iterates through every property in the userInputData, performs validation checks
        //   and calls onValidationFail for every input field that fails validaton.
        //
        // parameter pidlDocument
        //   This is the object returned by the GET /payment-method-description call
        //
        // parameter userInputData
        //   This is an object with user's input data in a format specified by the pidlDocument.
        //   Call commerce2.readUserInputData function to get this object.
        //
        // parameter onValidationFail
        //   This is the callback function that is called for every property that fails validation
        //
        // parameter pidlOperation
        //   Use one of the values from commerce2.PIDLOperations to pass to this parameter. The scenario in which this method is 
        //   called has to be sent as this parameter i.e. "Add" / "Update". Depending on the scenario, only certain fields in 
        //   userInputData are validated based on pidlDocument.
        //
        // parameter onComplete
        //   This is the callback function that is called after the validation is completed on every property.

        validateUserInputData: function (pidlDocument, userInputData, onValidationFail, pidlOperation, onComplete, onFail) {
            validateUserInputDataInternal(
                pidlDocument,
                userInputData,
                onValidationFail,
                pidlOperation,
                onComplete,
                onFail);
        },

        // function commerce2.validatePropertyByName
        //   This function iterates through the properties and validates the matching property against the pidl metadata
        //
        // parameter pidlDocument
        //   This is the object returned by the GET /payment-method-description call
        //
        // parameter propertyName
        //   This is the name of the property that needs to be validated
        //
        // parameter userInputData
        //   The user input data that contains the value of property
        //
        // parameter onValidationFail
        //   This is the callback function that is called for every property that fails validation
        //
        // parameter onValidationComplete
        //   This is the callback function that is called when the Property Validation is complete
        //
        // parameter onFail
        //   This is the callback function that is called when the underlying validation operation fails
        validateUserInputProperty: function (pidlDocument, propertyName, userInputData, onValidationFail, onComplete, onFail) {
            if (!validatePropertyInternal(
                pidlDocument,
                propertyName,
                userInputData,
                onValidationFail,
                onComplete,
                onFail)) {

                if (onFail) {
                    onFail(Error("The property with property Name:" + propertyName + " not found in the data description"));
                }
                else {
                    throw new Error("The property with property Name:" + propertyName + " not found in the data description");
                }
            }
        },

        // function commerce2.transformUserInputData
        //   This function iterates through every property in the userInputData, performs transformation if they are defined
        //
        // parameter pidlDocument
        //   This is the object returned by the GET /payment-method-description call
        //
        // parameter userInputData
        //   This is an object with user's input data in a format specified by the pidlDocument.
        //   Call commerce2.readUserInputData function to get this object.
        //
        // parameter onFail
        //   This is the callback function that is called for every transformation that fails transformation
        //
        // parameter onComplete
        //   This is the callback function that is called after the transformation is completed on every property.
        //
        // parameter pidlOperation
        //   Use one of the values from commerce2.PIDLOperations to pass to this parameter. The scenario in which this method is 
        //   called has to be sent as this parameter i.e. "Add" / "Update". Depending on the scenario, only certain fields in 
        //   userInputData are transformed based on pidlDocument.
        //
        transformUserInputData: function (pidlDocument, userInputData, onFail, onComplete, pidlOperation) {
            transformUserInputDataInternal(
                pidlDocument,
                userInputData,
                internal.constants.pidlTransformationTargets.ForSubmit,
                onFail,
                onComplete,
                pidlOperation);
        },

        // function commerce2.transformPropertyForSubmitByName
        //   This function iterates through the data description and applies transforms to the property if applicable for submit
        //
        // parameter pidlDocument
        //   This is the object returned by the GET /payment-method-description call
        //
        // parameter propertyName
        //   The name of the property that needs to be transformed
        //
        // parameter userInputData
        //   The user input object that contains the value of the property that needs to be transformed
        //
        // parameter onFail
        //   This is the callback function that is called for every transformation that fails transformation
        //
        // parameter onComplete
        //   This is the callback function that is called after the transformation is completed on every property.
        //
        transformUserInputPropertyForSubmit: function (pidlDocument, propertyName, userInputData, onFail, onComplete) {
            if (!transformPropertyInternal(
                pidlDocument,
                propertyName,
                userInputData,
                internal.constants.pidlTransformationTargets.ForSubmit,
                onFail,
                onComplete)) {

                if (onFail) {
                    onFail(Error("The property with property Name:" + propertyName + " does not have valid submit transform in data description"));
                }
                else {
                    throw new Error("The property with property Name:" + propertyName + " does not have valid submit transform in data description");
                }
            }
        },

        // function commerce2.transformPropertyForDisplayByName
        //   This function iterates through the data description and applies display transforms to the property if applicable for submit
        //
        // parameter pidlDocument
        //   This is the object returned by the GET /payment-method-description call
        //
        // parameter propertyName
        //   The name of the property that needs to be transformed
        //
        // parameter userDisplayData
        //   The property bag containing the user display data
        //
        // parameter onFail
        //   This is the callback function that is called for every transformation that fails transformation
        //
        // parameter onComplete
        //   This is the callback function that is called after the transformation is completed on every property.
        //
        transformUserInputPropertyForDisplay: function (pidlDocument, propertyName, userDisplayData, onFail, onComplete) {
            if (!transformPropertyInternal(
                pidlDocument,
                propertyName,
                userDisplayData,
                internal.constants.pidlTransformationTargets.ForDisplay,
                onFail,
                onComplete)) {

                if (onFail) {
                    onFail(Error("The property with property Name:" + propertyName + " does not have valid display transform in data description"));
                }
                else {
                    throw new Error("The property with property Name:" + propertyName + " does not have valid display transform in data description");
                }
            }
        },

        // function commerce2.tokenizeUserInputData
        //   This function takes sensitive pieces of the user's data and replaces them
        //   with tokens returned by the tokenization service.
        //
        // parameter pidlDocument
        //   This is the object returned by GET /payment-method-description call
        //
        // parameter userInputData
        //   This is an object with user's input data in a format specified by the pidlDocument.
        //   Call commerce2.readUserInputData function to get this object.
        //
        // parameter onFail
        //   This is the function called back on errors in this function
        //
        // parameter onComplete
        //   This function is called when all outstanding ajax calls to the tokenization service
        //   have returned.  It is after this callback that the userInputData is ready for further
        //   processing (e.g. to POST /payment-instrument).
        //
        // parameter pidlOperation
        //   Use one of the values from commerce2.PIDLOperations to pass to this parameter. The scenario in which this method is 
        //   called has to be sent as this parameter i.e. "Add" / "Update". Depending on the scenario, only certain fields in 
        //   userInputData are tokenized based on pidlDocument.
        //
        // parameter clientInfo
        //   This is an object with client related information such as deviceId and ipAddress.
        //
        tokenizeUserInputData: function (pidlDocument, userInputData, onFail, onComplete, pidlOperation, clientInfo) {
            if (typeof pidlDocument === "undefined") {
                throw new Error("Input parameter 'pidlDocument' in commerce2.tokenizeUserInputData is undefined.");
            }

            if (typeof userInputData === "undefined") {
                throw new Error("Input parameter 'userInputData' in commerce2.tokenizeUserInputData is undefined.");
            }

            if (typeof onFail === "undefined") {
                throw new Error("Input parameter 'onFail' in commerce2.tokenizeUserInputData is undefined.");
            }

            if (typeof onComplete === "undefined") {
                throw new Error("Input parameter 'onComplete' in commerce2.tokenizeUserInputData is undefined.");
            }

            if (typeof pidlOperation === "undefined") {
                pidlOperation = SupportedPIDLOperations.Add;
            }
            else {
                validatePidlOperation(pidlOperation);
            }

            var asyncTokenizationRequest = 0;
            var tokenizationCallsPending = 0;
            var tokenizationErrorMessage = "";

            var dataDescription;
            var clientContext = clientInfo;
            if (pidlDocument.length == 1) {
                dataDescription = pidlDocument[0].data_description;
                clientContext = (clientContext === null || clientContext === undefined) ? pidlDocument[0].clientContext : clientContext;
            } else if (pidlDocument.length > 1) {
                dataDescription = getDataDescriptionFromPidlDocumentAndUserInputData(pidlDocument, userInputData);
                clientContext = (clientContext === null || clientContext === undefined) ? getClientContextFromPidlDocumentAndUserInputData(pidlDocument, userInputData) : clientContext;
            }

            var tokenizationServiceEndPoint = internal.config.tokenizationEndpoint;
            for (var propertyName in dataDescription) {
                if (Object.prototype.toString.call(dataDescription[propertyName]) === '[object Array]') {
                    tokenizationCallsPending++;
                    asyncTokenizationRequest += commerce2.tokenizeUserInputData(dataDescription[propertyName], userInputData[propertyName],
                        function (e) {
                            onFail(e);
                        },
                        function () {
                            tokenizationCallsPending--;
                            if (tokenizationCallsPending === 0) {
                                if (asyncTokenizationRequest > 0) {
                                    onComplete();
                                }
                            }
                        }, pidlOperation, clientContext);
                }
                else {
                    if (pidlOperation === SupportedPIDLOperations.Update &&
                        (dataDescription[propertyName].hasOwnProperty("is_updatable") === false || dataDescription[propertyName]["is_updatable"] === false)) {
                        continue;
                    }

                    var propertyDescription = dataDescription[propertyName];
                    if (propertyDescription.hasOwnProperty("token_set")) {

                        if (userInputData.hasOwnProperty(propertyName)) {
                            (function (propertyName) {
                                tokenizationCallsPending++;
                                asyncTokenizationRequest++;

                                if (propertyDescription["token_set"] === "paypalEncryption") {
                                    var scriptDownloadPromise;
                                    if (typeof PayPalCrypto !== "undefined") {
                                        scriptDownloadPromise = $.Deferred().resolve().promise();
                                    }
                                    else {
                                        // TODO: Put url in pidl doc
                                        scriptDownloadPromise = $.getScript("https://buy.microsoft-int.com/PCSV2/Scripts/PayPalEncrypt.js").then(
                                            function () {
                                                $.ajaxSetup({ cache: false });
                                            },
                                            function () {
                                                $.ajaxSetup({ cache: false });
                                                throw new Error("Fail to download paypal encryption script file");
                                            });
                                    }

                                    var tokenizePropertyDescription = propertyDescription;
                                    $.when(scriptDownloadPromise).then(function () {
                                        if (typeof PayPalCrypto === "undefined") {
                                            throw new Error("Fail to initialize PayPal encryption instance");
                                        }

                                        PayPalCrypto.Encrypt(tokenizePropertyDescription, clientContext, userInputData[propertyName], function (encryptedValue) {
                                            userInputData[propertyName] = btoa(String.fromCharCode.apply(null, encryptedValue));
                                            tokenizationCallsPending--;
                                            if (tokenizationCallsPending === 0) {
                                                if (tokenizationErrorMessage === "") {
                                                    onComplete();
                                                } else {
                                                    onFail(new Error(tokenizationErrorMessage))
                                                }
                                            }
                                        })});
                                } else {
                                    $.ajax({
                                        url: tokenizationServiceEndPoint + dataDescription[propertyName]["token_set"] + '/getToken',
                                        type: "POST",
                                        dataType: "json",
                                        contentType: "application/json",
                                        data: '{ "data": "' + userInputData[propertyName] + '" }',
                                        success: function (tokenData, textStatus, jqXHR) {
                                            userInputData[propertyName] = tokenData.data;
                                            tokenizationCallsPending--;
                                            if (tokenizationCallsPending === 0) {
                                                if (tokenizationErrorMessage === "") {
                                                    onComplete();
                                                } else {
                                                    onFail(new Error(tokenizationErrorMessage))
                                                }
                                            }
                                        },
                                        error: function (jqxhr, textStatus, error) {
                                            tokenizationErrorMessage += "Tokenizing " + dataDescription[propertyName]["display_name"] + " failed with error message: " + textStatus + "\n";
                                            tokenizationCallsPending--;
                                            if (tokenizationCallsPending === 0) {
                                                onFail(new Error(tokenizationErrorMessage))
                                            }
                                        }
                                    });
                                }
                            })(propertyName);
                        } else {
                            if (propertyDescription.hasOwnProperty("is_optional") === false || propertyDescription["is_optional"] === false) {
                                throw new Error("DataDescription contains a required field '" + propertyName + "' which is missing in the userInputData passed in.");
                            }
                        }
                    }
                }
            }
            if (tokenizationCallsPending === 0) {
                onComplete();
            }

            return asyncTokenizationRequest;
        },

        // enum commerce2.PIDLOperations
        //   This enum provides the supported PIDL Operations which can be passed to some of the functions 
        //   in commerce2.js to provide the scenario in which the functions are called.
        //
        PIDLOperations: SupportedPIDLOperations
    }
}();

var dbconfig = {}

dbconfig.host = process.env.HOST || "https://payments-acs-emulator.documents.azure.com:443/";
dbconfig.databaseId = "acs-emulator";
dbconfig.collectionId = "acs-emulator";

module.exports = dbconfig;
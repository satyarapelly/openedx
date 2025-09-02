var akvConfig = {}

akvConfig.akv_url = process.env.AKV_URL || "https://payments-acs-emulator-kv.vault.azure.net/";

module.exports = akvConfig;
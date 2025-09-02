// <copyright file="CultureLanguageGenerator.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.CultureLanguageTransformationCSVGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    internal class CultureLanguageGenerator
    {
        private const string OutputCSVName = "CountryInformation.csv";

        private readonly XmlDocument countryInformationDocument;
        private readonly XmlDocument countryInformationConfigDocument;
        private readonly XmlNamespaceManager countryInformationConfigNSManager;
        private readonly string outputGeneratedCSVFullPath;

        internal CultureLanguageGenerator(string countryInformationFullPath, string countryInformationConfigFullPath)
        {
            Console.WriteLine("Input Xml Files:");
            Console.WriteLine("CountryInformationPath:{0}", countryInformationFullPath);
            Console.WriteLine("CountryInformationConfigPath:{0}", countryInformationConfigFullPath);
            Console.WriteLine("\nReading the files....\n");

            countryInformationFullPath = File.Exists(countryInformationFullPath) ? countryInformationFullPath : Path.Combine(Directory.GetCurrentDirectory(), countryInformationFullPath);
            countryInformationDocument = new XmlDocument();
            countryInformationDocument.Load(countryInformationFullPath);

            countryInformationConfigFullPath = File.Exists(countryInformationConfigFullPath) ? countryInformationConfigFullPath : Path.Combine(Directory.GetCurrentDirectory(), countryInformationConfigFullPath);
            countryInformationConfigDocument = new XmlDocument();
            countryInformationConfigDocument.Load(countryInformationConfigFullPath);

            countryInformationConfigNSManager = new XmlNamespaceManager(countryInformationConfigDocument.NameTable);
            countryInformationConfigNSManager.AddNamespace("ns", countryInformationConfigDocument.DocumentElement.Attributes["xmlns"].Value);

            outputGeneratedCSVFullPath = Path.Combine(Directory.GetCurrentDirectory(), OutputCSVName);
        }

        internal void GenerateCultureAndLanguageData()
        {
            Console.WriteLine("Parsing details for culture and language....\n");
            StringBuilder csvCultureContent = new StringBuilder();
            StringBuilder csvLanguageContent = new StringBuilder();

            XmlNodeList countriesInformation = countryInformationDocument.SelectNodes("/ArrayOfCountryInformation/CountryInformation");
            if (countriesInformation != null)
            {
                foreach (XmlNode countryInformationNode in countriesInformation)
                {
                    string countryName = countryInformationNode?.Attributes["Iso2Code"]?.Value;
                    string defaultCulture = countryInformationNode?.SelectSingleNode("DefaultCulture")?.InnerText;
                    string defaultLanguage = "en";

                    if (string.IsNullOrEmpty(countryName))
                    {
                        throw new CultureLanguageGeneratorException("Country Iso2Code is not expected to be empty");
                    }

                    if (string.IsNullOrEmpty(defaultCulture))
                    {
                        throw new CultureLanguageGeneratorException("Country DefaultCulture is not expected to be empty");
                    }

                    // Get all the supported cultures for the country
                    List<string> countrySupportedCultures = ParsePropertySupportedItems(countryInformationNode, "SupportedCultures", "Culture");

                    string cultureRegex = $"^(?!{string.Join("$|", countrySupportedCultures)}).*".Replace(")", "$)");
                    csvCultureContent.AppendLine($"profile_employee_culture,{countryName},forSubmit,regex,,\"{cultureRegex}\",{defaultCulture}");

                    // Get all the supported languages for the country
                    List<string> countrySupportedLanguages = ParsePropertySupportedItems(countryInformationNode, "SupportedLanguages", "Language");

                    if (!countrySupportedLanguages.Contains("en"))
                    {
                        Console.WriteLine($"\nCountry: {countryName} doesn't support {defaultLanguage} langauge\n");
                        defaultLanguage = countrySupportedLanguages.First();
                    }

                    string languageRegex = $"^(?!{string.Join("$|", countrySupportedLanguages)}).*".Replace(")", "$)");
                    csvLanguageContent.AppendLine($"profile_employee_language,{countryName},forSubmit,regex,,\"{languageRegex}\",{defaultLanguage}");
                }

                File.WriteAllText(outputGeneratedCSVFullPath, csvCultureContent.ToString());
                File.AppendAllText(outputGeneratedCSVFullPath, csvLanguageContent.ToString());
            }
            else
            {
                throw new CultureLanguageGeneratorException("Countries information is not found in CountryInformation Xml");
            }

            Console.WriteLine("\n\nProcessing completed");
            Console.WriteLine("\nOutput CSV File Path:{0}", outputGeneratedCSVFullPath);
        }

        private List<string> ParsePropertySupportedItems(XmlNode propertyParentNode, string propertyNodeName, string propertyName)
        {
            List<string> propertySupportedItems = new List<string>();

            foreach (XmlNode propertyNode in propertyParentNode.SelectSingleNode(propertyNodeName))
            {
                // If node type for child of supportedItems is propertyName. e.g. culture or language
                // else if node type for child of supportedItems is text having reference in countryInformationConfig
                if (string.Equals(propertyNode.Name, propertyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!propertySupportedItems.Contains(propertyNode.InnerText))
                    {
                        propertySupportedItems.Add(propertyNode.InnerText);
                    }
                }
                else if (propertyNode.Name == "#text")
                {
                    int startIndex = propertyNode.InnerText.IndexOf("[%") + 2;
                    int endIndex = propertyNode.InnerText.IndexOf("%]");
                    string configDocumentNodeName = propertyNode.InnerText.Substring(startIndex, endIndex - startIndex);

                    // Search and select the referenced property node from the countryInformationConfig
                    XmlNode extraSupportedItems = countryInformationConfigDocument.SelectSingleNode($"//ns:property[@name='{configDocumentNodeName}']", countryInformationConfigNSManager);

                    foreach (XmlNode extraSupportedItem in extraSupportedItems)
                    {
                        if (!string.IsNullOrEmpty(extraSupportedItem.InnerText))
                        {
                            XmlAttribute conditionAttribute = extraSupportedItem.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => string.Equals(a.Name, "condition", StringComparison.OrdinalIgnoreCase) && string.Equals(a.InnerText, "env.name = 'PROD'", StringComparison.OrdinalIgnoreCase));

                            if (conditionAttribute != null || extraSupportedItem.Attributes.Count == 0)
                            {
                                string supportedValue = XElement.Parse(extraSupportedItem.InnerText)?.Value;

                                if (!propertySupportedItems.Contains(supportedValue))
                                {
                                    propertySupportedItems.Add(supportedValue);
                                }

                                Console.WriteLine($"Extra supported item '{supportedValue}' found for {propertyName} in CountryInformationConfig");
                            }
                        }
                    }                        
                }
                else
                {
                    throw new CultureLanguageGeneratorException($"Unexpected {propertyName} property Node type");
                }
            }

            // Including lower case & UPPER CASE varients
            string[] propertySupportedItemsCopy = propertySupportedItems.ToArray();
            foreach (string supportedItem in propertySupportedItemsCopy)
            {
                if (!propertySupportedItems.Contains(supportedItem.ToLower()))
                {
                    propertySupportedItems.Add(supportedItem.ToLower());
                }

                if (!propertySupportedItems.Contains(supportedItem.ToUpper()))
                {
                    propertySupportedItems.Add(supportedItem.ToUpper());
                }
            }

            return propertySupportedItems;
        }
    }
}

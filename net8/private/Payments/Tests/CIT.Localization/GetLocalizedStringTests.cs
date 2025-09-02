// <copyright file="GetLocalizedStringTests.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace CIT.Localization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.VisualBasic.FileIO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetLocalizedStringTests
    {
        LocalizationRepository localizationRepository = LocalizationRepository.Instance;

        [TestMethod]
        public void GetExpectedLocalizedString()
        {
            string localizedString = localizationRepository.GetLocalizedString("Credit card number is required.", "es");
            Assert.AreEqual("El número de la tarjeta de crédito es obligatorio.", localizedString);
        }

        [TestMethod]
        public void LocalizedStringIsEnglishBecauseLanguageCodeIsInvalid()
        {
            string localizedString = localizationRepository.GetLocalizedString("Credit card number is required.", "qq");
            Assert.AreEqual("Credit card number is required.", localizedString);
        }

        [TestMethod]
        public void GetDisplayHintsAndErrorMessagesToCheckLocalizationFiles()
        {
            List<string> allFiles = new List<string>();
            List<string> allStrings = new List<string>();
            List<List<List<string>>> allTablesList = new List<List<List<string>>>();
            string[] searchPatterns = new string[] { "PropertyDisplayHints.csv", "PropertyErrorMessages.csv", "DisplayStrings.csv" };

            foreach (string searchPattern in searchPatterns)
            {
                allFiles.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, searchPattern, System.IO.SearchOption.AllDirectories));
            }

            Dictionary<string, List<string>> columnMappingList = new Dictionary<string, List<string>>
            {
                {
                     "PropertyDisplayHints.csv", new List<string> {"DisplayName", "DisplayContent", "DisplaySelectionText"}
                },
                {
                    "PropertyErrorMessages.csv", new List<string> {  "DefaultErrorMessage", "ErrorMessage" }
                },
                {
                     "DisplayStrings.csv", new List<string> {  "Value" }
                }
            };

            foreach (string file in allFiles)
            {
                var columns = columnMappingList.Where(c => c.Key.Equals(Path.GetFileName(file))).First().Value;
                var table = GetTableFromCSV(file, columns);
                allTablesList.Add(table);
            }
            
            foreach (List<List<string>> table in allTablesList)
            {
                foreach (List<string> stringList in table)
                {
                    foreach (string str in stringList)
                    {
                        if (!allStrings.Contains(str))
                        {
                            allStrings.Add(str);
                        }
                    }
                }
            }
            
            List<Dictionary<string, List<string>>> untranslatedValuesList = AddUntranslatedValuesToList(allStrings);

            CompareCSVFileWithCurrentStringList(untranslatedValuesList);

        }

        private List<Dictionary<string, List<string>>> AddUntranslatedValuesToList(List<String> stringList)
        {
            List<Dictionary<string, List<string>>> untranslatedValuesList = new List<Dictionary<string, List<string>>>();
            string[] languageCodes = new string[] { "af", "am", "ar", "as", "az-cyrl", "az-latn", "az", "be", "bg", "bn-BD", "bn-IN", "bn", "br", "bs-cyrl", "bs-latn", "bs", "ca-es-valencia", "ca", "CS", "cy", "DA", "DE", "EL", "es-MX", "ES", "et", "eu", "fa", "Fl", "Fil", "fr-CA", "FR", "GA", "gd", "gl", "gu", "ha-latn", "HE", "hi", "hr", "hu", "hy", "id", "ig", "is", "IT", "iu-latn", "ja-ploc", "JA", "ka", "kk", "km", "kn", "KO", "kok", "ku-arab", "ky", "lb", "lo", "lt", "lv", "mi", "mk", "ml", "mn-cyrl", "mn", "mr", "ms", "mt", "NB", "ne", "nl", "nn", "no", "nso", "or", "pa-arab", "pa", "PL", "prs", "PT-BR", "pt-PT", "PT", "qps-ploc", "qps-plocm", "quc", "qut", "quz", "ro", "ru", "Rw", "sd-arab-pk", "sd-Arab", "si", "sw", "ta", "te", "tg-Cyrl", "tg", "TH", "ti", "tk", "tn", "TR", "tt", "ug", "uk", "ur", "uz-cyrl", "uz-latn", "vi", "wo", "xh", "yo" };

            foreach (string str in stringList)
            {
                Dictionary<string, List<string>> stringDictionary = new Dictionary<string, List<string>>();
                stringDictionary.Add(str, new List<string>());
                foreach (string lang in languageCodes)
                {                
                    string valueAfterLocalization = localizationRepository.GetLocalizedString(str, lang);

                    if (str.Equals(valueAfterLocalization))
                    {
                        stringDictionary[str].Add(lang);   
                    }
                }
                untranslatedValuesList.Add(stringDictionary);
            }
            return untranslatedValuesList;
        }

        private List<List<string>> GetTableFromCSV(string csvFileName, List<string> columns)
        {
            List<List<string>> table = new List<List<string>>();
            
            using (TextFieldParser parser = new TextFieldParser(csvFileName))
            {
                parser.TrimWhiteSpace = false;
                parser.Delimiters = new string[] { "," };
                parser.HasFieldsEnclosedInQuotes = true;
                Dictionary<string, int> columnMapping = new Dictionary<string, int>();

                // Read first line to get headers
                string[] headers = parser.ReadFields();

                int headerindex = 0;
                foreach (string header in headers)
                {
                    columnMapping.Add(header, headerindex);
                    headerindex++;
                }
                
                while (!parser.EndOfData)
                {
                    string[] cells = null;
                    string lineThatNeedsParsing = null;
                    try
                    {
                        cells = parser.ReadFields();
                    }
                    catch(MalformedLineException ex)
                    {
                        string message = ex.Message;
                        lineThatNeedsParsing = parser.ReadLine();
                        cells = lineThatNeedsParsing.Split(',');
                    }

                    List<string> row = new List<string>();
                    foreach (string column in columns)
                    {
                        int index = 0;
                        try
                        {
                            index = columnMapping[column];
                        }
                        catch (NullReferenceException ex)
                        {
                            string message = ex.Message;
                            throw new NullReferenceException(string.Format("The column {0} was not found in {1}", column, columns));
                        }
                        
                        var value = cells[index];
                        if (!value.Equals(string.Empty))
                        {
                            row.Add(value); 
                        }
                    }
                    if (row.Count() > 0)
                    {
                        table.Add(row);
                    }
                }
            }
            return table;
        }

        private void CompareCSVFileWithCurrentStringList(List<Dictionary<string, List<string>>> untranslatedValuesList)
        {
            List<string> columns = new List<string> {"UntranslatedValue", "Languages" };
            List<List<string>> knownValuesTable = GetTableFromCSV("TestData/KnownUntranslatedValues.csv", columns);
            HashSet<string> knownValueHashSet = new HashSet<string>();
            HashSet<string> currentHashSet = new HashSet<string>(); 

            foreach (List<string> knownValuesRow in knownValuesTable)
            {
                foreach (string knownValue in knownValuesRow)
                {
                    if (!knownValueHashSet.Contains(knownValue))
                    {
                        knownValueHashSet.Add(knownValue);
                    }
                }
            }
            
            foreach (Dictionary<string, List<string>> currentValues in untranslatedValuesList)
            {
                if (!currentHashSet.Contains(currentValues.Keys.First()))
                {
                    currentHashSet.Add(currentValues.Keys.First());
                }
            }

            List<string> parsingQuirks = new List<string>
            {
                "The deposit will include \"Microsoft\" in the description. Note the deposit amount, then:",
                "2. When you see your Bank account, select \"Verify\".",
                "To verify your bank account, we made a small deposit. The description includes \"Microsoft\".",
                "By signing this mandate form, you authorize (A) Microsoft Payments Ltd. (\"Microsoft\") to send " +
                "instructions to your bank to debit your account and (B) your bank to debit your account in " +
                "accordance with the instructions from Microsoft Payments Ltd. (\"Microsoft\"). As part of your "+
                "rights, you are entitled to a refund from your bank under the terms and conditions of your "+
                "agreement with your bank. A refund must be claimed within 8 weeks starting from the date on "+
                "which your account was debited. Your rights are explained in a statement that you can obtain from your bank."
            };

            HashSet<string> exceptionsHashSet = new HashSet<string>();
            foreach (string currentValue in currentHashSet)
            {
                if (!knownValueHashSet.Contains(currentValue))
                {
                    exceptionsHashSet.Add(currentValue);
                }
            }
            foreach (string quirk in parsingQuirks)
            {
                if (exceptionsHashSet.Contains(quirk))
                {
                    exceptionsHashSet.Remove(quirk);
                }
            }

            if (exceptionsHashSet.Count() > 0)
            {
                StringBuilder sb = new StringBuilder();

                foreach (string exception in exceptionsHashSet)
                {
                    sb.Append(exception);
                    sb.Append(" ,");
                    var languagesListMatch = untranslatedValuesList.Where(p => p.ContainsKey(exception)).FirstOrDefault().Values.ToList();
                    if (languagesListMatch != null)
                    {
                        sb.Append("Languages not found for that string: ");
                        foreach (var language in languagesListMatch.FirstOrDefault())
                        {
                            sb.Append(language);
                            sb.Append(",");
                        }
                    }
                }
                //Remove last comma
                sb.Remove(sb.Length-1, 1);

                throw new Exception("The following strings were not expected: " + sb.ToString());

            }

        }
    }
}


// <copyright file="Program.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.CultureLanguageTransformationCSVGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Either update the full path for files or copy the xml files to folder
            // private\Payments\Tools\CultureLanguageTransformationCSVGenerator\bin\Debug after building project
            const string CountryInformationFullPath = "CountryInformation.xml";
            const string CountryInformationConfigFullPath = "CountryInformationConfig.xml";

            CultureLanguageGenerator cultureLanguageGenerator = new CultureLanguageGenerator(CountryInformationFullPath, CountryInformationConfigFullPath);
            cultureLanguageGenerator.GenerateCultureAndLanguageData();
        }
    }
}

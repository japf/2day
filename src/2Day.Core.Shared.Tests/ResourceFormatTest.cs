using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Chartreuse.Today.Core.Shared.Tests.Localization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chartreuse.Today.Core.Shared.Tests
{
    [TestClass]
    public class ResourceFormatTest
    {
        private const string SolutionFilename = "VTDLA.sln";
        private const string SourceFolderPath = @"src\2Day\src";

        [TestMethod]
        public void Resources_string_format()
        {            
            var localizationHelper = new LocalizationHelper();

            string location = Path.GetDirectoryName(typeof(LocalizationHelper).Assembly.Location);
            bool found = false;
            int attempt = 0;
            while (!found && attempt < 20)
            {
                found = Directory.GetFiles(location).ToList().Any(p => p.Contains(SolutionFilename));
                if (!found)
                {
                    string otherLocation = Path.Combine(location, SourceFolderPath);
                    if (Directory.Exists(otherLocation) && File.Exists(Path.Combine(otherLocation, SolutionFilename)))
                        found = true;
                    else
                        location += @"\..";    
                }
            }

            localizationHelper.ReadResources(location);

            var translations = new List<Translations>();
            foreach (var item in localizationHelper.Items)
            {
                if (item.Key.Contains("en-us"))
                    translations.Insert(0, new Translations { Language = item.Key, Entries = item.Value });
                else
                    translations.Add(new Translations { Language = item.Key, Entries = item.Value });
            }

            Dictionary<string, bool> keyFormats = new Dictionary<string, bool>();
            foreach (var translation in translations)
            {
                foreach (var translationEntry in translation.Entries)
                {
                    bool hasFormat = translationEntry.Value.Contains("{");
                    if (hasFormat)
                        AssertFormatIsValid(translationEntry.Value);

                    if (!keyFormats.ContainsKey(translationEntry.Key))
                    {
                        keyFormats.Add(translationEntry.Key, hasFormat);
                    }
                    else
                    {
                        bool expectedHasFormat = keyFormats[translationEntry.Key];

                        string message = string.Format("Translation of key {0} is {1} expected format {2} current {3}", translationEntry.Key, translationEntry.Value, expectedHasFormat, hasFormat);
                        Assert.AreEqual(expectedHasFormat, hasFormat, message);
                        AssertFormatIsValid(translationEntry.Value);
                    }
                }
            }

        }

        private static void AssertFormatIsValid(string text)
        {
            if (text.Contains("{"))
            {
                Assert.AreEqual(text.Count(c => c == '{'), text.Count(c => c == '}'));
            }
        }

        [DebuggerDisplay("Language: {Language} Count: {Entries.Count}")]
        private class Translations
        {
            public string Language { get; set; }
            public List<TranslationEntry> Entries { get; set; } 
        }
    }
}

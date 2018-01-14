using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Chartreuse.Today.Core.Shared.Tools
{
    public static class SupportedCultures
    {        
        private static readonly List<Tuple<string, string>> languages = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("English", "en-us"),
            new Tuple<string, string>("Chinese", "zh-cn"),
            new Tuple<string, string>("Czech", "cs-cz"),
            new Tuple<string, string>("French", "fr-fr"),
            new Tuple<string, string>("German", "de-de"),
            new Tuple<string, string>("Hungarian", "hu-hu"),
            new Tuple<string, string>("Italian", "it-it"),
            new Tuple<string, string>("Polish", "pl-pl"),
            new Tuple<string, string>("Portuguese Portugal", "pt-pt"),
            new Tuple<string, string>("Portuguese Brasil", "pt-br"),
            new Tuple<string, string>("Russian", "ru-ru"),
            new Tuple<string, string>("Spanish", "es-es"),
            new Tuple<string, string>("Swedish", "sv-se")
        };

        static SupportedCultures()
        {
            string userLanguage = CultureInfo.CurrentUICulture.ToString().ToLower();

            IsCurrentCultureSupported = userLanguage.StartsWith("en")
                        || userLanguage.StartsWith("fr")
                        || userLanguage.StartsWith("de")
                        || userLanguage.StartsWith("it")
                        || userLanguage.StartsWith("pt")
                        || userLanguage.StartsWith("es")
                        || userLanguage.StartsWith("ru")
                        || userLanguage.StartsWith("pl")
                        || userLanguage.StartsWith("cs")
                        || userLanguage.StartsWith("zh")
                        || userLanguage.StartsWith("hu")
                        || userLanguage.StartsWith("sv");

            IsSpeechRecognitionSupported = userLanguage.StartsWith("en")
                        || userLanguage.StartsWith("fr")
                        || userLanguage.StartsWith("de")
                        || userLanguage.StartsWith("it")
                        //|| userLanguage.StartsWith("pt")
                        || userLanguage.StartsWith("es")
                        //|| userLanguage.StartsWith("ru")
                        //|| userLanguage.StartsWith("pl")
                        //|| userLanguage.StartsWith("cs")
                        || userLanguage.StartsWith("zh");
        }

        /// <summary>
        /// Gets a value indicating whether the current culture is supported at the UI level
        /// </summary>
        public static bool IsCurrentCultureSupported { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current culture is supported for speech recognition
        /// </summary>
        public static bool IsSpeechRecognitionSupported { get; private set; }

        public static List<string> Languages
        {
            get { return languages.Select(i => i.Item1).ToList(); }
        }

        public static string GetLanguageNameFromCode(string code)
        {
            var item = languages.FirstOrDefault(i => i.Item2.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (item == null)
                item = languages[0];

            return item.Item1;
        }

        public static string GetLanguageCodeFromName(string name)
        {
            var item = languages.FirstOrDefault(i => i.Item1.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (item == null)
                item = languages[0];

            return item.Item2;
        }
    }
}

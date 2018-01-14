using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Chartreuse.Today.Core.Shared.Tests.Localization
{
    public class LocalizationHelper
    {
        private const string ResXExtension = "*.resx";
        private const string ResWExtension = "*.resw";

        private Dictionary<string, List<TranslationEntry>> items;

        public Dictionary<string, List<TranslationEntry>> Items
        {
            get { return this.items; }
        }

        public void ReadResources(string rootPath)
        {
            if (rootPath == null)
                throw new ArgumentNullException("rootPath");

            this.items = new Dictionary<string, List<TranslationEntry>>();

            foreach (var path in GetFiles(rootPath, ResXExtension))
                this.ReadResourceFile(path);

            foreach (var path in GetFiles(rootPath, ResWExtension))
                this.ReadResourceFile(path);
        }

        private static IEnumerable<string> GetFiles(string path, string extensions)
        {
            return Directory.GetFiles(path, extensions, SearchOption.AllDirectories).Where(p => !p.Contains("\\obj\\"));
        }

        private void ReadResourceFile(string path)
        {
            // get the language
            string filename = Path.GetFileName(path);
            var parts = filename.Split('.');
            if (parts.Length != 2 && parts.Length != 3)
                throw new NotSupportedException();

            string language = "en-us";
            if (parts.Length == 3)
            {
                language = parts[1].ToLower();
            }
            else
            {
                string directory = Path.GetDirectoryName(path).Split(new[] { '\\'}).Last();
                language = directory.ToLower();
            }

            if (!language.Contains("-"))
                return;

            if (!this.Items.ContainsKey(language))
                this.Items.Add(language, new List<TranslationEntry>());

            var translations = this.Items[language];

            XElement xelement = XElement.Load(path);
            foreach (var data in xelement.Elements("data"))
            {
                string key = data.Attribute("name").Value;
                string value = data.Value;
                value = value.Trim().Trim('\n');

                translations.Add(new TranslationEntry { Key = key, Value = value });
            }
        }
    }
}

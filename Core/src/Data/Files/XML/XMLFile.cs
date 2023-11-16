using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LabFusion.Data
{
    public sealed class XMLFile
    {
        private readonly string _fileName;

        private readonly string _rootName;

        private readonly string _filePath;

        public XMLFile(string fileName, string rootName)
        {
            _fileName = fileName;
            _rootName = rootName;
            _filePath = PersistentData.GetPath(_fileName);
        }

        public void ReadFile(Action<XDocument> onRead)
        {
            XDocument InstantiateDefault(string verb = "missing")
            {
                FusionLogger.Log($"{_rootName} was {verb}, created it!", ConsoleColor.DarkCyan);
                var defaultDocument = CreateDefault();
                File.WriteAllText(_filePath, defaultDocument.ToString());

                return defaultDocument;
            }

            XDocument document = null;

            try
            {
                if (File.Exists(_filePath))
                {
                    FusionLogger.Log($"{_rootName} was found, attempting to read it!", ConsoleColor.DarkCyan);
                    string raw = File.ReadAllText(_filePath);
                    document = XDocument.Parse(raw);

                    if (document.Root.Name != _rootName)
                        throw new ArgumentException($"Xml root wasn't {_rootName}, recreating the xml...");
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException($"parsing {_rootName}", e);
                document = InstantiateDefault("malformed");
            }

            document ??= InstantiateDefault();

            onRead(document);
        }

        public void WriteFile(IList<object> entries)
        {
            var baseDoc = CreateDefault();

            for (var i = 0; i < entries.Count; i++)
            {
                baseDoc.Root.Add(entries[i]);
            }

            File.WriteAllText(_filePath, baseDoc.ToString());
        }

        private XDocument CreateDefault()
        {
            XDocument document = new();
            document.Add(new XElement(_rootName));
            return document;
        }
    }
}

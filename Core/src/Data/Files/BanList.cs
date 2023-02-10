using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Utilities;
using LabFusion.Representation;
using LabFusion.Network;

namespace LabFusion.Data
{
    public static class BanList
    {
        private const string _rootName = "BanList";
        private const string _fileName = "banlist.xml";

        private const string _elementName = "Ban";
        private const string _idName = "id";
        private const string _userName = "username";
        private const string _reasonName = "reason";

        private static List<Tuple<ulong, string, string>> _bannedUsers = new List<Tuple<ulong, string, string>>();
        private static string _banListPath;

        public static IReadOnlyList<Tuple<ulong, string, string>> BannedUsers => _bannedUsers;

        public static void PullFromFile()
        {
            _bannedUsers.Clear();

            XDocument InstantiateDefault(string verb = "missing")
            {
                FusionLogger.Log($"{_rootName} was {verb}, created it!", ConsoleColor.DarkCyan);
                var defaultDocument = CreateDefault();
                File.WriteAllText(_banListPath, defaultDocument.ToString());

                return defaultDocument;
            }

            XDocument document = null;
            _banListPath = PersistentData.GetPath(_fileName);

            try
            {
                if (File.Exists(_banListPath))
                {
                    FusionLogger.Log($"{_rootName} was found, attempting to read it!", ConsoleColor.DarkCyan);
                    string raw = File.ReadAllText(_banListPath);
                    document = XDocument.Parse(raw);

                    if (document.Root.Name != _rootName)
                        throw new ArgumentException($"Xml root wasn't {_rootName}, recreating the xml...");
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("parsing BanList", e);
                document = InstantiateDefault("malformed");
            }

            if (document == null)
                document = InstantiateDefault();

            if (document != null)
            {
                document.Descendants(_elementName).ForEach((element) => {
                    if (element.TryGetAttribute(_idName, out string rawId) && element.TryGetAttribute(_userName, out string rawUser) && element.TryGetAttribute(_reasonName, out string rawReason))
                    {
                        if (ulong.TryParse(rawId, out ulong id))
                        {
                            _bannedUsers.Add(new Tuple<ulong, string, string>(id, rawUser, rawReason));
                        }
                    }
                });
            }
        }

        private static XDocument CreateDefault()
        {
            XDocument document = new XDocument();
            document.Add(new XElement(_rootName));
            return document;
        }

        private static void PushToFile() {
            var baseDoc = CreateDefault();

            foreach (var tuple in _bannedUsers) {
                XElement entry = new XElement(_elementName);
                entry.SetAttributeValue(_idName, tuple.Item1);
                entry.SetAttributeValue(_userName, tuple.Item2);
                entry.SetAttributeValue(_reasonName, tuple.Item3);

                baseDoc.Root.Add(entry);
            }

            File.WriteAllText(_banListPath, baseDoc.ToString());
        }

        public static void Ban(ulong longId, string username, string reason) {
            var tuple = new Tuple<ulong, string, string>(longId, username, reason);

            foreach (var user in _bannedUsers.ToArray()) {
                if (user.Item1 == longId) {
                    _bannedUsers.Remove(user);
                    break;
                }
            }

            _bannedUsers.Add(tuple);

            PushToFile();
        }

        public static void Pardon(ulong longId) {
            foreach (var user in _bannedUsers.ToArray()) {
                if (user.Item1 == longId) {
                    _bannedUsers.Remove(user);
                    break;
                }
            }

            PushToFile();
        }
    }
}

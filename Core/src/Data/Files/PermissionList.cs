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
    public static class PermissionList
    {
        private const string _rootName = "PermissionList";
        private const string _fileName = "permissionList.xml";

        private const string _elementName = "Permission";
        private const string _idName = "id";
        private const string _userName = "username";
        private const string _levelName = "level";

        private static List<Tuple<ulong, string, PermissionLevel>> _permittedUsers = new List<Tuple<ulong, string, PermissionLevel>>();
        private static string _permissionListPath;

        public static IReadOnlyList<Tuple<ulong, string, PermissionLevel>> PermittedUsers => _permittedUsers;

        public static void PullFromFile()
        {
            XDocument InstantiateDefault(string verb = "missing")
            {
                FusionLogger.Log($"{_rootName} was {verb}, created it!", ConsoleColor.DarkCyan);
                var defaultDocument = CreateDefault();
                File.WriteAllText(_permissionListPath, defaultDocument.ToString());

                return defaultDocument;
            }

            XDocument document = null;
            _permissionListPath = PersistentData.GetPath(_fileName);

            try
            {
                if (File.Exists(_permissionListPath))
                {
                    FusionLogger.Log($"{_rootName} was found, attempting to read it!", ConsoleColor.DarkCyan);
                    string raw = File.ReadAllText(_permissionListPath);
                    document = XDocument.Parse(raw);

                    if (document.Root.Name != _rootName)
                        throw new ArgumentException($"Xml root wasn't {_rootName}, recreating the xml...");
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("parsing PermissionList", e);
                document = InstantiateDefault("malformed");
            }

            if (document == null)
                document = InstantiateDefault();

            if (document != null)
            {
                document.Descendants(_elementName).ForEach((element) => {
                    if (element.TryGetAttribute(_idName, out string rawId) && element.TryGetAttribute(_userName, out string rawUser) && element.TryGetAttribute(_levelName, out string rawLevel))
                    {
                        if (ulong.TryParse(rawId, out ulong id) && Enum.TryParse(rawLevel, out PermissionLevel level))
                        {
                            _permittedUsers.Add(new Tuple<ulong, string, PermissionLevel>(id, rawUser, level));
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

            foreach (var tuple in _permittedUsers) {
                XElement entry = new XElement(_elementName);
                entry.SetAttributeValue(_idName, tuple.Item1);
                entry.SetAttributeValue(_userName, tuple.Item2);
                entry.SetAttributeValue(_levelName, tuple.Item3);

                baseDoc.Root.Add(entry);
            }

            File.WriteAllText(_permissionListPath, baseDoc.ToString());
        }

        public static void SetPermission(ulong longId, string username, PermissionLevel level) {
            var tuple = new Tuple<ulong, string, PermissionLevel>(longId, username, level);

            foreach (var user in _permittedUsers.ToArray()) {
                if (user.Item1 == longId) {
                    _permittedUsers.Remove(user);
                    break;
                }
            }

            _permittedUsers.Add(tuple);

            PushToFile();
        }
    }
}

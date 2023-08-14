using System;
using System.Collections.Generic;
using System.Xml.Linq;

using LabFusion.Extensions;

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

        private static readonly List<Tuple<ulong, string, string>> _bannedUsers = new();

        public static IReadOnlyList<Tuple<ulong, string, string>> BannedUsers => _bannedUsers;

        private static XMLFile _file;

        public static void ReadFile() {
            _bannedUsers.Clear();

            _file = new XMLFile(_fileName, _rootName);
            _file.ReadFile((d) => {
                d.Descendants(_elementName).ForEach((element) => {
                    if (element.TryGetAttribute(_idName, out string rawId) && element.TryGetAttribute(_userName, out string rawUser) && element.TryGetAttribute(_reasonName, out string rawReason))
                    {
                        if (ulong.TryParse(rawId, out ulong id))
                        {
                            _bannedUsers.Add(new Tuple<ulong, string, string>(id, rawUser, rawReason));
                        }
                    }
                });
            });
        }

        private static void WriteFile() {
            List<object> entries = new();

            foreach (var tuple in _bannedUsers) {
                XElement entry = new(_elementName);

                entry.SetAttributeValue(_idName, tuple.Item1);
                entry.SetAttributeValue(_userName, tuple.Item2);
                entry.SetAttributeValue(_reasonName, tuple.Item3);

                entries.Add(entry);
            }

            _file.WriteFile(entries);
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

            WriteFile();
        }

        public static void Pardon(ulong longId) {
            foreach (var user in _bannedUsers.ToArray()) {
                if (user.Item1 == longId) {
                    _bannedUsers.Remove(user);
                    break;
                }
            }

            WriteFile();
        }
    }
}
